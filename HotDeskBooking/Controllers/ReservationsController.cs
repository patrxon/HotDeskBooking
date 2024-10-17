using HotDeskBooking.Data;
using HotDeskBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotDeskBooking.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; 

        public ReservationsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var desk = await _context.Desks.FirstOrDefaultAsync(d => d.Id == reservationDto.DeskId);
            var endDate = reservationDto.StartDate.AddDays(reservationDto.DaysOfReservation);

            if (userId == null)
                return Unauthorized();
            
            if (desk == null)
                return BadRequest("Desk not found.");

            if (!desk.IsAvailable)
                return BadRequest("Desk is Unavailable.");   

            var isDeskAvailable = !_context.Reservations.Any(r =>
                r.DeskId == reservationDto.DeskId &&
                ((r.StartDate <= endDate && r.EndDate >= endDate) ||
                (r.StartDate <= reservationDto.StartDate && r.EndDate >= reservationDto.StartDate)));

            if (!isDeskAvailable)
                return BadRequest("Desk is reserved.");

            if (reservationDto.DaysOfReservation < 1)
                return BadRequest("Cannot reserve a desk for less then a day.");
           
            if (reservationDto.DaysOfReservation > 7)
                return BadRequest("Cannot reserve a desk for more than 7 days.");
            
            if (!isDeskAvailable)
                return BadRequest("Desk is not available on the specified dates.");
            
            var reservation = new Reservation
            {
                UserId = userId,
                DeskId = reservationDto.DeskId,
                StartDate = reservationDto.StartDate,
                EndDate = endDate
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Reservation successfully created." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations.FindAsync(id);
            var endDate = reservationDto.StartDate.AddDays(reservationDto.DaysOfReservation);

            if (reservation == null || reservation.UserId != userId)
                return Unauthorized();
          
            if ((reservation.StartDate - DateTime.Now).TotalHours < 24)
                return BadRequest("Cannot modify the reservation less than 24 hours before the reservation date.");
            
            var isDeskAvailable = !_context.Reservations.Any(r =>
                r.DeskId == reservationDto.DeskId &&
                r.Id != reservation.Id &&
                ((r.StartDate <= endDate && r.EndDate >= endDate) ||
                (r.StartDate <= reservationDto.StartDate && r.EndDate >= reservationDto.StartDate)));

            if (!isDeskAvailable)
                return BadRequest("Desk is not available on the specified dates.");

            reservation.DeskId = reservationDto.DeskId;
            reservation.StartDate = reservationDto.StartDate;
            reservation.EndDate = endDate;

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Reservation successfully updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null || reservation.UserId != userId)
                return Unauthorized();
            
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Reservation successfully deleted." });
        }

        [HttpGet("available")]
        public IActionResult GetAvailableDesks(DateTime reservationDate, DateTime? reservationEndDate)
        {
            var unavailableDeskIds = _context.Reservations
                .Where(r => r.StartDate >= reservationDate &&
                            (reservationEndDate == null || r.StartDate <= reservationEndDate))
                .Select(r => r.DeskId)
                .Distinct()
                .ToList();

            var availableDesks = _context.Desks
                .Where(d => !unavailableDeskIds.Contains(d.Id) && d.IsAvailable)
                .ToList();

            if (User.IsInRole("Admin"))
                availableDesks = _context.Desks
                    .Include(d => d.Reservations)
                    .Where(d => !unavailableDeskIds.Contains(d.Id) && d.IsAvailable)
                    .ToList();

            return Ok(availableDesks);
        }

        [HttpGet("location")]
        public IActionResult GetDesksInLocation(string locationName)
        {
            var location = _context.Locations.FirstOrDefault(l => l.Name == locationName);

            if(location == null)
                return NotFound();

            var desks = _context.Desks
                .Where(d => d.LocationId == location.Id);

            if (User.IsInRole("Admin")) 
                desks = _context.Desks
                    .Include(d => d.Reservations)
                    .Where(d => d.LocationId == location.Id);   
            
            return Ok(desks);
        }
    }
}