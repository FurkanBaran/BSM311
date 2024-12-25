using System;
using System.Collections.Generic;

namespace BSM311.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<DayOfWeek> WorkingDays { get; set; }
        public ICollection<Expertise> Expertises { get; set; }
    }
}