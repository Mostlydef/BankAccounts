using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Currency)
                    .HasMaxLength(3)
                    .IsRequired();

                entity.HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.OwnerId)
                    .HasMethod("hash");
            });

            // Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Currency)
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(t => t.Description)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.HasOne(t => t.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(t => t.AccountId);

                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(t => t.CounterpartyAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.Timestamp)
                    .HasMethod("gist");

                entity.HasIndex(t => new { t.AccountId, t.Timestamp })
                    .HasDatabaseName("IX_Transactions_AccountId_Timestamp");
            });
        }
    }
}
