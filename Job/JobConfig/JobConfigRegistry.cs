using Job.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Job.JobConfig;

public class JobConfigRegistry(IServiceProvider serviceProvider) : IJobConfigRegistry
{
    public IJobConfig<TJob> GetConfig<TJob>(TJob job) where TJob : IJob
    {
        var jobType = job.GetType(); // Get the concrete runtime type of the job
        var configType = typeof(IJobConfig<>).MakeGenericType(jobType); // Construct IJobConfig<TJob>

        // Fetch the service using the specific job type and cast it to IJobConfig<TJob>
        return (IJobConfig<TJob>)serviceProvider.GetRequiredKeyedService(configType, jobType.Name);
    }
}