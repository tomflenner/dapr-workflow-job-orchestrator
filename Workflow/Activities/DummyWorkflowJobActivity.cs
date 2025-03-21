using Dapr.Workflow;
using Job.Jobs.DummyWorkflowJob;
using Job.JobScheduler;
using Workflow.Workflow;

namespace Workflow.Activities;

public class DummyWorkflowJobActivity(ILogger<DummyWorkflowJobActivity> logger, IJobScheduler jobScheduler) : WorkflowActivity<DummyWorflowInput, Guid>
{
    public override async Task<Guid> RunAsync(WorkflowActivityContext context, DummyWorflowInput input)
    {
        logger.LogInformation("Received message {Message} and schedule a Dummy workflow job...", input.Message);

        var jobId = Guid.NewGuid();
        var job = new DummyWorfklowJob(input.WorkflowId, jobId, input.Message);

        try
        {
            await jobScheduler.ScheduleWorkflowJobAsync(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to schedule Dummy workflow job {JobType}...", job.GetType().Name);
            throw;
        }

        logger.LogInformation("Dummy workflow job {JobId} scheduled successfully...", jobId);

        return job.JobId;
    }
}