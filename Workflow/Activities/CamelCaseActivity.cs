using System.Globalization;
using Dapr.Workflow;
using Workflow.Workflow;

namespace Workflow.Activities;

public class CamelCaseActivity(ILogger<CamelCaseActivity> logger) : WorkflowActivity<DummyWorflowInput, string>
{
    public override Task<string> RunAsync(WorkflowActivityContext context, DummyWorflowInput input)
    {
        logger.LogInformation("Received message {Message} and convert it to camelCase...", input.Message);

        var camelCase = ToCamelCase(input.Message);

        logger.LogInformation("Converted message to camelCase {Message}...", camelCase);

        return Task.FromResult(camelCase);
    }

    private static string ToCamelCase(string text)
    {
        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        var camelCase = textInfo.ToTitleCase(text).Replace(" ", "").ToCharArray();
        camelCase[0] = char.ToLower(camelCase[0]);
        return new string(camelCase);
    }
}