using System.Collections.Generic;

namespace BSM311.Models
{
    public class Expertise
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<EmployeeExpertise> Employees { get; set; } = new List<EmployeeExpertise>();
    }
}