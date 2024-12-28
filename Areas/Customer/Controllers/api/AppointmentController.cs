// Required using statements for the controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSM311.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using BSM311.Data;
using BSM311.Models.DTOs;
using Microsoft.AspNetCore.Identity;

[Area("Customer")]
[ApiController]
[Route("api/[area]/[controller]")]
public class AppointmentController : ControllerBase
{
    // Database context and user manager instances
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Constructor for dependency injection
    public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Get service details by service ID
    [HttpGet("service-details/{serviceId}")]
    public async Task<IActionResult> GetServiceDetails(int serviceId)
    {
        // Query service with expertise information
        var service = await _context.Services
            .Include(s => s.Expertise)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Service not found"
            });

        return Ok(new ApiResponse
        {
            Success = true,
            Data = new
            {
                service.Name,
                service.Description,
                service.Price,
                service.DurationInMinutes
            }
        });
    }

    // Get available weeks for scheduling
    [HttpGet("available-weeks/{serviceId}")]
    public async Task<IActionResult> GetAvailableWeeks(int serviceId)
    {
        // Get service information
        var service = await _context.Services
            .Include(s => s.Expertise)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Service not found"
            });

        // Get salon working hours
        var salonWorkingHours = await _context.SalonWorkingHours.ToListAsync();

        // Calculate current and next week dates
        var now = DateTime.Now;
        var today = now.Date;
        var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var nextWeekStart = currentWeekStart.AddDays(7);

        var weeks = new List<object>();
        foreach (var weekStart in new[] { currentWeekStart, nextWeekStart })
        {
            var days = new List<object>();
            for (int i = 0; i < 7; i++)
            {
                var currentDate = weekStart.AddDays(i);
                var dayOfWeek = currentDate.DayOfWeek;

                // Check if salon is open on this day
                var salonWorkingHour = salonWorkingHours.FirstOrDefault(wh => wh.DayOfWeek == dayOfWeek);
                var isSalonOpen = salonWorkingHour?.IsOpen ?? false;

                // Check if current time is after closing time
                var isAfterClosingTime = false;
                if (currentDate.Date == today && salonWorkingHour != null)
                {
                    var lastAppointmentTime = salonWorkingHour.CloseTime.Add(TimeSpan.FromMinutes(-service.DurationInMinutes));
                    isAfterClosingTime = now.TimeOfDay > lastAppointmentTime;
                }

                // Check availability based on multiple conditions
                var isAvailable = isSalonOpen &&
                                !isAfterClosingTime &&
                                currentDate >= today &&
                                await _context.Employees
                                    .Include(e => e.WorkDays)
                                    .Include(e => e.Expertises)
                                    .AnyAsync(e =>
                                        e.WorkDays.Any(w => w.DayOfWeek == dayOfWeek && w.IsWorking) &&
                                        e.Expertises.Any(ee => ee.ExpertiseId == service.ExpertiseId));

                days.Add(new
                {
                    Date = currentDate,
                    DayOfWeek = dayOfWeek.ToString(),
                    FormattedDate = currentDate.ToString("dd MMM yyyy"),
                    IsAvailable = isAvailable,
                    IsSalonOpen = isSalonOpen,
                    IsToday = currentDate.Date == today,
                    IsAfterClosingTime = isAfterClosingTime
                });
            }

            weeks.Add(new { WeekStart = weekStart, Days = days });
        }

        return Ok(new ApiResponse { Success = true, Data = weeks });
    }

    // Get available employees for a specific service on a specific date
    [HttpGet("available-employees/{serviceId}/{date}")]
    public async Task<IActionResult> GetAvailableEmployees(int serviceId, [FromRoute] DateTime date)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid date format."
            });

        var service = await _context.Services
            .Include(s => s.Expertise)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Service not found"
            });

        var employees = await _context.Employees
            .Include(e => e.WorkDays)
            .Include(e => e.Expertises)
            .Where(e => e.WorkDays.Any(w => w.DayOfWeek == date.DayOfWeek && w.IsWorking) &&
                       e.Expertises.Any(ee => ee.ExpertiseId == service.ExpertiseId))
            .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
            .ToListAsync();

        if (!employees.Any())
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "No employees available on the selected day."
            });

        return Ok(new ApiResponse { Success = true, Data = employees });
    }

    // Get available time slots for a specific employee on a specific date
    [HttpGet("available-times/{employeeId}/{date}")]
    public async Task<IActionResult> GetAvailableTimes(int employeeId, DateTime date)
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.EmployeeId == employeeId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            var startTime = new TimeSpan(9, 0, 0);
            var endTime = new TimeSpan(18, 0, 0);
            var intervals = new List<object>();
            var now = DateTime.Now;

            while (startTime < endTime)
            {
                var currentDateTime = date.Date + startTime;

                // Filter out past times
                var isAvailable = currentDateTime > now &&
                    !appointments.Any(a =>
                        a.AppointmentDate <= currentDateTime &&
                        currentDateTime < a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes));

                intervals.Add(new
                {
                    Time = startTime.ToString(@"hh\:mm"),
                    IsAvailable = isAvailable
                });

                startTime = startTime.Add(TimeSpan.FromMinutes(30));
            }

            return Ok(new ApiResponse { Success = true, Data = intervals });
        }
        catch (Exception)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Error processing date/time information."
            });
        }
    }

    // Book an appointment
    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] AppointmentDTO model)
    {
        if (model.EmployeeId <= 0 || model.AppointmentDate == default)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Please fill in all required fields."
            });

        // Check for past date/time
        if (model.AppointmentDate <= DateTime.Now)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Cannot book appointments in the past."
            });

        var service = await _context.Services.FindAsync(model.ServiceId);
        if (service == null)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Service not found."
            });

        // Specify the kind and convert to UTC
        var localAppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Local);
        var utcAppointmentDate = localAppointmentDate.ToUniversalTime();
        var utcAppointmentEnd = utcAppointmentDate.AddMinutes(service.DurationInMinutes);

        // Calculate the overlapping appointments more accurately
        var isConflict = await _context.Appointments
            .Include(a => a.Service)
            .Where(a =>
                a.EmployeeId == model.EmployeeId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.AppointmentDate < utcAppointmentEnd &&
                utcAppointmentDate < a.AppointmentDate.AddMinutes(a.Service.DurationInMinutes))
            .AnyAsync();

        if (isConflict)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Selected time slot is already booked."
            });

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse
            {
                Success = false,
                Message = "User not authenticated.",
                Data = "/Account/Login"
            });

        var appointment = new Appointment
        {
            AppointmentDate = utcAppointmentDate,
            EmployeeId = model.EmployeeId,
            ServiceId = model.ServiceId,
            CustomerId = userId,
            Notes = model.Notes,
            Status = AppointmentStatus.Pending
        };

        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Appointment booked successfully!"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while booking the appointment."
            });
        }
    }

    // Cancel an appointment
    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var userId = _userManager.GetUserId(User);
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == userId);

        if (appointment == null)
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Appointment not found."
            });

        if (appointment.AppointmentDate <= DateTime.Now)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Cannot cancel past appointments."
            });

        if (appointment.Status == AppointmentStatus.Cancelled)
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Appointment is already cancelled."
            });

        appointment.Status = AppointmentStatus.Cancelled;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Appointment cancelled successfully!"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while cancelling the appointment."
            });
        }
    }
}
