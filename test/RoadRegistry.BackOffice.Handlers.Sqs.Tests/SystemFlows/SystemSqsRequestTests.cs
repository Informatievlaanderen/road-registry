namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.SystemFlows;

using RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;
using Xunit;

public class SystemSqsRequestTests
{
    [Fact]
    public void SystemSqsRequestsMustNotHaveAMediatorSqsHandler()
    {
        var assembly = typeof(ISystemSqsRequest).Assembly;

        var offenders = assembly.GetTypes()
            .Where(type => !type.IsAbstract)
            .Select(handler => new { Handler = handler, RequestType = FindSqsHandlerRequestType(handler) })
            .Where(x => x.RequestType is not null && typeof(ISystemSqsRequest).IsAssignableFrom(x.RequestType))
            .Select(x => $"{x.Handler.Name} -> {x.RequestType!.Name}")
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "System SqsRequests run without ticketing and must not be reachable through a mediator/SqsHandler entry point, " +
            $"but found: {string.Join(", ", offenders)}");
    }

    private static System.Type? FindSqsHandlerRequestType(System.Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition().Name == "SqsHandler`1")
            {
                return current.GetGenericArguments()[0];
            }
        }

        return null;
    }
}
