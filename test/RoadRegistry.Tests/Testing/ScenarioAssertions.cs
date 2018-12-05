namespace RoadRegistry.Testing
{
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using KellermanSoftware.CompareNetObjects;
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
                                recorded.Scenario.Thens.Select(given => $"Stream={given.Stream}-Event={given.Event.GetType().Name}")),
                            recorded.Actual.Length,
                            string.Join(",",
                                recorded.Actual.Select(actual => $"Stream={actual.Stream}-Event={actual.Event.GetType().Name}")));
                    }
                    else
                    {
                        messageBuilder.AppendLine("Expected events to match but found differences:");
                        var config = new ComparisonConfig
                        {
                            MaxDifferences = int.MaxValue,
                            MaxStructDepth = 5
                        };
                        var comparer = new CompareLogic(config);
                        var comparison = comparer.Compare(recorded.Scenario.Thens, recorded.Actual);
                        foreach (var difference in comparison.Differences)
                        {
                            messageBuilder.AppendLine("\t" + difference);
                        }
                        messageBuilder.AppendLine("Expected:");
                        foreach (var then in recorded.Scenario.Thens)
                        {
                            messageBuilder.AppendLine($"Stream={then.Stream}-Event={then.Event.GetType().Name}");
                            messageBuilder.AppendLine(JsonConvert.SerializeObject(then.Event, Formatting.Indented));

                        }
                        messageBuilder.AppendLine("Actual:");
                        foreach (var actual in recorded.Actual)
                        {
                            messageBuilder.AppendLine($"Stream={actual.Stream}-Event={actual.Event.GetType().Name}");
                            messageBuilder.AppendLine(JsonConvert.SerializeObject(actual.Event, Formatting.Indented));
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
                        messageBuilder.AppendLine($"\t{actual.Stream} - {actual.Event.GetType().Name} {JsonConvert.SerializeObject(actual.Event, Formatting.Indented)}");
                    }
                    throw new XunitException(messageBuilder.ToString());
            }
        }
    }
}
