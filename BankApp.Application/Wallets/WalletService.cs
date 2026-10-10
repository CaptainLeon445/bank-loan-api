using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BankApp.Domain.Entities;
using BankApp.Application.Common;
using BankApp.Application.DTOs;

namespace BankApp.Application.Wallets;

public class WalletService
{
    private readonly IBankDbContext _context;
    public WalletService(IBankDbContext context) => _context = context;

    public async Task<WalletDto?> GetForUserAsync(Guid userId)
    {
        var wallet = await _context.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId);
        return wallet == null ? null : ToDto(wallet);
    }

    // Demo stand-in for a real KYC provider callback: marks the caller's wallet as verified.
    public async Task<WalletDto?> VerifyKycAsync(Guid userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) return null;

        wallet.MarkKycVerified();
        await _context.SaveChangesAsync();
        return ToDto(wallet);
    }

    internal static WalletDto ToDto(Wallet wallet) =>
        new(wallet.Id, wallet.UserId, wallet.Balance, wallet.LoanLimit, wallet.IsKycVerified, wallet.CreatedAtUtc);
}
