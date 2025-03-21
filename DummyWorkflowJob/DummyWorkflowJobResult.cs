using System.Text.Json.Serialization;

namespace DummyWorkflowJob;

public record DummyWorkflowJobResult(
    [property: JsonPropertyName("$type")] string Type,
    Guid WorkflowId,
    Guid JobId,
    string Message)
{
    public override string ToString()
    {
        return "DummyWorkflowJobResult{" +
               $"Type='{Type}', " +
               $"WorkflowId={WorkflowId}, " +
               $"JobId={JobId}, " +
               $"Message='{Message}'" +
               "}";
    }
}