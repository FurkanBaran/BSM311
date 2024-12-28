using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSM311.Models.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public List<DayOfWeek> WorkDays { get; set; } = new List<DayOfWeek>();
        public List<string>ExpertiseNames { get; set; } = new List<string>();
    }

    public class EmployeeListDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }



    public class ExpertiseDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public List<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();
    }

    public class ServiceDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public int ExpertiseId { get; set; }
        public string? ExpertiseName { get; set; }
    }


    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }


    public class AppointmentDTO
    {
        [Required]
        public DateTime AppointmentDate { get; set; } 

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class BookAppointmentViewModel
    {
        public AppointmentDTO Appointment { get; set; } = new AppointmentDTO();
        public List<EmployeeListDTO> Employees { get; set; } = new List<EmployeeListDTO>();
    }

    public class SalonSettingsUpdateModel
    {
        public string SalonName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public List<SalonWorkingHours> WorkingHours { get; set; }
    }


}