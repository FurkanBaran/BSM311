// Areas/Admin/Controllers/api/AppointmentController.cs
using BSM311.Data;
using BSM311.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BSM311.Areas.Admin.Controllers.api
{
    
[Area("Admin")]
[ApiController]
[Route("api/[area]/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
        _context = context;
        _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            try
            {
                // Identify the Turkey time zone
                TimeZoneInfo turkeyTimeZone;
                try
                {
                    turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); // Windows
                }
                catch (TimeZoneNotFoundException)
                {
                    turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); // Linux/macOS
                }

                var appointments = await _context.Appointments
                    .Include(a => a.Employee)
                    .Include(a => a.Service)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => new
                    {
                        a.Id,
                        a.AppointmentDate,
                        CustomerId = a.CustomerId,
                        Employee = $"{a.Employee.FirstName} {a.Employee.LastName}",
                        Service = a.Service.Name,
                        Price = a.Service.Price,
                        Duration = a.Service.DurationInMinutes,
                        a.Status,
                        a.Notes,
                        CanEdit = a.AppointmentDate > DateTime.UtcNow
                    })
                    .ToListAsync();

                // Get customer information separately
                var customerIds = appointments.Select(a => a.CustomerId).Distinct();
                var customers = await _userManager.Users
                    .Where(u => customerIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}");

                // Adjust AppointmentDate to Turkey time zone
                var result = appointments.Select(a => new
                {
                    a.Id,
                    // Convert from UTC to Turkey local time
                    AppointmentDate = TimeZoneInfo.ConvertTimeFromUtc(a.AppointmentDate, turkeyTimeZone).ToString("o"),
                    Customer = customers.GetValueOrDefault(a.CustomerId, "Unknown"),
                    a.Employee,
                    a.Service,
                    a.Price,
                    a.Duration,
                    a.Status,
                    a.Notes,
                    a.CanEdit
                });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDTO model)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound(new { success = false, message = "Appointment not found" });

        appointment.Status = model.Status;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Status updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound(new { success = false, message = "Appointment not found" });

        try
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

// AppointmentStatusUpdateDTO.cs
public class AppointmentStatusUpdateDTO
{
    public AppointmentStatus Status { get; set; }
}

}
