using StatelessTest;

var loanStatus = (int)LoanStatusEnum.Defaulted;

var loan = new Loan
{
    LoanStatusId = loanStatus,
    LoanStatus = new LoanStatus
    {
        Id = loanStatus,
        Name = ((LoanStatusEnum)loanStatus).ToString()
    }
};

var loanExample = new LoanExample();

Console.WriteLine(loanExample.TryToChangeLoanStatus(loan, LoanStatusEnum.Repaid, Trigger.LastInstallmentPayment));




        
