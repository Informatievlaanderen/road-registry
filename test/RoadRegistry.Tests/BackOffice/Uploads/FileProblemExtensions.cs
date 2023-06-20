namespace RoadRegistry.Tests.BackOffice.Uploads
{
    using System.Text;
    using RoadRegistry.BackOffice.Uploads;

    public static class FileProblemExtensions
    {
        public static string Describe(this FileProblem problem)
        {
            var sb = new StringBuilder();
            sb.Append($"{problem.File}: {problem.Reason}");

            if (problem.Parameters.Any())
            {
                sb.Append($" -> {string.Join(", ", problem.Parameters.Select(parameter => $"{parameter.Name}={parameter.Value}"))}");
            }

            return sb.ToString();
        }
    }
}
