using System.ComponentModel.DataAnnotations;

namespace BSM311.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        public ICollection<EmployeeWorkDay> WorkDays { get; set; } = new List<EmployeeWorkDay>();
        public ICollection<EmployeeExpertise> Expertises { get; set; } = new List<EmployeeExpertise>();
    }
}