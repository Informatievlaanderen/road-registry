namespace RoadRegistry.BackOffice.Messages;

using System;
using System.Linq;

public class FileProblem
{
    public string File { get; set; }
    public ProblemParameter[] Parameters { get; set; }
    public string Reason { get; set; }
    public ProblemSeverity Severity { get; set; }

    public string GetParameterValue(string parameterName)
    {
        var parameter = Parameters.SingleOrDefault(x => x.Name == parameterName);
        if (parameter is null)
        {
            throw new ArgumentException($"No parameter found with name '{parameterName}'");
        }
        return parameter.Value;
    }
}
