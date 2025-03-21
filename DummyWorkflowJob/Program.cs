using System.Net.Http.Json;
using Dapr.Client;
using DummyWorkflowJob;

var daprClient = new DaprClientBuilder().Build();
Console.WriteLine("Waiting for Dapr sidecar to be ready...");

try
{
    await daprClient.WaitForSidecarAsync();
}
catch (Exception e)
{
    Console.WriteLine($"Error during health check: {e.Message}");
    Environment.Exit(1);
}

// Doing work
var workflowId = Environment.GetEnvironmentVariable("WORKFLOWID");
var jobId = Environment.GetEnvironmentVariable("JOBID");
var jobResultType = Environment.GetEnvironmentVariable("JOBRESULTTYPE");
var message = Environment.GetEnvironmentVariable("MESSAGE");

message += " YAY DUMMY";

DummyWorkflowJobResult jobResult = new(jobResultType, Guid.Parse(workflowId), Guid.Parse(jobId), message);

Console.WriteLine(jobResult);

try
{
    var httpClient = daprClient.CreateInvokableHttpClient("workflow-app");
    Console.WriteLine("Sending job terminated event.");
    var result = await httpClient.PostAsJsonAsync("job-terminated", jobResult);
    result.EnsureSuccessStatusCode();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    await daprClient.ShutdownSidecarAsync();
    Environment.Exit(1);
}

await daprClient.ShutdownSidecarAsync();
Console.WriteLine("Job done.");