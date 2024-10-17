namespace HotDeskBooking.Models
{
    public class ReservationDto
    {
        public int DeskId { get; set; }
        public DateTime StartDate { get; set; }
        public int DaysOfReservation { get; set; }
    }
}
