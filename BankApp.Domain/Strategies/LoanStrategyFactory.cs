using System;
using BankApp.Domain.Strategies;

namespace BankApp.Domain.Strategies;

public static class LoanStrategyFactory
{
    public static ILoanAmortizationStrategy GetStrategy(string type) => type.ToUpper() switch
    {
        "SIMPLE" => new SimpleInterestAmortizationStrategy(),
        "REDUCING" => new ReducingBalanceAmortizationStrategy(),
        _ => throw new ArgumentException("Unknown amortization framework configuration profile.")

    };
}