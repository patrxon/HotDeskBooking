namespace HotDeskBooking.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int DeskId { get; set; }
        public string UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
