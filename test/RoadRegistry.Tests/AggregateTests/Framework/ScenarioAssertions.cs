namespace RoadRegistry.Tests.AggregateTests.Framework;

using System.Text;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Exceptions;
using Xunit.Sdk;

public static class ScenarioAssertions
{
    public static async Task Assert(
        this IExpectEventsScenarioBuilder builder,
        ScenarioRunner runner,
        CancellationToken ct = default)
    {
        var scenario = builder.Build();
        var result = await runner.RunAsync(scenario);
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

    public static async Task Assert(
        this IExpectExceptionScenarioBuilder builder,
        ScenarioRunner runner)
    {
        var scenario = builder.Build();
        var result = await runner.RunAsync(scenario);
        switch (result)
        {
            case ScenarioExpectedExceptionButThrewNoException _:
                throw new XunitException("Expected exception but threw no exception");
            case ScenarioExpectedExceptionButThrewOtherException threw:
            {
                var messageBuilder = new StringBuilder();

                if (threw.Scenario.Throws is RoadRegistryProblemsException expected && threw.Actual is RoadRegistryProblemsException actual)
                {
                    messageBuilder.AppendLine("Expected exceptions to match but found differences:");
                    var comparison = runner.Compare(expected.Problems, actual.Problems);
                    foreach (var difference in comparison.Differences)
                    {
                        messageBuilder.AppendLine("\t" + difference);
                    }

                    messageBuilder.AppendLine("Expected:");
                    messageBuilder.AppendLine($"{expected.Problems}");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Actual:");
                    messageBuilder.AppendLine($"{actual.Problems}");
                }
                else
                {
                    messageBuilder.AppendLine("Expected exceptions to match but found differences.");
                    messageBuilder.AppendLine("Expected:");
                    messageBuilder.AppendLine($"{threw.Scenario.Throws}");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Actual:");
                    messageBuilder.AppendLine($"{threw.Actual}");
                }

                throw new XunitException(messageBuilder.ToString());
            }
            case ScenarioExpectedExceptionButRecordedEvents recorded:
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Expected exception but recorded these events:");
                foreach (var actual in recorded.Actual)
                {
                    messageBuilder.AppendLine($"Event={actual.GetType().Name}");
                    messageBuilder.AppendLine(JsonConvert.SerializeObject(actual, runner.SerializerSettings));
                }

                throw new XunitException(messageBuilder.ToString());
            }
        }
    }
}
