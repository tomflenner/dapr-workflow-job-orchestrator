using Job.Jobs;

namespace Job.WorkflowJob;

public abstract record WorfklowJob<TResult>(Guid WorkflowId, Guid JobId) : IJob where TResult : IJobResult
{
    public Guid JobId { get; set; } = JobId;
    public string JobResultType => typeof(TResult).Name;
}