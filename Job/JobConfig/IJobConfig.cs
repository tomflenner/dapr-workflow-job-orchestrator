using Job.Jobs;

namespace Job.JobConfig;

public interface IJobConfig<out TJob> where TJob : IJob
{
    string ImageName { get; }
    IEnumerable<string> EnvironmentVariables { get; }
}