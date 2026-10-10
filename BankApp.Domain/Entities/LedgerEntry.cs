using System;
namespace BankApp.Domain.Entities;

public class LedgerEntry
{
    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public string EntryType { get; private set; }
    public string Reference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private LedgerEntry()
    {
        EntryType = string.Empty;
        Reference = string.Empty;
    } //for EF core

    public LedgerEntry(Guid walletId, decimal amount, string entryType, string reference)
    {
        if (amount == 0) throw new ArgumentException("Ledger amount cannot be zero.");

        Id = Guid.NewGuid();
        WalletId = walletId;
        Amount = amount;
        EntryType = string.IsNullOrWhiteSpace(entryType) ? throw new ArgumentException("Invalid entry type.") : entryType;
        Reference = reference ?? string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
