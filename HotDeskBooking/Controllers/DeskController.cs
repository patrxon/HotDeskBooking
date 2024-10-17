using HotDeskBooking.Data;
using HotDeskBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotDeskBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DeskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddDesk(DeskDto deskDto)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Name == deskDto.LocationName);

            if (location == null)
            {
                return BadRequest("Location not found.");
            }

            var desk = new Desk
            {
                LocationId = location.Id,
                IsAvailable = deskDto.IsAvailable,
            };

            _context.Desks.Add(desk);
            await _context.SaveChangesAsync();
            return Ok(desk);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDesk(int id)
        {
            var desk = await _context.Desks.Include(d => d.Reservations).FirstOrDefaultAsync(d => d.Id == id);
            if (desk == null)
            {
                return NotFound();
            }

            if (desk.Reservations.Any())
            {
                return BadRequest("Cannot remove a desk that has reservation.");
            }

            _context.Desks.Remove(desk);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}, {available}")]
        public async Task<IActionResult> ChangeDeskAvailability(int id, bool available)
        {
            var desk = await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);
            if (desk == null)
            {
                return NotFound();
            }

            desk.IsAvailable = available;

            _context.Desks.Update(desk);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}