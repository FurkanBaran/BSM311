using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BSM311.Data;
using BSM311.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Service configurations
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Middleware configurations
ConfigureMiddleware(app);

// Configure routing
ConfigureRouting(app);

// Initialize database
await InitializeDatabase(app);

app.Run();

// Service configuration method
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // MVC Configuration
    services.AddControllersWithViews();

    // Database Configuration
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    // Identity Configuration
    services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Swagger Configuration
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "api",
            Version = "v1"
        });
    });

    // Authentication Configuration
    services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
}

// Middleware configuration method
static void ConfigureMiddleware(WebApplication app)
{
    // Development specific middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "api"));
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Common middleware
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
}

// Routing configuration method
static void ConfigureRouting(WebApplication app)
{
    // Area route
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    // Default route
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Admin route
    app.MapControllerRoute(
        name: "admin",
        pattern: "{controller=Admin}/{action=Dashboard}/{id?}");
}

// Database initialization method
static async Task InitializeDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await DbInitializer.Initialize(services);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}