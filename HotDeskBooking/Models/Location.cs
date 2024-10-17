namespace HotDeskBooking.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Desk> Desks { get; set; }
    }
}
