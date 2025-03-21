namespace Job.Jobs.DummyWorkflowJob;

public record DummyWorkflowJobResult(Guid WorkflowId, Guid JobId, string Message)
    : WorkflowJobResult.WorkflowJobResult(WorkflowId, JobId);