using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BSM311.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BSM311.Constants;


namespace BSM311.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                // Get required services from the service provider
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Initialize roles, admin user, and other data
                await CreateRoles(roleManager);
                await CreateAdminUser(userManager);
                await CreateSalonSettings(context);
                await CreateExpertises(context);
                await CreateServices(context);
                await CreateEmployees(context); 
            }
            catch (Exception ex)
            {
                throw new Exception("Database initialization failed", ex);
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            // Define roles to be created
            string[] roles = { UserRoles.Admin, UserRoles.Employee, UserRoles.Customer };
            foreach (var role in roles)
            {
                // Check if the role already exists, if not, create it
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "b231210371@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Check if the admin user already exists, if not, create it
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                // Create the admin user with a default password
                var result = await userManager.CreateAsync(admin, "sau");
                if (result.Succeeded)
                {
                    // Assign the admin role to the user
                    await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                }
            }
        }

        private static async Task CreateSalonSettings(ApplicationDbContext context)
        {
            // Check if salon settings already exist, if not, create them
            if (!await context.SalonSettings.AnyAsync())
            {
                var salonSettings = new SalonSettings
                {
                    SalonName = "BSM311 Salon",
                    Address = "Sakarya Uni, Sardunya Sk.",
                    Phone = "554-554-5454",
                    Email = "BSM311@sakarya.edu.tr",
                    WorkingHours = new List<SalonWorkingHours>()
                };

                // Define working hours for each day of the week
                for (int i = 0; i <= 6; i++)
                {
                    var day = (DayOfWeek)i;
                    var isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

                    salonSettings.WorkingHours.Add(new SalonWorkingHours
                    {
                        DayOfWeek = day,
                        OpenTime = new TimeSpan(9, 0, 0),
                        CloseTime = isWeekend ? new TimeSpan(13, 0, 0) : new TimeSpan(18, 0, 0),
                        IsOpen = day != DayOfWeek.Sunday
                    });
                }

                context.SalonSettings.Add(salonSettings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateExpertises(ApplicationDbContext context)
        {
            // Check if expertises already exist, if not, create them
            if (!await context.Expertises.AnyAsync())
            {
                var expertises = new[]
                {
                    new Expertise { Name = "Hair", Description = "Hair styling and treatment" },
                    new Expertise { Name = "Makeup", Description = "Makeup services" },
                    new Expertise { Name = "Nail", Description = "Nail care services" }
                };

                context.Expertises.AddRange(expertises);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateServices(ApplicationDbContext context)
        {
            // Check if services already exist, if not, create them
            if (context.Services != null && !await context.Services.AnyAsync())
            {
                if (context.Expertises == null)
                {
                    throw new Exception("Expertises DbSet is null");
                }
                var hairExpertise = await context.Expertises.FirstOrDefaultAsync(e => e.Name == "Hair");
                var makeupExpertise = await context.Expertises.FirstOrDefaultAsync(e => e.Name == "Makeup");

                if (hairExpertise == null || makeupExpertise == null)
                {
                    throw new Exception("Required expertise not found");
                }

                var services = new[]
                {
                    new Service
                    {
                        Name = "Haircut",
                        Description = "Basic haircut service",
                        Price = 200M,
                        DurationInMinutes = 30,
                        ExpertiseId = hairExpertise.Id
                    },
                    new Service
                    {
                        Name = "Hair Coloring",
                        Description = "Full hair coloring service",
                        Price = 500M,
                        DurationInMinutes = 120,
                        ExpertiseId = hairExpertise.Id
                    },
                    new Service
                    {
                        Name = "Basic Makeup",
                        Description = "Daily makeup application",
                        Price = 300M,
                        DurationInMinutes = 30,
                        ExpertiseId = makeupExpertise.Id
                    }
                };

                context.Services.AddRange(services);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateEmployees(ApplicationDbContext context)
        {
            // Check if employees already exist, if not, create them
            if (context.Employees != null && !await context.Employees.AnyAsync())
            {
                var employees = new[]
                {
                    new Employee
                    {
                        FirstName = "Ahmet",
                        LastName = "Çelik",
                        WorkDays = new List<EmployeeWorkDay>
                        {
                            new EmployeeWorkDay { DayOfWeek = DayOfWeek.Monday, IsWorking = true },
                            new EmployeeWorkDay { DayOfWeek = DayOfWeek.Tuesday, IsWorking = true }
                        }
                    },
                    new Employee
                    {
                        FirstName = "Ayşe",
                        LastName = "Demir",
                        WorkDays = new List<EmployeeWorkDay>
                        {
                            new EmployeeWorkDay { DayOfWeek = DayOfWeek.Wednesday, IsWorking = true },
                            new EmployeeWorkDay { DayOfWeek = DayOfWeek.Thursday, IsWorking = true }
                        }
                    }
                };

                context.Employees.AddRange(employees);
                await context.SaveChangesAsync();
            }
        }
    }
}