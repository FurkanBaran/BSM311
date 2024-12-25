using System.Collections.Generic;

namespace BSM311.Models
{
	public class SalonSettings
	{
		public int Id { get; set; }
		public string SalonName { get; set; }
		public string? Address { get; set; }
		public string? Phone { get; set; }
		public string? Email { get; set; }
		public ICollection<SalonWorkingHours> WorkingHours { get; set; }
	}
}