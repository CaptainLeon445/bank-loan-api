using System;
using System.Collections.Generic;
using BankApp.Domain.Strategies;
namespace BankApp.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestRate { get; private set; }
    public int TermInMonths { get; private set; }
    public string RepaymentStrategyType { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public ICollection<LoanScheduleItem> AmortizationSchedule { get; private set; } = new List<LoanScheduleItem>();

    private Loan()
    {
        RepaymentStrategyType= string.Empty;
        Status ="Pending";
    } //for EF core

    public Loan(Guid walletId, decimal principalAmount, decimal interestRate, int termInMonths, string repaymentStrategyType)
    {
        if (principalAmount <= 0) throw new ArgumentException("Principal amount must be greater than zero.");
        if (interestRate <= 0) throw new ArgumentException("Interest rate must be greater than zero.");
        if (termInMonths <= 0) throw new ArgumentException("Term in months must be greater than zero.");

        Id = Guid.NewGuid();
        WalletId = walletId;
        PrincipalAmount = principalAmount;
        InterestRate = interestRate;
        TermInMonths = termInMonths;
        RepaymentStrategyType = repaymentStrategyType;
        RemainingBalance = principalAmount;
        Status = "Pending";
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void GenerateSchedule(ILoanAmortizationStrategy strategy)
    {
        AmortizationSchedule = strategy.CalculateSchedule(this);
    }

    public void Disburse()
    {
        if (Status != "Pending")
            throw new InvalidOperationException("Loan can only be disbursed if it is in Pending status.");
        Status = "Active";
    }

    public void RecordPayment(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Payment amount must be positive.");
        RemainingBalance = Math.Max(0, RemainingBalance - amount);
        if (RemainingBalance == 0) Status = "FullyPaid";
    }


}