namespace RoadRegistry.BackOffice.Handlers.Sqs;

using System.Reflection;
using Abstractions;

public static class SqsJsonMessageAssemblies
{
    public static readonly IReadOnlyCollection<Assembly> Assemblies =
    [
        typeof(BackOfficeAbstractionsAssemblyMarker).Assembly,
        typeof(BackOfficeHandlersSqsAssemblyMarker).Assembly
    ];
}
