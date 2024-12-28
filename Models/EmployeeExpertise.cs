using BSM311.Models;

public class EmployeeExpertise
{
    public int EmployeeId { get; set; }
    public required Employee Employee { get; set; }

    public int ExpertiseId { get; set; }
    public required Expertise Expertise { get; set; } 

    public string? Name => Expertise?.Name;
}
