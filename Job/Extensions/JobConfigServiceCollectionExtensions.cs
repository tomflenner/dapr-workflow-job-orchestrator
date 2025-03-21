using Job.JobConfig;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Extensions;

public static class JobConfigExtensions
{
    public static IServiceCollection AddJobConfigs(this IServiceCollection services)
    {
        var jobConfigTypes = AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobConfig<>)))
            .ToList();

        foreach (var configType in jobConfigTypes)
        {
            var jobType = configType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobConfig<>))
                .GetGenericArguments()[0];

            services.AddKeyedSingleton(typeof(IJobConfig<>).MakeGenericType(jobType), jobType.Name, configType);
        }

        return services;
    }
}