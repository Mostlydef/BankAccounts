using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankAccounts.Database
{
    /// <summary>
    /// Фабрика для создания экземпляра <see cref="AppDbContext"/> во время проектирования.
    /// Используется инструментами EF Core, например для миграций.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Создает экземпляр <see cref="AppDbContext"/> с настройками подключения.
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются).</param>
        /// <returns>Новый экземпляр <see cref="AppDbContext"/>.</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=db;Username=admin;Password=admin");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}