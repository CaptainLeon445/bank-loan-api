using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BankApp.Application.Common;
using BankApp.Application.DTOs;
using BankApp.Domain.Entities;
using BankApp.Domain.Strategies;


using Microsoft.EntityFrameworkCore;

namespace BankApp.Application.Loans;

public class LoanApplicationService
{
    private readonly IBankDbContext _context;
    public LoanApplicationService(IBankDbContext context) => _context = context;

    public async Task<Guid> ApplyAndDisburseLoanAsync(Guid userId, ApplyLoanDto dto)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("Wallet not found for the current user.");
        if (!wallet.IsKycVerified) throw new InvalidOperationException("Kyc verificaion is required");
        if (dto.Amount > wallet.LoanLimit) throw new InvalidOperationException("Loan amount is greater than the loan limit");


        decimal defaultRiskInterestRate = 0.14m;
        var loan = new Loan(wallet.Id, dto.Amount, defaultRiskInterestRate, dto.TermInMonths, dto.MethodType);
        var strategy = LoanStrategyFactory.GetStrategy(dto.MethodType);
        loan.GenerateSchedule(strategy);
        loan.Disburse();

        using var txn = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Loans.Add(loan);

            wallet.Credit(dto.Amount);

            var ledgerDisbursalCredit = new LedgerEntry(wallet.Id, dto.Amount, "LoanDispatch", loan.Id.ToString());
            _context.LedgerEntries.Add(ledgerDisbursalCredit);

            var outboxItem = new OutboxMessage("loan-events", $"Loan disbursed successfully to wallet: {wallet.Id} for the volume value: {dto.Amount}");
            _context.OutboxMessages.Add(outboxItem);

            await _context.SaveChangesAsync();
            await txn.CommitAsync();

            return loan.Id;
        }
        catch (System.Exception)
        {
            await txn.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<LoanDto>> GetLoansForUserAsync(Guid userId)
    {
        var walletIds = _context.Wallets.Where(w => w.UserId == userId).Select(w => w.Id);
        var loans = await _context.Loans.AsNoTracking()
            .Include(l => l.AmortizationSchedule)
            .Where(l => walletIds.Contains(l.WalletId))
            .OrderByDescending(l => l.CreatedAtUtc)
            .ToListAsync();
        return loans.Select(ToDto).ToList();
    }

    public async Task<LoanDto?> GetLoanForUserAsync(Guid userId, Guid loanId)
    {
        var walletIds = _context.Wallets.Where(w => w.UserId == userId).Select(w => w.Id);
        var loan = await _context.Loans.AsNoTracking()
            .Include(l => l.AmortizationSchedule)
            .FirstOrDefaultAsync(l => l.Id == loanId && walletIds.Contains(l.WalletId));
        return loan == null ? null : ToDto(loan);
    }

    private static LoanDto ToDto(Loan loan) => new(
        loan.Id, loan.WalletId, loan.PrincipalAmount, loan.InterestRate, loan.TermInMonths,
        loan.RepaymentStrategyType, loan.RemainingBalance, loan.Status, loan.CreatedAtUtc,
        loan.AmortizationSchedule
            .OrderBy(i => i.DueMonthNumber)
            .Select(i => new LoanScheduleItemDto(i.DueMonthNumber, i.DueDateUtc, i.PrincipalDue, i.InterestDue, i.TotalInstallmentAmount, i.IsPaid))
            .ToList());
}
