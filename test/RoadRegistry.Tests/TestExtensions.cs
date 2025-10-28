namespace RoadRegistry.Tests;

using System.Text;
using RoadRegistry.BackOffice.Core;

public static class TestExtensions
{
    public static string Describe(this Problem problem)
    {
        var sb = new StringBuilder();
        sb.Append($"{problem.Reason}");

        if (problem.Parameters.Any())
        {
            sb.Append($" -> {string.Join(", ", problem.Parameters.Select(parameter => $"{parameter.Name}={parameter.Value}"))}");
        }

        return sb.ToString();
    }
}
