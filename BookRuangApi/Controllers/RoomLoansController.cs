using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookRuangApi.Models;
using BookRuangApi.DTOs;

namespace BookRuangApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomLoansController : ControllerBase
    {
        private readonly DataContext _context;

        public RoomLoansController(DataContext context)
        {
            _context = context;
        }

        // GET: api/RoomLoans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomLoanResponseDto>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? roomName = null,
            [FromQuery] string? borrowerName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.RoomLoans.AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status.ToLower() == status.ToLower());
            }

            // Filter by room name
            if (!string.IsNullOrEmpty(roomName))
            {
                query = query.Where(r => r.RoomName.ToLower().Contains(roomName.ToLower()));
            }

            // Filter by borrower name
            if (!string.IsNullOrEmpty(borrowerName))
            {
                query = query.Where(r => r.BorrowerName.ToLower().Contains(borrowerName.ToLower()));
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(r => r.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.Date <= endDate.Value);
            }

            var roomLoans = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var response = roomLoans.Select(r => MapToResponseDto(r)).ToList();

            return Ok(response);
        }

        // GET: api/RoomLoans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomLoanResponseDto>> GetById(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            return Ok(MapToResponseDto(roomLoan));
        }

        // GET: api/RoomLoans/status/pending
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<RoomLoanResponseDto>>> GetByStatus(string status)
        {
            if (!RoomLoanStatus.IsValid(status))
            {
                return BadRequest(new { message = "Status tidak valid. Gunakan: Pending, Approved, Rejected, atau Cancelled" });
            }

            var roomLoans = await _context.RoomLoans
                .Where(r => r.Status.ToLower() == status.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var response = roomLoans.Select(r => MapToResponseDto(r)).ToList();

            return Ok(response);
        }

        // GET: api/RoomLoans/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            var total = await _context.RoomLoans.CountAsync();
            var pending = await _context.RoomLoans.CountAsync(r => r.Status == RoomLoanStatus.Pending);
            var approved = await _context.RoomLoans.CountAsync(r => r.Status == RoomLoanStatus.Approved);
            var rejected = await _context.RoomLoans.CountAsync(r => r.Status == RoomLoanStatus.Rejected);

            return Ok(new
            {
                total,
                pending,
                approved,
                rejected,
                cancelled = await _context.RoomLoans.CountAsync(r => r.Status == RoomLoanStatus.Cancelled)
            });
        }

        // POST: api/RoomLoans
        [HttpPost]
        public async Task<ActionResult<RoomLoanResponseDto>> Create(CreateRoomLoanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validasi waktu
            if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime >= dto.EndTime)
            {
                return BadRequest(new { message = "Waktu selesai harus lebih besar dari waktu mulai" });
            }

            // Cek konflik jadwal (optional tapi bagus)
            if (dto.StartTime.HasValue && dto.EndTime.HasValue)
            {
                var hasConflict = await _context.RoomLoans
                    .AnyAsync(r => r.RoomName.ToLower() == dto.RoomName.ToLower()
                                && r.Status == RoomLoanStatus.Approved
                                && r.StartTime < dto.EndTime
                                && r.EndTime > dto.StartTime);

                if (hasConflict)
                {
                    return BadRequest(new { message = "Ruangan sudah dibooking pada waktu tersebut" });
                }
            }

            var roomLoan = new RoomLoan
            {
                BorrowerName = dto.BorrowerName,
                RoomName = dto.RoomName,
                Purpose = dto.Purpose,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = RoomLoanStatus.Pending,
                Date = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.RoomLoans.Add(roomLoan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = roomLoan.Id }, MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateRoomLoanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            // Hanya bisa update jika status masih pending
            if (roomLoan.Status != RoomLoanStatus.Pending)
            {
                return BadRequest(new { message = "Hanya peminjaman dengan status Pending yang bisa diupdate" });
            }

            // Validasi waktu
            if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime >= dto.EndTime)
            {
                return BadRequest(new { message = "Waktu selesai harus lebih besar dari waktu mulai" });
            }

            roomLoan.BorrowerName = dto.BorrowerName;
            roomLoan.RoomName = dto.RoomName;
            roomLoan.Purpose = dto.Purpose;
            roomLoan.StartTime = dto.StartTime;
            roomLoan.EndTime = dto.EndTime;
            roomLoan.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5/approve
        [HttpPut("{id}/approve")]
        public async Task<ActionResult<RoomLoanResponseDto>> Approve(int id, UpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            if (roomLoan.Status != RoomLoanStatus.Pending)
            {
                return BadRequest(new { message = "Hanya peminjaman dengan status Pending yang bisa diapprove" });
            }

            // Cek konflik jadwal sebelum approve
            if (roomLoan.StartTime.HasValue && roomLoan.EndTime.HasValue)
            {
                var hasConflict = await _context.RoomLoans
                    .AnyAsync(r => r.Id != id
                                && r.RoomName.ToLower() == roomLoan.RoomName.ToLower()
                                && r.Status == RoomLoanStatus.Approved
                                && r.StartTime < roomLoan.EndTime
                                && r.EndTime > roomLoan.StartTime);

                if (hasConflict)
                {
                    return BadRequest(new { message = "Ruangan sudah dibooking pada waktu tersebut oleh peminjaman lain" });
                }
            }

            roomLoan.Status = RoomLoanStatus.Approved;
            roomLoan.ApprovedBy = dto.UpdatedBy;
            roomLoan.ApprovedAt = DateTime.Now;
            roomLoan.Notes = dto.Notes;
            roomLoan.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5/reject
        [HttpPut("{id}/reject")]
        public async Task<ActionResult<RoomLoanResponseDto>> Reject(int id, UpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            if (roomLoan.Status != RoomLoanStatus.Pending)
            {
                return BadRequest(new { message = "Hanya peminjaman dengan status Pending yang bisa direject" });
            }

            roomLoan.Status = RoomLoanStatus.Rejected;
            roomLoan.RejectedBy = dto.UpdatedBy;
            roomLoan.RejectedAt = DateTime.Now;
            roomLoan.Notes = dto.Notes;
            roomLoan.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<RoomLoanResponseDto>> Cancel(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            if (roomLoan.Status == RoomLoanStatus.Cancelled)
            {
                return BadRequest(new { message = "Peminjaman sudah dibatalkan" });
            }

            roomLoan.Status = RoomLoanStatus.Cancelled;
            roomLoan.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(MapToResponseDto(roomLoan));
        }

        // DELETE: api/RoomLoans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            _context.RoomLoans.Remove(roomLoan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Peminjaman berhasil dihapus" });
        }

        // Helper method untuk mapping
        private RoomLoanResponseDto MapToResponseDto(RoomLoan roomLoan)
        {
            return new RoomLoanResponseDto
            {
                Id = roomLoan.Id,
                BorrowerName = roomLoan.BorrowerName,
                RoomName = roomLoan.RoomName,
                Purpose = roomLoan.Purpose,
                Status = roomLoan.Status,
                Date = roomLoan.Date,
                StartTime = roomLoan.StartTime,
                EndTime = roomLoan.EndTime,
                ApprovedBy = roomLoan.ApprovedBy,
                ApprovedAt = roomLoan.ApprovedAt,
                RejectedBy = roomLoan.RejectedBy,
                RejectedAt = roomLoan.RejectedAt,
                Notes = roomLoan.Notes,
                CreatedAt = roomLoan.CreatedAt,
                UpdatedAt = roomLoan.UpdatedAt
            };
        }
    }
}