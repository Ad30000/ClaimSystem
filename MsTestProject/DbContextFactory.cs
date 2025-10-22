using ClaimSystem.DB;
using ClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace TestProject2
{
    public static class TestDbContextFactory
    {
        public static ClaimDatabaseContext CreateUniqueContext()
        {
            var options = new DbContextOptionsBuilder<ClaimDatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ClaimDatabaseContext(options);
        }

        public static ClaimDatabaseContext CreateSeededContext()
        {
            var context = CreateUniqueContext();

            // Seed test data
            var user = new User
            {
                UserId = 1,
                Username = "testuser",
                Email = "test@university.com",
                PasswordHash = "hashedpassword",
                Role = "Lecturer",
                IsActive = true
            };

            var lecturer = new Lecturer
            {
                LecturerId = 1,
                UserId = 1,
                Name = "Test Lecturer",
                Department = "Computer Science",
                PhoneNumber = "1234567890",
                Address = "Test Address"
            };

            var claim = new Claim
            {
                ClaimId = 1,
                ClaimNumber = "CLM-001",
                LecturerId = 1,
                MonthClaim = "January 2024",
                HoursWorked = 40,
                AmountPerHour = 100,
                Status = "Pending",
                DateSubmitted = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            context.Users.Add(user);
            context.Lecturers.Add(lecturer);
            context.Claims.Add(claim);
            context.SaveChanges();

            return context;
        }
    }

}

/*Part 2 Referencing
 * Author: Teddy Smith 
 * Year: 2022
 * Title of video: Unit Testing in C# 2022: 5. MVC Controllers
 * Available at:https://www.youtube.com/watch?v=QnIwfFjBkTM
 * Accessed: 20 October 2025
 */
