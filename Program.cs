using Stateless;
using StatelessTest;

var loanStatus = (int)LoanStatusEnum.Created;

var loan = new Loan
{
    LoanStatusId = loanStatus,
    LoanStatus = new LoanStatus
    {
        Id = loanStatus,
        Name = LoanStatusEnum.Created.ToString()
    }
};

var loanExample = new LoanExample();

Console.WriteLine(loanExample.TryToChangeLoanStatus(loan, LoanStatusEnum.Created, Trigger.ManualChangeLO));

//Console.Read();





        
