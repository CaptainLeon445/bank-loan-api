using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BankApp.Domain.Entities;

namespace BankApp.Application.Common;

public interface IBankDbContext
{
    DbSet<User> Users { get; }
    DbSet<Loan> Loans {get;}
    DbSet<Wallet> Wallets { get; }
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
