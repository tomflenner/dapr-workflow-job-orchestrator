using Job.Jobs;

namespace Job.JobScheduler;

public interface IJobScheduler
{
    Task<bool> ScheduleJobAsync(IJob job, CancellationToken cancellationToken = default);

    Task<bool> ScheduleWorkflowJobAsync(IJob job, CancellationToken cancellationToken = default);
}