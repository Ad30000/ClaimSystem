using ClaimSystem.DB;
using Microsoft.EntityFrameworkCore;

namespace TestProject2
{
    internal class DbContextFactory
    {
        public static ClaimDatabaseContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ClaimDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;


            return new ClaimDatabaseContext(optionsBuilder);


        }
    }
}
