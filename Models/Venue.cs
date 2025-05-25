namespace EventBookingWeb.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        public required string VenueName { get; set; }
        public required string Location { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
    }
}
