namespace BSM311.Models
{
    public class Service
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public int ExpertiseId { get; set; }
        public Expertise ?Expertise { get; set; }
    }
}