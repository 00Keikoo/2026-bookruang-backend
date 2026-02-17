using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookRuangApi.Models;
using BookRuangApi.DTOs;
using System.Security.Claims;

namespace BookRuangApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            [FromQuery] string? borrowerName = null)
        {
            var query = _context.RoomLoans.AsQueryable();

            // Kalau role User, hanya tampilkan milik dia sendiri
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            if (role == UserRoles.User)
            {
                query = query.Where(r => r.BorrowerName == fullName);
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrEmpty(roomName))
                query = query.Where(r => r.RoomName.ToLower().Contains(roomName.ToLower()));

            if (!string.IsNullOrEmpty(borrowerName) && role == UserRoles.Admin)
                query = query.Where(r => r.BorrowerName.ToLower().Contains(borrowerName.ToLower()));

            var roomLoans = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(roomLoans.Select(r => MapToResponseDto(r)).ToList());
        }

        // GET: api/RoomLoans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomLoanResponseDto>> GetById(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);
            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            // User hanya bisa lihat miliknya sendiri
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;
            if (role == UserRoles.User && roomLoan.BorrowerName != fullName)
                return Forbid();

            return Ok(MapToResponseDto(roomLoan));
        }

        // GET: api/RoomLoans/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            IQueryable<RoomLoan> query = _context.RoomLoans;

            // User hanya lihat statistik miliknya
            if (role == UserRoles.User)
                query = query.Where(r => r.BorrowerName == fullName);

            return Ok(new
            {
                total    = await query.CountAsync(),
                pending  = await query.CountAsync(r => r.Status == RoomLoanStatus.Pending),
                approved = await query.CountAsync(r => r.Status == RoomLoanStatus.Approved),
                rejected = await query.CountAsync(r => r.Status == RoomLoanStatus.Rejected),
                cancelled = await query.CountAsync(r => r.Status == RoomLoanStatus.Cancelled)
            });
        }

        // POST: api/RoomLoans
        [HttpPost]
        public async Task<ActionResult<RoomLoanResponseDto>> Create(CreateRoomLoanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "Waktu selesai harus lebih besar dari waktu mulai" });

            if (dto.StartTime.HasValue && dto.EndTime.HasValue)
            {
                var hasConflict = await _context.RoomLoans
                    .AnyAsync(r => r.RoomName.ToLower() == dto.RoomName.ToLower()
                                && r.Status == RoomLoanStatus.Approved
                                && r.StartTime < dto.EndTime
                                && r.EndTime > dto.StartTime);
                if (hasConflict)
                    return BadRequest(new { message = "Ruangan sudah dibooking pada waktu tersebut" });
            }

            var roomLoan = new RoomLoan
            {
                BorrowerName = dto.BorrowerName,
                RoomName     = dto.RoomName,
                Purpose      = dto.Purpose,
                StartTime    = dto.StartTime,
                EndTime      = dto.EndTime,
                Status       = RoomLoanStatus.Pending,
                Date         = DateTime.Now,
                CreatedAt    = DateTime.Now
            };

            _context.RoomLoans.Add(roomLoan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = roomLoan.Id }, MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5 — User hanya bisa edit miliknya, Admin bisa semua
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateRoomLoanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roomLoan = await _context.RoomLoans.FindAsync(id);
            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            var role     = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            if (role == UserRoles.User && roomLoan.BorrowerName != fullName)
                return Forbid();

            if (roomLoan.Status != RoomLoanStatus.Pending)
                return BadRequest(new { message = "Hanya peminjaman Pending yang bisa diupdate" });

            if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "Waktu selesai harus lebih besar dari waktu mulai" });

            roomLoan.BorrowerName = dto.BorrowerName;
            roomLoan.RoomName     = dto.RoomName;
            roomLoan.Purpose      = dto.Purpose;
            roomLoan.StartTime    = dto.StartTime;
            roomLoan.EndTime      = dto.EndTime;
            roomLoan.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5/approve — ADMIN ONLY
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomLoanResponseDto>> Approve(int id, UpdateStatusDto dto)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);
            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            if (roomLoan.Status != RoomLoanStatus.Pending)
                return BadRequest(new { message = "Hanya peminjaman Pending yang bisa diapprove" });

            if (roomLoan.StartTime.HasValue && roomLoan.EndTime.HasValue)
            {
                var hasConflict = await _context.RoomLoans
                    .AnyAsync(r => r.Id != id
                                && r.RoomName.ToLower() == roomLoan.RoomName.ToLower()
                                && r.Status == RoomLoanStatus.Approved
                                && r.StartTime < roomLoan.EndTime
                                && r.EndTime > roomLoan.StartTime);
                if (hasConflict)
                    return BadRequest(new { message = "Ruangan sudah dibooking pada waktu tersebut" });
            }

            roomLoan.Status     = RoomLoanStatus.Approved;
            roomLoan.ApprovedBy = dto.UpdatedBy;
            roomLoan.ApprovedAt = DateTime.Now;
            roomLoan.Notes      = dto.Notes;
            roomLoan.UpdatedAt  = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(MapToResponseDto(roomLoan));
        }

        // PUT: api/RoomLoans/5/reject — ADMIN ONLY
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomLoanResponseDto>> Reject(int id, UpdateStatusDto dto)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);
            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            if (roomLoan.Status != RoomLoanStatus.Pending)
                return BadRequest(new { message = "Hanya peminjaman Pending yang bisa direject" });

            roomLoan.Status     = RoomLoanStatus.Rejected;
            roomLoan.RejectedBy = dto.UpdatedBy;
            roomLoan.RejectedAt = DateTime.Now;
            roomLoan.Notes      = dto.Notes;
            roomLoan.UpdatedAt  = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(MapToResponseDto(roomLoan));
        }

        // DELETE: api/RoomLoans/5 — ADMIN ONLY
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);
            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            _context.RoomLoans.Remove(roomLoan);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Peminjaman berhasil dihapus" });
        }

        // PUT: api/RoomLoans/5/cancel — User hanya bisa cancel miliknya
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<RoomLoanResponseDto>> Cancel(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);
            if (roomLoan == null)
                return NotFound(new { message = "Peminjaman tidak ditemukan" });

            var role     = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            if (role == UserRoles.User && roomLoan.BorrowerName != fullName)
                return Forbid();

            if (roomLoan.Status == RoomLoanStatus.Cancelled)
                return BadRequest(new { message = "Peminjaman sudah dibatalkan" });

            roomLoan.Status    = RoomLoanStatus.Cancelled;
            roomLoan.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(MapToResponseDto(roomLoan));
        }

        private RoomLoanResponseDto MapToResponseDto(RoomLoan r) => new RoomLoanResponseDto
        {
            Id           = r.Id,
            BorrowerName = r.BorrowerName,
            RoomName     = r.RoomName,
            Purpose      = r.Purpose,
            Status       = r.Status,
            Date         = r.Date,
            StartTime    = r.StartTime,
            EndTime      = r.EndTime,
            ApprovedBy   = r.ApprovedBy,
            ApprovedAt   = r.ApprovedAt,
            RejectedBy   = r.RejectedBy,
            RejectedAt   = r.RejectedAt,
            Notes        = r.Notes,
            CreatedAt    = r.CreatedAt,
            UpdatedAt    = r.UpdatedAt
        };
    }
}
