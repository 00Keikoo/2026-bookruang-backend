using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookRuangApi.Models;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomLoan>>> GetAll()
        {
            return await _context.RoomLoans.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomLoan>> GetById(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound();

            return roomLoan;
        }

        [HttpPost]
        public async Task<ActionResult<RoomLoan>> Create(RoomLoan roomLoan)
        {
            _context.RoomLoans.Add(roomLoan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = roomLoan.Id }, roomLoan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoomLoan roomLoan)
        {
            if (id != roomLoan.Id)
                return BadRequest();

            _context.Entry(roomLoan).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomLoan = await _context.RoomLoans.FindAsync(id);

            if (roomLoan == null)
                return NotFound();

            _context.RoomLoans.Remove(roomLoan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

