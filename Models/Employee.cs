namespace BSM311.Models
{

    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Expertise { get; set; }
        public decimal HourlyRate { get; set; }
        public List<Appointment> Appointments { get; set; }
    }


}
