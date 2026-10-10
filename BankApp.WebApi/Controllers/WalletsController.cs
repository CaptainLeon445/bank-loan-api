using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BankApp.Application.DTOs;
using BankApp.Application.Wallets;
using BankApp.WebApi.Extensions;

namespace BankApp.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly WalletService _walletService;

    public WalletsController(WalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<WalletDto>> GetMyWallet()
    {
        var wallet = await _walletService.GetForUserAsync(User.GetUserId());
        if (wallet == null) return NotFound(new { Message = "Wallet not found." });
        return Ok(wallet);
    }

    // Demo only: lets the caller mark their own wallet as KYC-verified until a real KYC provider is integrated.
    [HttpPost("me/verify-kyc")]
    public async Task<ActionResult<WalletDto>> VerifyKyc()
    {
        var wallet = await _walletService.VerifyKycAsync(User.GetUserId());
        if (wallet == null) return NotFound(new { Message = "Wallet not found." });
        return Ok(wallet);
    }
}
