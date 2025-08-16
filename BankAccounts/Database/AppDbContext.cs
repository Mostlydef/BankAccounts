using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;
using BankAccounts.Infrastructure.Rabbit.Consumers;
using BankAccounts.Infrastructure.Rabbit.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Database
{
    /// <summary>
    /// Контекст базы данных для работы с банковскими счетами и транзакциями.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Контекст базы данных для работы с банковскими счетами и транзакциями.
        /// </summary>
        public DbSet<Account> Accounts { get; set; }
        /// <summary>
        /// Таблица транзакций по счетам.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }
        /// <summary>
        /// Таблица исходящих сообщений Outbox.
        /// </summary>
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        /// <summary>
        /// Таблица сообщений, которые уже были обработаны (Inbox) для обеспечения идемпотентности.
        /// </summary>
        public DbSet<InboxConsumed> InboxConsumed { get; set; }
        /// <summary>
        /// Таблица событий аудита.
        /// </summary>
        public DbSet<AuditEvent> AuditEvents { get; set; }
        /// <summary>
        /// Таблица сообщений, которые не удалось обработать (Dead Letters).
        /// </summary>
        public DbSet<InboxDeadLetter> InboxDeadLetters { get; set; }

        /// <summary>
        /// Конфигурация модели данных с настройками сущностей и связей.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели для настройки сущностей EF Core.</param>
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

                entity.Property(a => a.Xmin)
                    .HasColumnName("xmin")               
                    .HasColumnType("xid")                
                    .ValueGeneratedOnAddOrUpdate()       
                    .IsConcurrencyToken();
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

            // OutboxMessage
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("outbox_messages");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Payload)
                    .HasColumnType("jsonb")
                    .HasMaxLength(256);
                entity.Property(x => x.Headers)
                    .HasColumnType("jsonb")
                    .HasMaxLength(256);
                entity.Property(x => x.Type)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(x => x.RoutingKey)
                    .HasMaxLength(256)
                    .IsRequired();
                entity.Property(x => x.Status)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.HasIndex(x => new { x.Status, x.NextAttemptAt, x.OccurredAt })
                    .HasDatabaseName("idx_outbox_pending");
            });

            // InboxConsumed
            modelBuilder.Entity<InboxConsumed>(entity =>
            {
                entity.ToTable("inbox_message");
                entity.HasKey(x => x.MessageId);
                entity.Property(x => x.Handler)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            // AuditEvent
            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.ToTable("audit_events")
                    .HasKey(x => x.Id);
                entity.Property(x => x.Handler)
                    .HasMaxLength(100);
                entity.Property(x => x.Payload)
                    .HasColumnType("jsonb");
            });

            // InboxDeadLetter
            modelBuilder.Entity<InboxDeadLetter>(entity =>
            {
                entity.ToTable("inbox_dead_letters");
                entity.HasKey(x => x.MessageId);
                entity.Property(x => x.Handler)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(x => x.Payload)
                    .HasColumnType("jsonb")
                    .IsRequired();
                entity.Property(x => x.Error)
                    .HasColumnType("text")
                    .IsRequired();
                entity.Property(x => x.ReceivedAt)
                    .IsRequired();
            });
        }
    }
}
