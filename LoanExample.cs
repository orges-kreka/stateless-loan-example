using Stateless;
using Stateless.Graph;

namespace StatelessTest;

public class LoanExample
{
    private readonly StateMachine<LoanStatusEnum, Trigger> _machine;

    private readonly StateMachine<LoanStatusEnum, Trigger>.TriggerWithParameters<LoanStatusEnum> _statusChangedManuallyFromLoanOfficerTrigger;
    private readonly StateMachine<LoanStatusEnum, Trigger>.TriggerWithParameters<LoanStatusEnum> _statusChangedFromWorkflowTrigger;
    
    public LoanExample()
    {
        _machine = new StateMachine<LoanStatusEnum, Trigger>(LoanStatusEnum.Created);
        
          _statusChangedManuallyFromLoanOfficerTrigger = _machine.SetTriggerParameters<LoanStatusEnum>(Trigger.ManualChangeLO);
         _statusChangedFromWorkflowTrigger = _machine.SetTriggerParameters<LoanStatusEnum>(Trigger.Workflow);

        
        // configure the machine states and triggers
        _machine.Configure(LoanStatusEnum.Created)
            .PermitReentry(Trigger.LenderChange)
            .Permit(Trigger.CallFromPI, LoanStatusEnum.PreFunded)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Cancelled,
                (newStatus) =>
                {
                    Console.WriteLine(newStatus);
                    return newStatus == LoanStatusEnum.Cancelled;
                })
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Refused,
                (newStatus) => newStatus == LoanStatusEnum.Refused);
        
        /*
        _machine.Configure(LoanStatusEnum.PreFunded)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.ManualChangeLO, LoanStatusEnum.Funded)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Cancelled,
                (newStatus) => newStatus == LoanStatusEnum.Cancelled)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Refused,
                (newStatus) => newStatus == LoanStatusEnum.Refused);
        
        _machine.Configure(LoanStatusEnum.Funded)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.ContractSigning, LoanStatusEnum.UnderAcceptance)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Cancelled,
                (newStatus) => newStatus == LoanStatusEnum.Cancelled)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Refused,
                (newStatus) => newStatus == LoanStatusEnum.Refused);
        
        _machine.Configure(LoanStatusEnum.UnderAcceptance)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.Accepted, LoanStatusEnum.Accepted)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Cancelled,
                (newStatus) => newStatus == LoanStatusEnum.Cancelled)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Refused,
                (newStatus) => newStatus == LoanStatusEnum.Refused);
        
        _machine.Configure(LoanStatusEnum.Accepted)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.ManualChangeLO, LoanStatusEnum.WaitingForDisbursement)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Cancelled,
                (newStatus) => newStatus == LoanStatusEnum.Cancelled)
            .PermitIf(_statusChangedManuallyFromLoanOfficerTrigger, LoanStatusEnum.Refused,
                (newStatus) => newStatus == LoanStatusEnum.Refused);

        _machine.Configure(LoanStatusEnum.WaitingForDisbursement)
            .Permit(Trigger.CallFromPI, LoanStatusEnum.Disbursed);

        _machine.Configure(LoanStatusEnum.Disbursed)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.PrePaid)
            .PermitIf(_statusChangedFromWorkflowTrigger, LoanStatusEnum.InArrears,
                newStatus => newStatus == LoanStatusEnum.InArrears)
            .PermitIf(_statusChangedFromWorkflowTrigger, LoanStatusEnum.Current,
                newStatus => newStatus == LoanStatusEnum.Current);

        _machine.Configure(LoanStatusEnum.InArrears)
            .Permit(Trigger.LastInstallmentPayment, LoanStatusEnum.Repaid)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.PrePaid)
            .PermitIf(_statusChangedFromWorkflowTrigger, LoanStatusEnum.Current,
                newStatus => newStatus == LoanStatusEnum.Current)
            .PermitIf(_statusChangedFromWorkflowTrigger, LoanStatusEnum.Defaulted,
                newStatus => newStatus == LoanStatusEnum.Defaulted);
        
        _machine.Configure(LoanStatusEnum.Current)
            .Permit(Trigger.LastInstallmentPayment, LoanStatusEnum.Repaid)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.PrePaid)
            .Permit(Trigger.Workflow, LoanStatusEnum.InArrears);


        _machine.Configure(LoanStatusEnum.Defaulted)
            .Permit(Trigger.LastInstallmentPayment, LoanStatusEnum.Repaid)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.Guaranteed)
            .PermitIf(_statusChangedFromWorkflowTrigger, LoanStatusEnum.InArrears,
                (newStatus) => newStatus == LoanStatusEnum.InArrears)
            .PermitIf(_statusChangedFromWorkflowTrigger, LoanStatusEnum.WriteOff,
                (newStatus) => newStatus == LoanStatusEnum.WriteOff);
        */
    }


    public bool TryToChangeLoanStatus(Loan loan, LoanStatusEnum newLoanStatus, Trigger trigger)
    {
        //var triggerWithParameters = _machine.SetTriggerParameters<LoanStatusEnum>(trigger);

        try
        {
            _machine.OnTransitioned( t => Console.WriteLine($"Transition from: '{t.Source}' to: '{t.Destination}' with trigger: '{t.Trigger}'"));
            
             _machine.Fire(trigger);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
    
    public string MachineGraph =>  UmlDotGraph.Format(_machine.GetInfo());
    
}

public enum Trigger
{
    LenderChange,
    ManualChangeLO,
    Workflow,
    EmporiumAfterNotificationFromPI,
    LastInstallmentPayment,
    CallFromPI,
    ContractSigning,
    Accepted,
}


public enum LoanStatusEnum
{
    /// <summary>
    /// Created
    /// </summary>
    Created = 1,

    /// <summary>
    /// Closing
    /// </summary>
    Closing = 2,

    /// <summary>
    /// Pre-Funded
    /// </summary>
    PreFunded = 3,

    /// <summary>
    /// Funded
    /// </summary>
    Funded = 4,

    /// <summary>
    /// Accepting
    /// </summary>
    UnderAcceptance = 5,

    /// <summary>
    /// Accepted
    /// </summary>
    Accepted = 6,

    /// <summary>
    /// Refused
    /// </summary>
    Rejected = 7,

    /// <summary>
    /// Disbursed
    /// </summary>
    Disbursed = 8,

    /// <summary>
    /// Current
    /// </summary>
    Current = 9,

    /// <summary>
    /// In arrears
    /// </summary>
    InArrears = 10,

    /// <summary>
    /// Defaulted
    /// </summary>
    Defaulted = 11,

    /// <summary>
    /// Repaid
    /// </summary>
    Repaid = 12,

    /// <summary>
    /// Pre-paid
    /// </summary>
    PrePaid = 13,

    /// <summary>
    /// Partially Covered
    /// </summary>
    PartiallyCovered = 14,

    /// <summary>
    /// Guaranteed
    /// </summary>
    Guaranteed = 15,

    /// <summary>
    /// Write-off
    /// </summary>
    WriteOff = 16,

    /// <summary>
    /// Timeout
    /// </summary>
    Timeout = 17,

    /// <summary>
    /// Erased
    /// </summary>
    Erased = 18,
    
    Cancelled = 19,
    
    Refused = 20,
    
    WaitingForDisbursement
}



 public class Loan 
    {
        public decimal RequestedAmount { get; set; }
        public decimal FinancedAmount { get; set; }
        public decimal? DisbursedAmount { get; set; }
        public decimal OutstandingAmount { get; set; }        
        public int? ReferenceRateId { get; set; }
        public int AmortizationPeriod { get; set; }
        public int PreAmortizationPeriod { get; set; }
        public double BrokerageFee { get; set; }
        public decimal? RiskFee { get; set; }
        public decimal? PaymentFee { get; set; }
        public double APR { get; set; }
        public int ProductId { get; set; }
        public int ApplicationId { get; set; }
        public int LenderId { get; set; }
        public int LoanStatusId { get; set; }
        public int? FDGGuaranteeId { get; set; }
        public DateTime StatusChangingTime { get; set; }
        public int? StatusUpdatedUserId { get; set; }
        public Guid TenantId { get; set; }
        public LoanStatus LoanStatus { get; set; }
        public double InitialExpenses { get; set; }
        public double RecurringExpenses { get; set; }
        public double PaymentInstitutionCommission { get; set; }
        public int RemainingAmortizationPeriod { get; set; }
        public int RemainingPreAmortizationPeriod { get; set; }
        public DateTime? DateDisbursed { get; set; }
        public int? UpdatedById { get; set; }
        public int CreatedById { get; set; }
    }

public class LoanStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
}