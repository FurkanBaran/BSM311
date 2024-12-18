using Microsoft.EntityFrameworkCore;
using BSM311.Models;

namespace BSM311.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Salon> Salons { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }
}
