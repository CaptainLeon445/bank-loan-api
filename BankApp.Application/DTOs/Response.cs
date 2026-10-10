using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BankApp.Domain.Enums;

namespace BankApp.Application.DTOs;

public record AuthResponseDto(
    string Token, Guid UserId, string Email
);

public record WalletDto(
    Guid Id, Guid UserId, decimal Balance, decimal LoanLimit, bool IsKycVerified, DateTime CreatedAtUtc
);

public record LoanScheduleItemDto(
    int DueMonthNumber, DateTime DueDateUtc, decimal PrincipalDue, decimal InterestDue, decimal TotalInstallmentAmount, bool IsPaid
);

public record LoanDto(
    Guid Id, Guid WalletId, decimal PrincipalAmount, decimal InterestRate, int TermInMonths,
    RepaymentStrategyType RepaymentStrategyType, decimal RemainingBalance, LoanStatus Status, DateTime CreatedAtUtc,
    IReadOnlyList<LoanScheduleItemDto> Schedule
);
