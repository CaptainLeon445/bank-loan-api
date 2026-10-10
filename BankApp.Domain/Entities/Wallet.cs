using System;
namespace BankApp.Domain.Entities;

public class Wallet
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal LoanLimit { get; private set; }
    public bool IsKycVerified { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Wallet() { } //for EF core

    public Wallet(Guid userId, decimal loanLimit)
    {
        if (loanLimit < 0) throw new ArgumentException("Loan limit cannot be negative.");

        Id = Guid.NewGuid();
        UserId = userId;
        Balance = 0;
        LoanLimit = loanLimit;
        IsKycVerified = false;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkKycVerified() => IsKycVerified = true;

    public void Credit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Credit amount must be positive.");
        Balance += amount;
    }
}
