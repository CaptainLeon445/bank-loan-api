using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankApp.Application.DTOs;

public record AuthResponseDto(
    string Token, Guid UserId, string Email
);

public record ApplyLoanDto(
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Amount,
    [Range(1, 360)] int TermInMonths,
    [Required] string MethodType
);

public record WalletDto(
    Guid Id, Guid UserId, decimal Balance, decimal LoanLimit, bool IsKycVerified, DateTime CreatedAtUtc
);

public record LoanScheduleItemDto(
    int DueMonthNumber, DateTime DueDateUtc, decimal PrincipalDue, decimal InterestDue, decimal TotalInstallmentAmount, bool IsPaid
);

public record LoanDto(
    Guid Id, Guid WalletId, decimal PrincipalAmount, decimal InterestRate, int TermInMonths,
    string RepaymentStrategyType, decimal RemainingBalance, string Status, DateTime CreatedAtUtc,
    IReadOnlyList<LoanScheduleItemDto> Schedule
);
