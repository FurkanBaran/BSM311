using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BSM311.Models;

namespace BSM311.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Expertise> Expertises { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<SalonSettings> SalonSettings { get; set; }
        public DbSet<SalonWorkingHours> SalonWorkingHours { get; set; }
        public DbSet<EmployeeWorkDay> EmployeeWorkDays { get; set; }
        public DbSet<EmployeeExpertise> EmployeeExpertises { get; set; } // EmployeeExpertise DbSet'i eklendi

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EmployeeExpertise>()
                .HasKey(ee => new { ee.EmployeeId, ee.ExpertiseId });

            builder.Entity<EmployeeExpertise>()
                .HasOne(ee => ee.Employee)
                .WithMany(e => e.Expertises)
                .HasForeignKey(ee => ee.EmployeeId);

            builder.Entity<EmployeeExpertise>()
                .HasOne(ee => ee.Expertise)
                .WithMany(e => e.Employees)
                .HasForeignKey(ee => ee.ExpertiseId);

            builder.Entity<Service>()
                .HasOne(s => s.Expertise)
                .WithMany(e => e.Services)
                .HasForeignKey(s => s.ExpertiseId);

            builder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<EmployeeWorkDay>()
                .HasOne(ewd => ewd.Employee)
                .WithMany(e => e.WorkDays)
                .HasForeignKey(ewd => ewd.EmployeeId);

            builder.Entity<SalonWorkingHours>()
                .HasOne(wh => wh.SalonSettings)
                .WithMany(s => s.WorkingHours)
                .HasForeignKey(wh => wh.SalonSettingsId);

            builder.Entity<EmployeeWorkDay>()
                .HasIndex(ewd => new { ewd.EmployeeId, ewd.DayOfWeek });

            builder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentDate);

            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);
        }
    }
}
