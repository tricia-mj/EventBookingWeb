namespace EventBookingWeb.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int VenueId { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } // pending, approved, rejected

        public User User { get; set; }
        public Venue Venue { get; set; }
    }
}
