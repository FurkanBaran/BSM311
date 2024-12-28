using Microsoft.AspNetCore.Identity;
using System;

namespace BSM311.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Address { get; set; }
    }

}