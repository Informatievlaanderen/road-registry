namespace RoadRegistry.Infrastructure.DutchTranslations;

using Messages;

public interface IProblemTranslator
{
    ProblemTranslation Translate(Problem problem);
}
