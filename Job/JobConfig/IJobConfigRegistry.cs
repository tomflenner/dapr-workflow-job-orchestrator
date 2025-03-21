using Job.Jobs;

namespace Job.JobConfig;

public interface IJobConfigRegistry
{
    IJobConfig<TJob> GetConfig<TJob>(TJob job) where TJob : IJob;
}