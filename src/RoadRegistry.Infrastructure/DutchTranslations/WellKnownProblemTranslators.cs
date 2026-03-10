namespace RoadRegistry.Infrastructure.DutchTranslations;

public static class WellKnownProblemTranslators
{
    public static readonly IProblemTranslator Default = new DefaultProblemTranslator();
    public static readonly IProblemTranslator Extract = new ExtractProblemTranslator();
}
