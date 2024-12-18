namespace BSM311.Models
{
    public class Salon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public List<Employee> Employees { get; set; }
    }


}
