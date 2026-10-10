using Microsoft.EntityFrameworkCore;
using BankApp.Domain.Entities;
using BankApp.Application.Common;

namespace BankApp.Infrastructure.Data;

public class BankDbContext : DbContext, IBankDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
           entity.HasKey(e=>e.Id);
           entity.HasIndex(e=>e.Email).IsUnique();
           entity.Property(e=>e.FirstName).HasMaxLength(100).IsRequired();
           entity.Property(e=>e.LastName).HasMaxLength(100).IsRequired();
           entity.Property(e=>e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
           entity.HasKey(e=>e.Id);
           entity.HasIndex(e=>e.UserId);
           entity.Property(e=>e.Balance).HasPrecision(18, 2);
           entity.Property(e=>e.LoanLimit).HasPrecision(18, 2);
           // Maps to PostgreSQL's xmin: concurrent balance updates fail with DbUpdateConcurrencyException instead of overwriting each other.
           entity.Property<uint>("Version").IsRowVersion();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
           entity.HasKey(e=>e.Id);
           entity.HasIndex(e=>e.WalletId);
           entity.Property(e=>e.PrincipalAmount).HasPrecision(18, 2);
           entity.Property(e=>e.RemainingBalance).HasPrecision(18, 2);
           entity.Property(e=>e.InterestRate).HasPrecision(9, 6);
           entity.Property(e=>e.RepaymentStrategyType).HasMaxLength(50).IsRequired();
           entity.Property(e=>e.Status).HasMaxLength(50).IsRequired();
           entity.HasMany(e=>e.AmortizationSchedule).WithOne().HasForeignKey(i=>i.LoanId);
        });

        modelBuilder.Entity<LoanScheduleItem>(entity =>
        {
           entity.HasKey(e=>e.Id);
           entity.Property(e=>e.PrincipalDue).HasPrecision(18, 2);
           entity.Property(e=>e.InterestDue).HasPrecision(18, 2);
           entity.Ignore(e=>e.TotalInstallmentAmount);
        });

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
           entity.HasKey(e=>e.Id);
           entity.HasIndex(e=>e.WalletId);
           entity.Property(e=>e.Amount).HasPrecision(18, 2);
           entity.Property(e=>e.EntryType).HasMaxLength(50).IsRequired();
           entity.Property(e=>e.Reference).HasMaxLength(100);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
           entity.HasKey(e=>e.Id);
           entity.HasIndex(e=>new { e.Processed, e.CreatedAtUtc });
           entity.Property(e=>e.Topic).HasMaxLength(200).IsRequired();
           entity.Property(e=>e.Payload).IsRequired();
        });
    }
}
