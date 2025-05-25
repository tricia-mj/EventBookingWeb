using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventBookingWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventBookingWeb.Controllers
{
    [Authorize(Roles = "user")]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);

            var bookings = await _context.Bookings
                .Include(b => b.Venue)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(bookings);
        }

        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venues.Where(v => v.IsAvailable).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            booking.UserId = int.Parse(User.FindFirst("UserId").Value);

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
                return View(booking);
            }

            booking.Status = "pending";
            _context.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}