namespace HotDeskBooking.Models
{
    public class DeskDto
    {
        public int Id { get; set; }
        public string LocationName { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
