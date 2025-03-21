using Job.WorkflowJob;

namespace Job.Jobs.DummyWorkflowJob;

public sealed record DummyWorfklowJob(Guid WorkflowId, Guid JobId, string Message) : WorfklowJob<DummyWorkflowJobResult>(WorkflowId, JobId);