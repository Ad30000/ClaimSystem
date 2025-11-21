using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ClaimDatabaseContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add session services
            builder.Services.AddDistributedMemoryCache(); // Saves session details in server memory
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // User stays logged in for 30 minutes
                options.Cookie.HttpOnly = true; // Protects from attacks and session stealing
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "ClaimSystem.Session";
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ClaimDatabaseContext>();

                    // Ensure database is created and migrations are applied
                    context.Database.EnsureCreated();

                    // Seed HR user if not exists
                    if (!context.Users.Any(u => u.Role == "HR"))
                    {
                        var hrUser = new User
                        {
                            Username = "hr@test.com",
                            Email = "hr@test.com",
                            PasswordHash = "hashed_password", // In production, hash this properly
                            Role = "HR",
                            IsActive = true
                        };

                        context.Users.Add(hrUser);
                        context.SaveChanges();

                        var hrProfile = new HR
                        {
                            UserId = hrUser.UserId,
                            Name = "",
                            Department = "",
                            PhoneNumber = "",
                            DefaultHourlyRate = 500.00m
                        };


                        context.HR.Add(hrProfile);
                        context.SaveChanges();

                        Console.WriteLine("HR user seeded successfully!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while setting up the database: {ex.Message}");
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
            app.UseStaticFiles(); // Added missing static files middleware
            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    context.Request.Path = "/Home/Error";
                    await next();
                }
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

/*Part 2 Referencing
 * Author: Raw Coding
 * Year: 2019
 * Title of video: ASP.NET Core - Roles vs Claims vs Policy
 * Available at:https://www.youtube.com/watch?v=cbtK3U2aOlg&t=592s
 * Accessed: 20 October 2025
 */


