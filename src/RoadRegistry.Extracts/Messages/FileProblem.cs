namespace RoadRegistry.Extracts.Messages;

using System;
using System.Linq;
using RoadRegistry.Infrastructure.Messages;

public class FileProblem
{
    public string File { get; set; }
    public ProblemParameter[] Parameters { get; set; }
    public string Reason { get; set; }
    public ProblemSeverity Severity { get; set; }

    public string? GetParameterValue(string parameterName, bool required = true)
    {
        var parameter = Parameters.SingleOrDefault(x => string.Equals(x.Name, parameterName, StringComparison.InvariantCultureIgnoreCase));
        if (parameter is null)
        {
            if (required)
            {
                throw new ArgumentException($"No parameter found with name '{parameterName}'");
            }

            return null;
        }

        return parameter.Value;
    }
}
