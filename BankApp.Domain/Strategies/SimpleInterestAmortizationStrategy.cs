using System;
using System.Collections.Generic;

using BankApp.Domain.Entities;
using BankApp.Domain.Strategies;

namespace BankApp.Domain.Strategies;

public class SimpleInterestAmortizationStrategy : ILoanAmortizationStrategy
{
    public ICollection<LoanScheduleItem> CalculateSchedule(Loan loan)
    {
        var schedule = new List<LoanScheduleItem>();
        decimal monthlyPrincipal = Math.Round(loan.PrincipalAmount / loan.TermInMonths, 2);
        decimal totalInterest = loan.PrincipalAmount * loan.InterestRate * (loan.TermInMonths / 12m);
        decimal monthlyInterest = Math.Round(totalInterest / loan.TermInMonths, 2);

        for (int month = 1; month <= loan.TermInMonths; month++)
        {
            if (month == loan.TermInMonths)
            {
                monthlyPrincipal = loan.PrincipalAmount - (monthlyPrincipal * (loan.TermInMonths - 1));
                monthlyInterest = totalInterest - (monthlyInterest * (loan.TermInMonths - 1));
            }
            schedule.Add(
                new LoanScheduleItem(
                    month, monthlyPrincipal, monthlyInterest, DateTime.UtcNow.AddMonths(month)
                )
            );
        }
        return schedule;
    }


}

