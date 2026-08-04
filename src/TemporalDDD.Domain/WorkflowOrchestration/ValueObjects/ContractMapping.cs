using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record ContractMapping
{
    public bool ConvertXmlToJson { get; }
    public string? QueryParameters { get; }
    public string? RequestMapping { get; }
    public string? ResponseMapping { get; }

    private ContractMapping(bool convertXmlToJson, string? queryParameters, string? requestMapping, string? responseMapping)
    {
        ConvertXmlToJson = convertXmlToJson;
        QueryParameters = queryParameters;
        RequestMapping = requestMapping;
        ResponseMapping = responseMapping;
    }

    public static Result<ContractMapping> Create(bool convertXmlToJson, string? queryParameters, string? requestMapping, string? responseMapping)
    {
        return Result<ContractMapping>.Success(new ContractMapping(convertXmlToJson, queryParameters, requestMapping, responseMapping));
    }

    public override string ToString() => $"ConvertXmlToJson: {ConvertXmlToJson}, QueryParameters: {QueryParameters}";
}
