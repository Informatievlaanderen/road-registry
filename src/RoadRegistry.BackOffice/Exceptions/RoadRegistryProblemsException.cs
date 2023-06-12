namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Linq;
using System.Runtime.Serialization;
using Core;

[Serializable]
public class RoadRegistryProblemsException : RoadRegistryException
{
    public Problems Problems { get; }

    public RoadRegistryProblemsException(Problems problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        if (!problems.Any())
        {
            throw new ArgumentException("At least 1 problem is required", nameof(problems));
        }

        Problems = problems;
    }

    protected RoadRegistryProblemsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
