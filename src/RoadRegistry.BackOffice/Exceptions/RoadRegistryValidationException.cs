namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Runtime.Serialization;
using Core;
using Extensions;

[Serializable]
public class RoadRegistryValidationException : RoadRegistryException
{
    public string ErrorCode { get; }

    public RoadRegistryValidationException(Problem problem)
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

    protected RoadRegistryValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
