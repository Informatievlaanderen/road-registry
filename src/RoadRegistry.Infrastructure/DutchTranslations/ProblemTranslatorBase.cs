namespace RoadRegistry.Infrastructure.DutchTranslations;

using RoadRegistry.Infrastructure.Messages;
using RoadRegistry.ValueObjects.ProblemCodes;

public abstract class ProblemTranslatorBase : IProblemTranslator
{
    private readonly Lazy<Dictionary<ProblemCode, Func<Problem, ProblemTranslation>>> _translators;

    protected ProblemTranslatorBase()
    {
        _translators = new Lazy<Dictionary<ProblemCode, Func<Problem, ProblemTranslation>>>(CreateTranslators);
    }

    protected abstract Dictionary<ProblemCode, Func<Problem, ProblemTranslation>> CreateTranslators();

    public ProblemTranslation Translate(Problem problem)
    {
        var problemCode = ProblemCode.FromReason(problem.Reason);

        if (problemCode is not null && _translators.Value.TryGetValue(problemCode, out Func<Problem, ProblemTranslation> translator))
        {
            return translator(problem);
        }

        return CreateMissingTranslation(problem);
    }

    public ProblemTranslation CreateMissingTranslation(Problem problem)
    {
        return new ProblemTranslation(problem.Severity, problem.Reason, $"'{problem.Reason}' has no translation. Please fix it.");
    }
}
