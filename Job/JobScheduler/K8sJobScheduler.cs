using Job.Jobs;

namespace Job.JobScheduler;

public class K8sJobScheduler : IJobScheduler
{
    public Task<bool> ScheduleJobAsync(IJob job, CancellationToken cancellationToken = default)
    {
        // Implement stuff using K8s .NET SDK.

        throw new NotImplementedException();
    }

    public Task<bool> ScheduleWorkflowJobAsync(IJob job, CancellationToken cancellationToken = default)
    {
        // Implement stuff using K8s .NET SDK.

        throw new NotImplementedException();
    }
}