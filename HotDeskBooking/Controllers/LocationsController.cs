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
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddLocation(LocationDto locationDto)
        {
            if (_context.Locations.Any(l => l.Name == locationDto.Name))
            {
                return BadRequest("Location already exists.");
            }

            var location = new Location
            {
                Name = locationDto.Name
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return Ok(location);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveLocation(LocationDto locationDto)
        {
            var location = await _context.Locations.Include(l => l.Desks).FirstOrDefaultAsync(l => l.Name == locationDto.Name);

            if (location == null)
            {
                return NotFound();
            }

            if (location.Desks.Any())
            {
                return BadRequest("Cannot remove a location that has desks.");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}