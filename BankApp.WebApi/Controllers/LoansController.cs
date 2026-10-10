using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using BankApp.Application.DTOs;
using BankApp.Application.Loans;
using BankApp.WebApi.Extensions;

namespace BankApp.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly LoanApplicationService _loanService;

    public LoansController(LoanApplicationService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost]
    public async Task<ActionResult<LoanDto>> Apply([FromBody] ApplyLoanDto dto)
    {
        var userId = User.GetUserId();
        try
        {
            var loanId = await _loanService.ApplyAndDisburseLoanAsync(userId, dto);
            var loan = await _loanService.GetLoanForUserAsync(userId, loanId);
            return CreatedAtAction(nameof(GetById), new { id = loanId }, loan);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            // Business-rule failures from the service and domain (no wallet, no KYC, over limit, bad strategy, bad amount).
            return BadRequest(new { Message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { Message = "The wallet was updated by another request. Please retry." });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetMyLoans()
    {
        return Ok(await _loanService.GetLoansForUserAsync(User.GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanDto>> GetById(Guid id)
    {
        var loan = await _loanService.GetLoanForUserAsync(User.GetUserId(), id);
        if (loan == null) return NotFound(new { Message = "Loan not found." });
        return Ok(loan);
    }
}
