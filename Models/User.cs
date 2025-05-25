namespace EventBookingWeb.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; } // "admin" or "user"
        public required string Password { get; set; } // Added for password storage
    }
}
