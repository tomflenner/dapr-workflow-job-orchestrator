using Job.Jobs;

namespace Job.WorkflowJobResult;

public record WorkflowJobResult(Guid WorkflowId, Guid JobId) : IJobResult
{
    public Guid JobId { get; set; } = JobId;
}