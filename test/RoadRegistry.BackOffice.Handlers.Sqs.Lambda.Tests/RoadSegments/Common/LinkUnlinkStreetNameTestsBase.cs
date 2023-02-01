namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.Common;

using Abstractions;
using Autofac;
using BackOffice.Framework;
using Core;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using StreetNameConsumer.Schema;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public abstract class LinkUnlinkStreetNameTestsBase : BackOfficeLambdaTest
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
                    new FakeRoadNetworkSnapshotWriter(),
                    Clock,
                    LoggerFactory.CreateLogger<RoadNetworkCommandModule>()
                )
            }), ApplicationMetadata));
    }

    protected async Task GivenSegment1Added()
    {
        await Given(Organizations.ToStreamName(ChangedByOrganization), new ImportedOrganization
        {
            Code = ChangedByOrganization,
            Name = ChangedByOrganizationName,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = RequestId,
            Reason = ReasonForChange,
            Operator = ChangedByOperator,
            OrganizationId = ChangedByOrganization,
            Organization = ChangedByOrganizationName,
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = StartNode1Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = EndNode1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = Segment1Added
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
