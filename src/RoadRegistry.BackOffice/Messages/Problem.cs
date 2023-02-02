namespace RoadRegistry.BackOffice.Messages;

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
}
