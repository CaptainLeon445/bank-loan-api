using System;
using BankApp.Domain.Enums;

namespace BankApp.Domain.Strategies;

public static class LoanStrategyFactory
{
    public static ILoanAmortizationStrategy GetStrategy(RepaymentStrategyType type) => type switch
    {
        RepaymentStrategyType.Simple => new SimpleInterestAmortizationStrategy(),
        RepaymentStrategyType.Reducing => new ReducingBalanceAmortizationStrategy(),
        _ => throw new ArgumentException("Unknown amortization framework configuration profile.")

    };
}
