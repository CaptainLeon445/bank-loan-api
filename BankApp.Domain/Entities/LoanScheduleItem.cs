using System;
using System.Collections.Generic;

namespace BankApp.Domain.Entities;

public class LoanScheduleItem
{
    public Guid Id { get; private set; }
    public Guid LoanId { get; private set; }
    public decimal PrincipalDue { get; private set; }
    public decimal InterestDue { get; private set; }
    public int DueMonthNumber { get; private set; }
    public decimal TotalInstallmentAmount => PrincipalDue + InterestDue;
    public bool IsPaid { get; private set; }
    public DateTime DueDateUtc { get; private set; }

    public LoanScheduleItem(int dueMonthNumber, decimal principalDue, decimal interestDue, DateTime dueDateUtc)
    {

        Id = Guid.NewGuid();
        DueMonthNumber = dueMonthNumber;
        PrincipalDue = principalDue;
        InterestDue = interestDue;
        DueDateUtc = dueDateUtc;
        IsPaid = false;
    }

}