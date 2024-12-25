using System;

namespace BSM311.Models
{
	public class SalonWorkingHours
	{
		public int Id { get; set; }
		public DayOfWeek DayOfWeek { get; set; }
		public TimeSpan OpenTime { get; set; }
		public TimeSpan CloseTime { get; set; }
		public bool IsOpen { get; set; }
		public int SalonSettingsId { get; set; }
		public SalonSettings SalonSettings { get; set; }
	}
}
