namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Linq;
using Core;

public class RoadRegistryProblemsException : RoadRegistryException
{
    public Problems Problems { get; }

    public RoadRegistryProblemsException(Problems problems)
        : base($"{problems.Count} problems:{Environment.NewLine}{problems}")
    {
        ArgumentNullException.ThrowIfNull(problems);
        if (!problems.Any())
        {
            throw new ArgumentException("At least 1 problem is required", nameof(problems));
        }

        Problems = problems;
    }
}
