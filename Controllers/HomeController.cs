using BSM311.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using BSM311.Data;

namespace BSM311.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Action method for the Index page
        public async Task<IActionResult> Index()
        {
            // Retrieve all expertises and include their related services
            ViewBag.Expertises = await _context.Expertises
                .Include(e => e.Services)
                .ToListAsync();

            // Retrieve all services and include their related expertise
            ViewBag.Services = await _context.Services
                .Include(s => s.Expertise)
                .ToListAsync();

            // Retrieve salon settings including working hours
            var salonSettings = await _context.SalonSettings
                .Include(s => s.WorkingHours)
                .FirstOrDefaultAsync();

            // Store salon settings in ViewBag
            ViewBag.SalonSettings = salonSettings;

            // Get today's working hours
            var today = DateTime.Now.DayOfWeek;
            ViewBag.TodayWorkingHours = salonSettings?.WorkingHours
                ?.FirstOrDefault(w => w.DayOfWeek == today);

            // Get the days when the salon is open
            ViewBag.OpenDays = salonSettings?.WorkingHours
                ?.Where(w => w.IsOpen)
                ?.OrderBy(w => w.DayOfWeek)
                ?.ToList();

            return View();
        }

        // Action method for the Privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Action method for the Error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
