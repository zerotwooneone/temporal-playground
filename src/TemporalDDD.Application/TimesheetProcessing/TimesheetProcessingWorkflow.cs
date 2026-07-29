using Temporalio.Workflows;
using TemporalDDD.Application.TimesheetProcessing;

namespace TemporalDDD.Application.TimesheetProcessing;

[Workflow]
public class TimesheetProcessingWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(Guid providerId, DateTime periodStart, DateTime periodEnd, decimal totalHours, decimal hourlyRate, decimal facilityBillRate)
    {
        // Step 1: Validate Timesheet Rules
        var timesheetId = Workflow.NewGuid();
        await Workflow.ExecuteActivityAsync(
            (ITimesheetProcessingActivities activities) => activities.ValidateTimesheetRulesAsync(timesheetId),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 2: Calculate Payroll and Taxes
        var payrollResult = await Workflow.ExecuteActivityAsync(
            (ITimesheetProcessingActivities activities) => activities.CalculatePayrollAndTaxesAsync(timesheetId),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 3: Submit Bank Transfer with Idempotency Key
        // IMPORTANT: Pass Workflow.Info.WorkflowId as idempotency key for payment gateway
        var idempotencyKey = Workflow.Info.WorkflowId;
        var paymentReference = await Workflow.ExecuteActivityAsync(
            (ITimesheetProcessingActivities activities) => activities.SubmitBankTransferAsync(timesheetId, idempotencyKey),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(10) }
        );

        Console.WriteLine($"[TimesheetProcessing] Payment submitted with reference: {paymentReference}");

        // Step 4: Generate and Send Invoice to ERP System
        var invoiceNumber = await Workflow.ExecuteActivityAsync(
            (ITimesheetProcessingActivities activities) => activities.GenerateAndSendInvoiceAsync(timesheetId, facilityBillRate),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        Console.WriteLine($"[TimesheetProcessing] Invoice generated: {invoiceNumber}");
    }
}
