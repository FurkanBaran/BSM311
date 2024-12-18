namespace BSM311.Models
{

    public class Appointment
    {
        public int Id { get; set; }
        public int SalonId { get; set; }
        public Salon Salon { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }
    }
}
