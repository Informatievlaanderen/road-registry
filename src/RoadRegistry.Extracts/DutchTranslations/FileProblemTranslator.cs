namespace RoadRegistry.Extracts.DutchTranslations;

using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.Messages;
using FileProblem = RoadRegistry.Extracts.Messages.FileProblem;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public abstract class FileProblemTranslator
{
    public static readonly FileProblemTranslator DomainV1 = new DefaultFileProblemTranslator();
    public static readonly FileProblemTranslator DomainV2 = new DomainV2FileProblemTranslator();

    private readonly IProblemTranslator _problemTranslator;

    protected FileProblemTranslator(IProblemTranslator problemTranslator)
    {
        _problemTranslator = problemTranslator;
    }

    public ProblemTranslation Translate(FileProblem fileProblem)
    {
        return InnerTranslate(fileProblem)
               ?? _problemTranslator.Translate(new Problem
               {
                   Reason = fileProblem.Reason,
                   Parameters = fileProblem.Parameters,
                   Severity = fileProblem.Severity
               });
    }

    protected abstract ProblemTranslation? InnerTranslate(FileProblem problem);
}
