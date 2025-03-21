using Docker.DotNet;
using Docker.DotNet.Models;
using Job.Helpers;
using Job.JobConfig;
using Job.Jobs;
using Microsoft.Extensions.Logging;

namespace Job.JobScheduler;

public class DockerJobScheduler(ILogger<DockerJobScheduler> logger, IJobConfigRegistry configRegistry) : IJobScheduler
{
    private readonly DockerClient _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
        .CreateClient();

    private async Task<CreateContainerResponse> CreateSidecarContainerAsync(string jobContainerName,
        CancellationToken cancellationToken)
    {
        const string daprImage = "daprio/daprd:latest";
        var daprContainerName = $"{jobContainerName}-sidecar";

        var daprContainerResponse = await _dockerClient.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                Image = daprImage,
                Name = daprContainerName,
                Env = ["DAPR_HTTP_PORT=3500"],
                Cmd = ["./daprd", "-app-id", "dummy-job", "-log-level", "warn"],
                HostConfig = new HostConfig
                {
                    AutoRemove = false,
                    NetworkMode = $"container:{jobContainerName}"
                }
            }, cancellationToken);
        return daprContainerResponse;
    }

    private async Task<CreateContainerResponse> CreateJobContainerAsync<T>(T job, IJobConfig<T> jobConfig,
        string networkName, CancellationToken cancellationToken) where T : IJob
    {
        var appContainerResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = jobConfig.ImageName,
            Name = GetJobContainerName(job),
            Env =
            [
                ..jobConfig.EnvironmentVariables,
                ..ExtractJobEnvs.ExtractJobProperties(job),
                "DAPR_HTTP_PORT=3500"
            ],
            HostConfig = new HostConfig
            {
                AutoRemove = false,
                NetworkMode = networkName
            }
        }, cancellationToken);

        return appContainerResponse;
    }


    private async Task EnsureNetworkExistsAsync(string networkName, CancellationToken cancellationToken)
    {
        var networks = await _dockerClient.Networks.ListNetworksAsync(null, cancellationToken);
        if (networks.All(n => n.Name != networkName))
            await _dockerClient.Networks.CreateNetworkAsync(new NetworksCreateParameters
            {
                Name = networkName,
                Driver = "bridge"
            }, cancellationToken);
    }

    private async Task EnsureImageExistsAsync(string imageName, CancellationToken cancellationToken)
    {
        var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters(), cancellationToken);
        if (images.All(img => img.RepoTags?.Any(tag => tag.StartsWith(imageName)) != true))
            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName },
                null,
                new Progress<JSONMessage>(),
                cancellationToken);
    }

    public Task<bool> ScheduleJobAsync(IJob job, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ScheduleWorkflowJobAsync(IJob job, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobConfig = configRegistry.GetConfig(job);
            var jobContainerName = GetJobContainerName(job);
            const string networkName = "workflow-network";

            await EnsureNetworkExistsAsync(networkName, cancellationToken);
            await EnsureImageExistsAsync(jobConfig.ImageName, cancellationToken);

            // Create the application container
            var appContainerResponse =
                await CreateJobContainerAsync(job, configRegistry.GetConfig(job), networkName, cancellationToken);

            // Create the Dapr sidecar container
            var daprContainerResponse =
                await CreateSidecarContainerAsync(jobContainerName, cancellationToken);

            // Start the application container
            await _dockerClient.Containers.StartContainerAsync(appContainerResponse.ID, null, cancellationToken);

            // Start the Dapr container
            return await _dockerClient.Containers.StartContainerAsync(daprContainerResponse.ID, null,
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong when creating container jobs");
            throw;
        }
    }

    private static string GetJobContainerName(IJob job)
    {
        var jobContainerName = $"{job.GetType().Name.ToLower()}-{job.JobId}";
        return jobContainerName;
    }
}