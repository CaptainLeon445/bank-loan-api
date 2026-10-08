using Microsoft.EntityFrameworkCore;
using BankApp.Domain.Entities;
using BankApp.Application.Common;

namespace BankApp.Infrastructure.Data;

public class BankDbContext : DbContext, IBankDbContext
{
    public DbSet<User> Users => Set<User>();

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
    }
}
