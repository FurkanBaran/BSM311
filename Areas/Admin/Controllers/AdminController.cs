using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BSM311.Data;
using System.Linq;
using System.Threading.Tasks;
using BSM311.Models;
using Microsoft.EntityFrameworkCore;
using BSM311.Models.DTOs;

namespace BSM311.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin")]

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }


        [Route("Dashboard")]
        [HttpGet]

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.UtcNow.Date;

            // Revenue calculations
            var dailyRevenue = await _context.Appointments
                .Where(a => a.AppointmentDate == today && a.Status == AppointmentStatus.Completed)
                .SumAsync(a => (decimal?)a.Service.Price) ?? 0;

            var weeklyRevenue = await _context.Appointments
                .Where(a => a.AppointmentDate >= today.AddDays(-(int)today.DayOfWeek) && a.AppointmentDate < today.AddDays(7 - (int)today.DayOfWeek) && a.Status == AppointmentStatus.Completed)
                .SumAsync(a => (decimal?)a.Service.Price) ?? 0;

            var monthlyRevenue = await _context.Appointments
                .Where(a => a.AppointmentDate.Month == today.Month && a.AppointmentDate.Year == today.Year && a.Status == AppointmentStatus.Completed)
                .SumAsync(a => (decimal?)a.Service.Price) ?? 0;

            // Upcoming revenue
            var upcomingRevenue = await _context.Appointments
                .Where(a => a.AppointmentDate >= today && a.Status == AppointmentStatus.Pending)
                .SumAsync(a => (decimal?)a.Service.Price) ?? 0;

            // Weekly Appointments
            var weeklyAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate >= today.AddDays(-(int)today.DayOfWeek) && a.AppointmentDate < today.AddDays(7 - (int)today.DayOfWeek));

            var todaysAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate == today);

            // Total appointments
            var totalAppointments = await _context.Appointments.CountAsync();


            // Employees
            var totalEmployees = await _context.Employees.CountAsync();

            var todayWorkingEmployees = await _context.EmployeeWorkDays
                .CountAsync(ewd => ewd.DayOfWeek == today.DayOfWeek && ewd.IsWorking);

            // Services
            var expertiseWithServiceCount = await _context.Expertises
                .Select(e => new
                {
                    e.Name,
                    ServiceCount = e.Services.Count
                })
                .ToListAsync();


            // Salon Settings'i al
            var salonSettings = await _context.SalonSettings
                .Include(s => s.WorkingHours)
                .FirstOrDefaultAsync();




            // Model
            var model = new
            {
                DailyRevenue = dailyRevenue,
                WeeklyRevenue = weeklyRevenue,
                MonthlyRevenue = monthlyRevenue,
                UpcomingRevenue = upcomingRevenue, 
                WeeklyAppointments = weeklyAppointments,
                TodaysAppointments = todaysAppointments,
                TotalAppointments = totalAppointments,
                TotalEmployees = totalEmployees,
                TodayWorkingEmployees = todayWorkingEmployees,
                ExpertiseWithServiceCount = expertiseWithServiceCount,
                SalonSettings = salonSettings

            };


            return View(model);
        }

        [Route("Index")]
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            return await Dashboard();
        }



        // GET: Admin/Expertise
        [HttpGet]
        [Route("Expertise")]
        public async Task<IActionResult> Expertise()
        {
            var expertises = await _context.Expertises
                .Include(e => e.Services)
                .ToListAsync();
            return View(expertises);
        }


        // GET: Admin/Employee
        [Route("Employee")]
        [HttpGet]
        public async Task<IActionResult> Employee()
        {
            var employees = await _context.Employees
           .Include(e => e.WorkDays)
           .Include(e => e.Expertises)
               .ThenInclude(ee => ee.Expertise) // Expertise'e erişmek için ThenInclude ekle
           .Select(e => new EmployeeDTO
           {
               Id = e.Id,
               FirstName = e.FirstName,
               LastName = e.LastName,
               WorkDays = e.WorkDays.Select(wd => wd.DayOfWeek).ToList(),
               ExpertiseNames = e.Expertises.Select(ex => ex.Expertise.Name).ToList() 
           })
           .ToListAsync();

            ViewBag.Expertises = await _context.Expertises.ToListAsync();
            return View(employees);
        }



        [Route("Appointment")]
        [HttpGet]
        public IActionResult Appointment()
        {
            return View();
        }




        [Route("SaveSettings")]
        [HttpPost]
        public async Task<IActionResult> SaveSettings([FromBody] SalonSettingsUpdateModel model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            var settings = await _context.SalonSettings
                .Include(s => s.WorkingHours)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SalonSettings
                {
                    WorkingHours = new List<SalonWorkingHours>()
                };
            }

            settings.SalonName = model.SalonName;
            settings.Address = model.Address;
            settings.Phone = model.Phone;
            settings.Email = model.Email;

            // Önce yeni bir settings kaydı oluşturuyoruz (eğer yoksa)
            if (settings.Id == 0)
            {
                _context.SalonSettings.Add(settings);
                await _context.SaveChangesAsync(); // SalonSettingsId için kaydetmemiz gerekiyor
            }

            // Çalışma saatlerini güncelle
            foreach (var workingHour in model.WorkingHours)
            {
                var existing = settings.WorkingHours
                    .FirstOrDefault(w => (int)w.DayOfWeek == workingHour.DayOfWeek);

                if (existing != null)
                {
                    existing.OpenTime = TimeSpan.Parse(workingHour.OpenTime);
                    existing.CloseTime = TimeSpan.Parse(workingHour.CloseTime);
                    existing.IsOpen = workingHour.IsOpen;
                    _context.Update(existing);
                }
                else
                {
                    var newWorkingHour = new SalonWorkingHours
                    {
                        DayOfWeek = (DayOfWeek)workingHour.DayOfWeek,
                        OpenTime = TimeSpan.Parse(workingHour.OpenTime),
                        CloseTime = TimeSpan.Parse(workingHour.CloseTime),
                        IsOpen = workingHour.IsOpen,
                        SalonSettingsId = settings.Id
                    };
                    _context.SalonWorkingHours.Add(newWorkingHour);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class SalonSettingsUpdateModel
        {
            public string SalonName { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public List<WorkingHourUpdateModel> WorkingHours { get; set; } = new();
        }

        public class WorkingHourUpdateModel
        {
            public int DayOfWeek { get; set; }
            public string OpenTime { get; set; }
            public string CloseTime { get; set; }
            public bool IsOpen { get; set; }
        }






    }
}
