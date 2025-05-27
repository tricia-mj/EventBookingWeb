using Microsoft.AspNetCore.Mvc;
using EventBookingWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text.Json;


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

            // Show forecast after saving
            string forecast = await GetWeatherForecastAsync("Bacolod", booking.Date);
            TempData["Forecast"] = forecast;

            return RedirectToAction("Index");
        }



        private async Task<string> GetWeatherForecastAsync(string city, DateTime eventDate)
        {
            string apiKey = "10cc820ff03e401fbeb100750252705"; // TODO: Insert your actual WeatherAPI key
            string url = $"https://api.weatherapi.com/v1/forecast.json?key=10cc820ff03e401fbeb100750252705&q=Bacolod&days=3";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return "Forecast unavailable.";

            string result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);
            var forecastDays = doc.RootElement.GetProperty("forecast").GetProperty("forecastday");

            foreach (var day in forecastDays.EnumerateArray())
            {
                if (DateTime.Parse(day.GetProperty("date").ToString()).Date == eventDate.Date)
                {
                    var condition = day.GetProperty("day").GetProperty("condition").GetProperty("text").ToString();
                    var temp = day.GetProperty("day").GetProperty("avgtemp_c").GetDecimal();
                    return $"{condition}, {temp}°C";
                }
            }

            return "Forecast not available for selected date.";
        }

    }
}
