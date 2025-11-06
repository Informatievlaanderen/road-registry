namespace RoadRegistry.Tests.AggregateTests.Framework;

using System.Text;
using Newtonsoft.Json;
using Xunit.Sdk;

public static class ScenarioAssertions
{
    public static async Task AssertAsync(
        this IExpectEventsScenarioBuilder builder,
        ScenarioRunner runner,
        CancellationToken ct = default)
    {
        var scenario = builder.Build();
        var result = await runner.RunAsync(scenario, ct);
        switch (result)
        {
            case ScenarioExpectedEventsButThrewException threw:
                throw new XunitException($"Expected events but threw {threw.Actual}");
            case ScenarioExpectedEventsButRecordedOtherEvents recorded:
                var messageBuilder = new StringBuilder();
                if (recorded.Scenario.Thens.Length != recorded.Actual.Length)
                {
                    messageBuilder.AppendFormat("Expected {0} events ({1}) but recorded {2} events ({3}).",
                        recorded.Scenario.Thens.Length,
                        string.Join(",",
                            recorded.Scenario.Thens.Select(given => $"Event={given.GetType().Name}")),
                        recorded.Actual.Length,
                        string.Join(",",
                            recorded.Actual.Select(actual => $"Event={actual.GetType().Name}")));
                }
                else
                {
                    messageBuilder.AppendLine("Expected events to match but found differences:");
                    var comparison = runner.Compare(recorded.Scenario.Thens, recorded.Actual);
                    foreach (var difference in comparison.Differences)
                    {
                        messageBuilder.AppendLine("\t" + difference);
                    }

                    messageBuilder.AppendLine("Expected:");
                    foreach (var then in recorded.Scenario.Thens)
                    {
                        messageBuilder.AppendLine($"Event={then.GetType().Name}");
                        messageBuilder.AppendLine(JsonConvert.SerializeObject(then, runner.SerializerSettings));
                    }

                    messageBuilder.AppendLine("Actual:");
                    foreach (var actual in recorded.Actual)
                    {
                        messageBuilder.AppendLine($"Event={actual.GetType().Name}");
                        messageBuilder.AppendLine(JsonConvert.SerializeObject(actual, runner.SerializerSettings));
                    }
                }

                throw new XunitException(messageBuilder.ToString());
        }
    }

    public static async Task AssertAsync(
        this IExpectExceptionScenarioBuilder builder,
        ScenarioRunner runner,
        CancellationToken ct = default)
    {
        var scenario = builder.Build();
        var result = await runner.RunAsync(scenario, ct);
        switch (result)
        {
            case ScenarioExpectedExceptionButThrewNoException _:
                throw new XunitException("Expected exception but threw no exception");
            case ScenarioExpectedExceptionButThrewOtherException threw:
                throw new XunitException($"Expected exception but threw {threw.Actual}");
            case ScenarioExpectedExceptionButRecordedEvents recorded:
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Expected exception but recorded these events:");
                foreach (var actual in recorded.Actual)
                {
                    messageBuilder.AppendLine($"\t{actual.Stream} - {actual.Event.GetType().Name} {JsonConvert.SerializeObject(actual.Event, runner.SerializerSettings)}");
                }

                throw new XunitException(messageBuilder.ToString());
        }
    }
}
