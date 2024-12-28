using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "First Name")]
    public required string FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public required string PhoneNumber { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; set; }
}