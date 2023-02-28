using System.Diagnostics;
using Stateless;
using Stateless.Graph;

namespace StatelessTest;

public class LoanExample
{
    /// <summary>
    ///     Func that is invoked during PermitDynamic to check if the newState that we want to change the loan for the given trigger is allowed or not.
    ///     NOTE: PermitDynamic is used when the from the LoanStatus A with Trigger T can go to new status C or D
    /// </summary>
    /// <param name="newLoanStatus">The new status that we want to check</param>
    /// <param name="currentStatus">Used just for error error message</param>
    /// <param name="trigger">Used just for error message</param>
    /// <param name="allowedStates">The states that the newLoanStatus is allowed to be</param>
    /// <returns>the new loan status if the condition is meet: allowedStates.Contains(newLoanStatus)</returns>
    /// <exception cref="InvalidOperationException">if the condition is not meet: allowedStates.Contains(newLoanStatus)</exception>
    private Func<LoanStatusEnum> CheckIfTransitionIsPossibleOrThrowError(LoanStatusEnum newLoanStatus,
        LoanStatusEnum currentStatus, Trigger trigger,
        params LoanStatusEnum[] allowedStates) => () =>
    {
        if (allowedStates.Contains(newLoanStatus)) return newLoanStatus;

        throw new InvalidOperationException(
            $"No valid leaving transitions are permitted from state '{currentStatus}' for trigger '{trigger}' to state '{newLoanStatus}'. Consider ignoring the trigger.");
    };

    private StateMachine<LoanStatusEnum, Trigger> CreateStateMachine(LoanStatusEnum currentStatus,
        LoanStatusEnum newStatus, Trigger trigger)
    {
        var machine = new StateMachine<LoanStatusEnum, Trigger>(() => currentStatus, s => currentStatus = s);

        // configure the machine states and triggers
        machine.Configure(LoanStatusEnum.Created)
            .PermitReentry(Trigger.LenderChange)
            .Permit(Trigger.CallFromPI, LoanStatusEnum.PreFunded)
            .PermitDynamic(Trigger.ManualChangeLO,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.Created, trigger,
                    LoanStatusEnum.Cancelled, LoanStatusEnum.Refused)
            );

        machine.Configure(LoanStatusEnum.PreFunded)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.ManualChangeLO, LoanStatusEnum.Funded)
            .PermitDynamic(Trigger.ManualChangeLO,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.PreFunded, trigger,
                    LoanStatusEnum.Cancelled, LoanStatusEnum.Refused));


        machine.Configure(LoanStatusEnum.Funded)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.ContractSigning, LoanStatusEnum.UnderAcceptance)
            .PermitDynamic(Trigger.ManualChangeLO,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.Funded, trigger,
                    LoanStatusEnum.Cancelled, LoanStatusEnum.Refused));

        machine.Configure(LoanStatusEnum.UnderAcceptance)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.Accepted, LoanStatusEnum.Accepted)
            .PermitDynamic(Trigger.ManualChangeLO,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.UnderAcceptance, trigger,
                    LoanStatusEnum.Cancelled, LoanStatusEnum.Refused));

        machine.Configure(LoanStatusEnum.Accepted)
            .Permit(Trigger.LenderChange, LoanStatusEnum.Created)
            .Permit(Trigger.ManualChangeLO, LoanStatusEnum.WaitingForDisbursement)
            .PermitDynamic(Trigger.ManualChangeLO,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.Accepted, trigger,
                    LoanStatusEnum.Cancelled, LoanStatusEnum.Refused));


        machine.Configure(LoanStatusEnum.WaitingForDisbursement)
            .Permit(Trigger.CallFromPI, LoanStatusEnum.Disbursed);

        machine.Configure(LoanStatusEnum.Disbursed)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.PrePaid)
            .PermitDynamic(Trigger.Workflow,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.Disbursed, trigger,
                    LoanStatusEnum.InArrears, LoanStatusEnum.Current));

        machine.Configure(LoanStatusEnum.InArrears)
            .Permit(Trigger.LastInstallmentPayment, LoanStatusEnum.Repaid)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.PrePaid)
            .PermitDynamic(Trigger.Workflow,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.InArrears, trigger,
                    LoanStatusEnum.Current, LoanStatusEnum.Defaulted));

        machine.Configure(LoanStatusEnum.Current)
            .Permit(Trigger.LastInstallmentPayment, LoanStatusEnum.Repaid)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.PrePaid)
            .Permit(Trigger.Workflow, LoanStatusEnum.InArrears);


        machine.Configure(LoanStatusEnum.Defaulted)
            .Permit(Trigger.LastInstallmentPayment, LoanStatusEnum.Repaid)
            .Permit(Trigger.EmporiumAfterNotificationFromPI, LoanStatusEnum.Guaranteed)
            .PermitDynamic(Trigger.Workflow,
                CheckIfTransitionIsPossibleOrThrowError(newStatus, LoanStatusEnum.Defaulted, trigger,
                    LoanStatusEnum.WriteOff, LoanStatusEnum.InArrears));

        return machine;
    }


    public bool TryToChangeLoanStatus(Loan loan, LoanStatusEnum newLoanStatus, Trigger trigger)
    {
        try
        {
            var machine = CreateStateMachine((LoanStatusEnum)loan.LoanStatus.Id, newLoanStatus, trigger);
            machine.OnTransitioned(t =>
                Console.WriteLine($"Transition from: '{t.Source}' to: '{t.Destination}' with trigger: '{t.Trigger}'"));

            machine.Fire(trigger);

            if (!machine.IsInState(newLoanStatus))
                throw new Exception(
                    $"Transition from: '{(LoanStatusEnum)loan.LoanStatus.Id}' with trigger: '{trigger}' dont take you to status: '{newLoanStatus}' ");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
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
    Accepted
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