using System;
using System.Collections.Generic;
using BankApp.Domain.Entities;

namespace BankApp.Domain.Strategies;

public interface ILoanAmortizationStrategy
{
    ICollection<LoanScheduleItem> CalculateSchedule(Loan loan);
}

