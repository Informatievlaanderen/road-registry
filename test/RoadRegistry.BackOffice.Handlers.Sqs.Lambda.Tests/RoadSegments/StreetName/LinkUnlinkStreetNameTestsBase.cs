namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.StreetName;

using Abstractions;
using Autofac;
using BackOffice.Framework;
using Core;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.Tests.Framework;
using StreetNameConsumer.Schema;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public abstract class LinkUnlinkStreetNameTestsBase : SqsLambdaTestsBase
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected LinkUnlinkStreetNameTestsBase(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
        StreetNameCache = new FakeStreetNameCache()
            .AddStreetName(WellKnownStreetNameIds.Proposed, "Proposed street", nameof(StreetNameStatus.Proposed))
            .AddStreetName(WellKnownStreetNameIds.Current, "Current street", nameof(StreetNameStatus.Current))
            .AddStreetName(WellKnownStreetNameIds.Retired, "Retired street", nameof(StreetNameStatus.Retired));
    }

    protected IStreetNameCache StreetNameCache { get; }

    protected override void ConfigureCommandHandling(ContainerBuilder builder)
    {
        base.ConfigureCommandHandling(builder);

        builder.RegisterInstance(Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(
                    Store,
                    EntityMapFactory,
                    new FakeRoadNetworkSnapshotReader(),
                    Clock,
                    LoggerFactory
                )
            }), ApplicationMetadata));
    }

    protected async Task GivenSegment1Added()
    {
        await Given(Organizations.ToStreamName(TestData.ChangedByOrganization), new ImportedOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = TestData.ChangedByOrganizationName,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = TestData.StartNode1Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = TestData.EndNode1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = TestData.Segment1Added
                }
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
    }

    protected string StreetNamePuri(int identifier)
    {
        return $"https://data.vlaanderen.be/id/straatnaam/{identifier}";
    }

    protected static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
    }
}
