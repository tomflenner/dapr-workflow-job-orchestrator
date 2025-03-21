using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Job.Resolver;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        var workflowBaseType = typeof(WorkflowJobResult.WorkflowJobResult);

        if (jsonTypeInfo.Type != workflowBaseType) return jsonTypeInfo;

        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
        };

        foreach (var derivedType in GetDerivedTypes<WorkflowJobResult.WorkflowJobResult>())
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);

        return jsonTypeInfo;
    }

    private static IList<JsonDerivedType> GetDerivedTypes<T>() where T : class
    {
        var types = AssemblyReference.Assembly.GetTypes()
            .Where(t => typeof(T).IsAssignableFrom(t) && t != typeof(T) && !t.IsAbstract)
            .Select(t => new JsonDerivedType(t, t.Name))
            .ToList();

        return types;
    }
}