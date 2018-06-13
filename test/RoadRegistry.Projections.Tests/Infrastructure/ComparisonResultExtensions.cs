namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System.Collections.Generic;
    using System.Text;
    using KellermanSoftware.CompareNetObjects;

    public static class ComparisonResultExtensions
    {
        public static string CreateDifferenceMessage(this ComparisonResult result, IEnumerable<object> actual, IEnumerable<object> expected)
        {
            var message = new StringBuilder();
            message
                .AppendTitleBlock("Expected", expected, Formatters.NamedJsonMessage)
                .AppendTitleBlock("But", actual, Formatters.NamedJsonMessage)
                .AppendTitleBlock("Difference", result.DifferencesString.Trim());

            return message.ToString();
        }

    }
}
