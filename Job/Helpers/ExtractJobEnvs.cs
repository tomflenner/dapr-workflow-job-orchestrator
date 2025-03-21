using System.Reflection;
using Job.Jobs;

namespace Job.Helpers;

public static class ExtractJobEnvs
{
    public static List<string> ExtractJobProperties<T>(T jobInstance) where T : IJob
    {
        var envVariables = new List<string>();

        // Use jobInstance.GetType() to ensure all properties are detected
        var properties = jobInstance.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(jobInstance)?.ToString();
            if (value != null)
            {
                envVariables.Add($"{property.Name.ToUpper()}={value}");
            }
        }

        return envVariables;
    }
}