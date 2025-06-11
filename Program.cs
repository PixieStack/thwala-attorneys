using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThwalaAttorneys.Data;
using ThwalaAttorneys.Services;
using ThwalaAttorneys.Services.Interfaces;
using System;
using System.Threading;

namespace thwala_attorneys
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Enhanced logging setup
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Log the connection string being used (without credentials)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"Using database connection: {connectionString?.Split(';')[0]}");

            // Log the email settings (without password)
            var emailServer = builder.Configuration["EmailSettings:SmtpServer"];
            var emailPort = builder.Configuration["EmailSettings:SmtpPort"];
            var emailUsername = builder.Configuration["EmailSettings:SmtpUsername"];
            Console.WriteLine($"Email configuration: Server={emailServer}, Port={emailPort}, Username={emailUsername}");

            // DB Context configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString ??
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            // Register email service and test it
            builder.Services.AddScoped<IEmailService, EmailService>();

            // Add a transient service to verify email configuration on startup
            builder.Services.AddTransient<IHostedService, EmailConfigVerifier>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Create/migrate database on startup with enhanced retry logic
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                int retryCount = 0;
                const int maxRetries = 30;  // Increased to 30 attempts (5 minutes at 10-second intervals)
                const int retryDelayMs = 10000;  // 10 seconds between retries

                Console.WriteLine("Starting database connection and migration process...");

                while (retryCount < maxRetries)
                {
                    try
                    {
                        if (retryCount > 0)
                        {
                            Console.WriteLine($"Retry attempt {retryCount}/{maxRetries}...");
                        }

                        logger.LogInformation("Attempting to connect to the database and migrate...");
                        var context = services.GetRequiredService<ApplicationDbContext>();

                        // First check if we can connect
                        Console.WriteLine("Testing database connection...");
                        bool canConnect = context.Database.CanConnect();

                        if (!canConnect)
                        {
                            throw new Exception("Cannot establish connection to the database");
                        }

                        Console.WriteLine("Connection successful, applying migrations...");
                        context.Database.Migrate();

                        logger.LogInformation("Database migrated successfully");
                        Console.WriteLine("Database migrated successfully");
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        logger.LogError(ex, $"Database migration failed (Attempt {retryCount}/{maxRetries}): {ex.Message}");
                        Console.WriteLine($"Error during migration (Attempt {retryCount}/{maxRetries}): {ex.Message}");

                        if (retryCount < maxRetries)
                        {
                            logger.LogInformation($"Waiting {retryDelayMs / 1000} seconds before retrying...");
                            Console.WriteLine($"Waiting {retryDelayMs / 1000} seconds before retrying...");
                            Thread.Sleep(retryDelayMs);
                        }
                        else
                        {
                            logger.LogCritical($"Database migration failed after {maxRetries} attempts");
                            Console.WriteLine($"Database migration failed after {maxRetries} attempts");
                            Console.WriteLine("Continuing application startup despite database migration failure");
                        }
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }

    // Add this class to verify email configuration on startup
    public class EmailConfigVerifier : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailConfigVerifier> _logger;

        public EmailConfigVerifier(IServiceProvider serviceProvider, ILogger<EmailConfigVerifier> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                _logger.LogInformation("Testing email service connectivity...");
                var smtpServer = configuration["EmailSettings:SmtpServer"];
                var smtpUsername = configuration["EmailSettings:SmtpUsername"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername))
                {
                    _logger.LogWarning("Email settings not fully configured. SMTP Server or Username is missing.");
                    return;
                }

                // Test email connection without actually sending an email
                await Task.Delay(5000, cancellationToken); // Wait for app to initialize

                try
                {
                    // Send a test email to verify configuration
                    await emailService.SendEmailAsync(
                        configuration["EmailSettings:LawyerEmail"] ?? configuration["EmailSettings:SmtpUsername"],
                        "Email Service Test - Application Startup",
                        "<p>This is a test email sent during application startup to verify email configuration.</p>"
                    );
                    _logger.LogInformation("Email service connectivity test successful!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email service connectivity test failed. Email functionality may not work.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while verifying email configuration");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}