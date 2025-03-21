using Dapr.Workflow;
using Job.Jobs.DummyWorkflowJob;
using Workflow.Activities;

namespace Workflow.Workflow;

public class DummyWorkflow : Workflow<DummyWorflowInput, DummyWorkflowOutput>
{
    public override async Task<DummyWorkflowOutput> RunAsync(WorkflowContext context, DummyWorflowInput input)
    {
        var logger = context.CreateReplaySafeLogger<DummyWorkflow>();

        logger.LogInformation("Starting workflow {WorkflowId} with message {Message}...", input.WorkflowId,
            input.Message);

        var camelCaseMessage = await context.CallActivityAsync<string>(nameof(CamelCaseActivity), input);

        input = input with { Message = camelCaseMessage };

        var scheduledJobId = await context.CallActivityAsync<Guid>(nameof(DummyWorkflowJobActivity), input);

        logger.LogInformation("Job {JobId} scheduled successfully waiting for job to complete...", scheduledJobId);

        var jobResult =
            await context.WaitForExternalEventAsync<DummyWorkflowJobResult>(scheduledJobId.ToString(), TimeSpan.FromMinutes(5));

        logger.LogInformation("Received transformed message {Message} from job {JobId}...", jobResult.Message,
            scheduledJobId);

        return new DummyWorkflowOutput(jobResult.Message);
    }
}