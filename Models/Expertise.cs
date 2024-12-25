using System.Collections.Generic;

namespace BSM311.Models
{
    public class Expertise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Service> Services { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}