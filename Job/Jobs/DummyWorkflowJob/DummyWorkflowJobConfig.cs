using Job.JobConfig;

namespace Job.Jobs.DummyWorkflowJob;

public class DummyWorkflowJobConfig : IJobConfig<DummyWorfklowJob>
{
    public string ImageName => "dummy-workflow-job";

    public IEnumerable<string> EnvironmentVariables =>
    [
        "DUMMY_WORKFLOW_JOB_ENV=dummy-workflow-job-env"
    ];
}