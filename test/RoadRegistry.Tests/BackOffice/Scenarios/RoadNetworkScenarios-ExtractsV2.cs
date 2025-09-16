namespace RoadRegistry.Tests.BackOffice.Scenarios;

using Autofac;
using AutoFixture;
using Framework.Testing;
using Moq;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using AcceptedChange = RoadRegistry.BackOffice.Messages.AcceptedChange;

public class RoadNetworkScenariosExtractsV2 : RoadNetworkTestBase
{
    private readonly Mock<IExtractRequests> _extractRequests = new();

    public RoadNetworkScenariosExtractsV2(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper, new UseExtractsV2FeatureToggle(true))
    {
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder.RegisterInstance(_extractRequests.Object);
    }

    [Fact]
    public async Task WhenChangesAccepted_ThenExtractV2IsClosed()
    {
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ObjectProvider.Create<ExtractRequestId>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var currentInstant = Clock.GetCurrentInstant();

        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                }
            )
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId),
                new RoadNetworkExtractGotRequestedV2
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalRequestId,
                    DownloadId = downloadId,
                    IsInformative = false,
                    Description = ObjectProvider.Create<ExtractDescription>(),
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                }
            )
            .When(new ChangeRoadNetworkBuilder(TestData)
                .WithDownloadId(downloadId)
                .WithExtractRequestId(extractRequestId)
                .WithAddRoadNode(TestData.AddStartNode1)
                .WithAddRoadNode(TestData.AddEndNode1)
                .WithAddRoadSegment(TestData.AddSegment1)
                .Build()
            )
            .Then([
                new RecordedEvent(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = TestData.RequestId,
                    Reason = TestData.ReasonForChange,
                    Operator = TestData.ChangedByOperator,
                    OrganizationId = TestData.ChangedByOrganization,
                    Organization = TestData.ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    DownloadId = downloadId,
                    Changes =
                    [
                        new AcceptedChange
                        {
                            RoadNodeAdded = TestData.StartNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = TestData.EndNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = TestData.Segment1Added,
                            Problems = []
                        }
                    ],
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                })
            ]));

        _extractRequests.Verify(x => x.UploadAcceptedAsync(downloadId, It.IsAny<CancellationToken>()));
    }
}
