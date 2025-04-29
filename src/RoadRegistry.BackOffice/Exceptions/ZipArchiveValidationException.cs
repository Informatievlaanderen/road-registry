namespace RoadRegistry.BackOffice.Exceptions;

using System.Linq;
using Uploads;

public class ZipArchiveValidationException : RoadRegistryException
{
    public ZipArchiveProblems Problems { get; }

    public ZipArchiveValidationException(ZipArchiveProblems problems)
        : base("Zip archive is invalid.")
    {
        Problems = problems;
    }

    public override string ToString()
    {
        return $"{base.ToString()}\nProblems:\n{string.Join("\n", Problems.Select(x => x.Describe()))}";
    }
}
