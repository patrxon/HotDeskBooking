namespace HotDeskBooking.Models
{
    public class Desk
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public bool IsAvailable { get; set; } = true;
        public ICollection<Reservation> Reservations { get; set; }
    }
}
