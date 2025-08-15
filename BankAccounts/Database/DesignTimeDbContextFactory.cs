using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankAccounts.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Здесь укажи строку подключения для миграций
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=admin;Password=admin");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}