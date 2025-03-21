using Dapr.Workflow;
using Job.Extensions;
using Job.JobConfig;
using Job.Jobs.DummyWorkflowJob;
using Job.JobScheduler;
using Job.Resolver;
using Job.WorkflowJobResult;
using Workflow;
using Workflow.Activities;
using Workflow.Workflow;

var builder = WebApplication.CreateBuilder(args);

var k8s = Environment.GetEnvironmentVariable("K8S") == "true";
if (k8s)
    builder.Services.AddScoped<IJobScheduler, K8sJobScheduler>();
else
    builder.Services.AddScoped<IJobScheduler, DockerJobScheduler>();


builder.Services.AddScoped<IJobConfigRegistry, JobConfigRegistry>();
builder.Services.AddJobConfigs();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new PolymorphicTypeResolver());
});

builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<DummyWorkflow>();
    options.RegisterActivity<CamelCaseActivity>();
    options.RegisterActivity<DummyWorkflowJobActivity>();
});

var app = builder.Build();

app.MapPost("/workflow", async (WorkflowPayload payload, DaprWorkflowClient workflowClient, ILogger<Program> logger) =>
{
    try
    {
        var workflowId = Guid.NewGuid();
        var input = new DummyWorflowInput(workflowId, payload.Message);

        logger.LogInformation("Starting workflow {WorkflowId} with message {Message}...", workflowId, payload.Message);
        await workflowClient.ScheduleNewWorkflowAsync(nameof(DummyWorkflow), workflowId.ToString(), input);
        logger.LogInformation("Workflow {WorkflowId} scheduled successfully.", workflowId);

        return Results.Ok(new
        {
            WorkflowId = workflowId
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to start workflow...");
        return Results.InternalServerError(ex);
    }
});

app.MapGet("/workflow/{workflowId:guid}",
    async (Guid workflowId, DaprWorkflowClient workflowClient, ILogger<Program> logger) =>
    {
        try
        {
            var state = await workflowClient.GetWorkflowStateAsync(workflowId.ToString());

            if (state == null) return Results.NotFound();

            if (state.RuntimeStatus != WorkflowRuntimeStatus.Completed)
                return Results.Accepted(value: "Workflow is in state " + state.RuntimeStatus);

            var result = state.ReadOutputAs<DummyWorkflowOutput>();

            await workflowClient.TerminateWorkflowAsync(workflowId.ToString());

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get workflow state...");
            return Results.InternalServerError(ex);
        }
    });

app.MapPost("/job-terminated",
    async (WorkflowJobResult job, DaprWorkflowClient workflowClient, ILogger<Program> logger) =>
    {
        try
        {
            await workflowClient.RaiseEventAsync(job.WorkflowId.ToString(), job.JobId.ToString(), job);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle job termination...");
            return Results.InternalServerError(ex);
        }
    });

await app.RunAsync();