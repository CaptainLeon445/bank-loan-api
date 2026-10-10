using System;
using System.Collections.Generic;

using BankApp.Domain.Entities;
using BankApp.Domain.Strategies;

namespace BankApp.Domain.Strategies;

public class ReducingBalanceAmortizationStrategy : ILoanAmortizationStrategy
{
    public ICollection<LoanScheduleItem> CalculateSchedule(Loan loan)
    {
        var schedule = new List<LoanScheduleItem>();
        double monthlyRate = (double)(loan.InterestRate / 12m);
        double totalPrincipal = (double)loan.PrincipalAmount;

        // Equated monthly installment (EMI) formula: [P x R x (1+R)^N]/[((1+R)^N)-1]
        double emi = totalPrincipal * monthlyRate * Math.Pow(1 + monthlyRate, loan.TermInMonths) /
             (Math.Pow(1 + monthlyRate, loan.TermInMonths) - 1);

        decimal balance = loan.PrincipalAmount;

        for (int month = 1; month <= loan.TermInMonths; month++)
        {
            decimal interestDue = Math.Round(balance * (loan.InterestRate / 12m), 2);
            decimal principalDue = Math.Round((decimal)emi - interestDue, 2);

            if (month == loan.TermInMonths)
            {
                principalDue = balance;
            }

            balance -= principalDue;
            schedule.Add(new LoanScheduleItem(
                month, principalDue, interestDue, DateTime.UtcNow.AddMonths(month)
            ));
        }

        return schedule;
    }


}

