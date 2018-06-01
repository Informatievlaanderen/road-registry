namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using KellermanSoftware.CompareNetObjects;

    public static class ComparisonResultExtensions
    {
        public static string CreateDifferenceMessage(this ComparisonResult result)
        {
            return result.DifferencesString.Trim();
        }
    }
}