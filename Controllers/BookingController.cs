using Microsoft.AspNetCore.Mvc;
using EventBookingWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBookingWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        // Display all bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Venue)
                .ToListAsync();

            return View(bookings);
        }

        // Show booking form
        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venues.Where(v => v.IsAvailable).ToList();
            ViewBag.Users = _context.Users.ToList(); // temporary until login system
            return View();
        }

        // Submit booking form
        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Check for conflict
            bool hasConflict = _context.Bookings.Any(b =>
                b.VenueId == booking.VenueId &&
                b.Date == booking.Date &&
                b.Status == "approved" &&
                b.StartTime < booking.EndTime &&
                b.EndTime > booking.StartTime);

            if (hasConflict)
            {
                ModelState.AddModelError("", "This venue is already booked at that time.");
                ViewBag.Venues = _context.Venues.ToList();
                ViewBag.Users = _context.Users.ToList();
                return View(booking);
            }

            booking.Status = "pending";
            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
