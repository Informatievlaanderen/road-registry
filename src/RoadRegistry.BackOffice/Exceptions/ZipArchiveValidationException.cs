namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Linq;
using System.Runtime.Serialization;
using Uploads;

[Serializable]
public class ZipArchiveValidationException : RoadRegistryException
{
    public ZipArchiveProblems Problems { get; }

    public ZipArchiveValidationException(ZipArchiveProblems problems)
        : base("Zip archive is invalid.")
    {
        Problems = problems;
    }

    protected ZipArchiveValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public override string ToString()
    {
        return $"{base.ToString()}\nProblems:\n{string.Join("\n", Problems.Select(x => x.Describe()))}";
    }
}
