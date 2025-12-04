namespace RoadRegistry.BackOffice.Exceptions;

using CommandHandling;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using Core;
using Extensions;
using Problem = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem;

public class RoadRegistryValidationException : RoadRegistryException
{
    public string ErrorCode { get; }

    public RoadRegistryValidationException(ValueObjects.Problems.Problem problem)
        : this(problem.TranslateToDutch())
    {
    }
    public RoadRegistryValidationException(Problem problem)
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
