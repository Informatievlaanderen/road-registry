namespace RoadRegistry.BackOffice.Exceptions;

using Core;
using Extensions;

public class RoadRegistryValidationException : RoadRegistryException
{
    public string ErrorCode { get; }

    public RoadRegistryValidationException(Core.Problem problem)
        : this(problem.TranslateToDutch())
    {
    }
    public RoadRegistryValidationException(Messages.Problem problem)
        : this(problem.TranslateToDutch())
    {
    }

    public RoadRegistryValidationException(ProblemTranslation problemTranslation)
        : base(problemTranslation.Message)
    {
        ErrorCode = problemTranslation.Code;
    }

    public RoadRegistryValidationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
