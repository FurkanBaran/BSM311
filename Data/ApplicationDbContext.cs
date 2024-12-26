using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BSM311.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        public DbSet<EmployeeWorkDay> EmployeeWorkDays { get; set; } // Eklendi

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Employee>()
                .HasMany(e => e.Expertises)
                .WithMany(e => e.Employees);

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

            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EmployeeWorkDay>() // Düzeltildi
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