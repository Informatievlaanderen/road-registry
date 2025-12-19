namespace RoadRegistry.Infrastructure.Messages;

using System;
using System.Linq;
using System.Text;

public class Problem
{
    public ProblemParameter[] Parameters { get; set; }
    public string Reason { get; set; }
    public ProblemSeverity Severity { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{nameof(Severity)}: {Severity}");
        sb.AppendLine($"{nameof(Reason)}: {Reason}");

        if (Parameters?.Length > 0)
        {
            sb.AppendLine($"{nameof(Parameters)}:");
            foreach (var parameter in Parameters)
            {
                sb.AppendLine($"- {parameter.Name}={parameter.Value}");
            }
        }

        return sb.ToString();
    }

    public string GetParameterValue(string parameterName)
    {
        var value = GetOptionalParameterValue(parameterName);
        if (value is null)
        {
            throw new ArgumentException($"No parameter found with name '{parameterName}'");
        }

        return value;
    }
    public string? GetOptionalParameterValue(string parameterName)
    {
        return Parameters
            .SingleOrDefault(x => string.Equals(x.Name, parameterName, StringComparison.InvariantCultureIgnoreCase))
            ?.Value;
    }

    public bool HasParameter(string parameterName)
    {
        return Parameters.Any(x => string.Equals(x.Name, parameterName, StringComparison.InvariantCultureIgnoreCase));
    }
}
