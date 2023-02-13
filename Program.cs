using Stateless;
using StatelessTest;


var loan = new Loan
{
    LoanStatus = new LoanStatus
    {
        Id = (int)LoanStatusEnum.Created,
        Name = LoanStatusEnum.Created.ToString()
    }
};

var loanExample = new LoanExample();

Console.WriteLine(loanExample.TryToChangeLoanStatus(loan, LoanStatusEnum.Funded, Trigger.CallFromPI));

//Console.Read();





        
