using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSM311.Models;
using BSM311.Models.DTOs;
using BSM311.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BSM311.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Customer/Home/Index
        // This action method retrieves a list of expertises and their related services
        [HttpGet]
        [Route("Customer/Home/Index")]
        public async Task<IActionResult> Index()
        {
            var expertises = await _context.Expertises
                .Include(e => e.Services)
                .Select(e => new ExpertiseDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Services = e.Services.Select(s => new ServiceDTO
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.Price,
                        DurationInMinutes = s.DurationInMinutes
                    }).ToList()
                })
                .ToListAsync();

            return View(expertises);
        }

        // GET: Customer/Home/BookAppointment/{serviceId}
        // This action method displays the appointment booking page for a specific service
        [HttpGet]
        [Route("Customer/Home/BookAppointment/{serviceId?}")]
        public IActionResult BookAppointment(int serviceId)
        {
            var model = new AppointmentDTO
            {
                ServiceId = serviceId
            };
            return View(model);
        }

        // GET: Customer/Home/MyAppointments
        // This action method retrieves the list of appointments for the logged-in user
        [HttpGet]
        [Route("Customer/Home/MyAppointments")]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Where(a => a.CustomerId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            // Convert appointment dates to Turkish time zone
            var turkishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            foreach (var appointment in appointments)
            {
                appointment.AppointmentDate = TimeZoneInfo.ConvertTimeFromUtc(
                    appointment.AppointmentDate.ToUniversalTime(),
                    turkishTimeZone
                );
            }

            return View(appointments);
        }
    }
}
