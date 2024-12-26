namespace BSM311.Models
{
	public class EmployeeWorkDay
	{
		public int Id { get; set; }
		public int EmployeeId { get; set; }
		public Employee Employee { get; set; }
		public DayOfWeek DayOfWeek { get; set; }
		public bool IsWorking { get; set; } = true;
	}
}