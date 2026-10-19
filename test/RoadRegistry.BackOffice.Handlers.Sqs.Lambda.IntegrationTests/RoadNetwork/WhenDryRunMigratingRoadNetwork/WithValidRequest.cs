namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenDryRunMigratingRoadNetwork;

using Actions.MigrateDryRunRoadNetwork;
using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.BackOffice;
using TicketingService.Abstractions;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class WithValidRequest : RoadNetworkIntegrationTest
{
    private readonly Mock<IExtractRequests> _extractRequestsMock = new();

    public WithValidRequest(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task GivenEmptyRoadNetwork_ThenSucceeded()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var ticketId = TestData.Fixture.Create<TicketId>();
        var command = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = TestData.Fixture.Create<UploadId>(),
                DownloadId = TestData.Fixture.Create<DownloadId>(),
                Changes =
                [
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1StartNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1EndNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadSegment = TestData.AddSegment1
                    }
                ],
                TicketId = ticketId,
                ProvenanceData = provenanceData
            },
            TicketId = ticketId,
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        var ticketResult = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().BeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().BeEmpty();

        var roadSegments = await session.LoadManyAsync([TestData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().BeEmpty();

        var inwinningsZone = extractsDbContext.Inwinningszones.Single(x => x.DownloadId == command.MigrateRoadNetworkSqsRequest.DownloadId.ToGuid());
        inwinningsZone.Completed.Should().BeFalse();

        MediatorMock.Verify(x => x.Send(
            It.Is<DataValidationSqsRequest>(r => r.MigrateRoadNetworkSqsRequest == command.MigrateRoadNetworkSqsRequest && r.TicketId == command.TicketId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenOnlyDryRunAndValidRequest_ThenError()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var ticketId = TestData.Fixture.Create<TicketId>();
        var command = new MigrateDryRunRoadNetworkSqsRequest
        {
            OnlyDryRun = true,
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = TestData.Fixture.Create<UploadId>(),
                DownloadId = TestData.Fixture.Create<DownloadId>(),
                Changes =
                [
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1StartNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1EndNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadSegment = TestData.AddSegment1
                    }
                ],
                TicketId = ticketId,
                ProvenanceData = provenanceData
            },
            TicketId = ticketId,
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = command.MigrateRoadNetworkSqsRequest.UploadId,
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            Status = ExtractUploadStatus.Processing,
            TicketId = command.TicketId,
            UploadedOn = DateTimeOffset.UtcNow
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        TicketingMock.VerifyThatTicketHasError(code: "UploadDryRunSuccessful");

        MediatorMock.Verify(x => x.Send(
            It.Is<DataValidationSqsRequest>(r => r.MigrateRoadNetworkSqsRequest == command.MigrateRoadNetworkSqsRequest && r.TicketId == command.TicketId),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GivenInwinningRoadSegments_WhenNotAllRoadSegmentIdsAreUploaded_ThenException()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var ticketId = TestData.Fixture.Create<TicketId>();
        var command = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = TestData.Fixture.Create<UploadId>(),
                DownloadId = TestData.Fixture.Create<DownloadId>(),
                Changes =
                [
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1StartNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1EndNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadSegment = TestData.AddSegment1
                    }
                ],
                TicketId = ticketId,
                ProvenanceData = provenanceData
            },
            TicketId = ticketId,
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            NisCode = nisCode,
            RoadSegmentId = 999,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
        var act = () => handler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // A road segment the inwinning still lists that is already ingewonnen has nothing left for an upload to carry:
    // it is not missing from it, and completing the inwinning is what takes it from locked to compleet.
    [Fact]
    public async Task GivenInwinningRoadSegments_WhenAMissingRoadSegmentIsAlreadyIngewonnen_ThenNoException()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var ticketId = TestData.Fixture.Create<TicketId>();
        var command = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = TestData.Fixture.Create<UploadId>(),
                DownloadId = TestData.Fixture.Create<DownloadId>(),
                Changes =
                [
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1StartNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1EndNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadSegment = TestData.AddSegment1
                    }
                ],
                TicketId = ticketId,
                ProvenanceData = provenanceData
            },
            TicketId = ticketId,
            ProvenanceData = provenanceData
        };

        // A V2 road segment, so its inwinning is done whatever the inwinning this upload belongs to still says. It
        // sits far away from what the upload changes, so it is the inwinning that brings it into the picture and
        // not the scope of the migration.
        var alreadyIngewonnenRoadSegmentId = new RoadSegmentId(999);
        var store = sp.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession())
        {
            var roadSegmentWasAdded = TestData.Fixture.Create<RoadSegmentWasAdded>() with
            {
                RoadSegmentId = alreadyIngewonnenRoadSegmentId,
                Geometry = BuildRoadSegmentGeometry(10000, 10000, 10010, 10000)
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), alreadyIngewonnenRoadSegmentId), roadSegmentWasAdded);
            await session.SaveChangesAsync();
        }

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            NisCode = nisCode,
            RoadSegmentId = alreadyIngewonnenRoadSegmentId,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
        var act = () => handler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        MediatorMock.Verify(x => x.Send(
            It.Is<DataValidationSqsRequest>(r => r.MigrateRoadNetworkSqsRequest == command.MigrateRoadNetworkSqsRequest && r.TicketId == command.TicketId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // A road segment the inwinning still lists that has since been removed cannot be in any upload. Like one that is
    // already ingewonnen, it is not missing: completing the inwinning is what takes it from locked to compleet.
    [Fact]
    public async Task GivenInwinningRoadSegments_WhenAMissingRoadSegmentWasRemoved_ThenNoException()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var ticketId = TestData.Fixture.Create<TicketId>();
        var command = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = TestData.Fixture.Create<UploadId>(),
                DownloadId = TestData.Fixture.Create<DownloadId>(),
                Changes =
                [
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1StartNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadNode = TestData.AddSegment1EndNode
                    },
                    new ChangeRoadNetworkItem
                    {
                        AddRoadSegment = TestData.AddSegment1
                    }
                ],
                TicketId = ticketId,
                ProvenanceData = provenanceData
            },
            TicketId = ticketId,
            ProvenanceData = provenanceData
        };

        // A V1 road segment - the aggregate carries no attributes, so it is not ingewonnen - that an earlier upload
        // for this zone removed. It is the removal and not the inwinning that settles it, which is what sets this
        // apart from the test above. It sits far away from what the upload changes, so it is the inwinning that
        // brings it into the picture and not the scope of the migration.
        var removedRoadSegmentId = new RoadSegmentId(999);
        var removedRoadSegmentGeometry = BuildRoadSegmentGeometry(10000, 10000, 10010, 10000);
        var store = sp.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession())
        {
            var importedRoadSegment = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment>();
            importedRoadSegment.RoadSegmentId = removedRoadSegmentId;
            importedRoadSegment.Geometry = removedRoadSegmentGeometry;

            // What the marten migration does for a V1 road segment: the legacy event on the stream, and the
            // aggregate stored alongside it.
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), removedRoadSegmentId), importedRoadSegment);
            session.Store(RoadSegment.CreateForMigration(removedRoadSegmentId, removedRoadSegmentGeometry, RoadSegmentStatusV2.Gerealiseerd, null, null));

            await session.SaveChangesAsync();
        }

        // In a transaction of its own: the topology projection only lets a removal through when it is later than
        // what it has on record for the segment.
        await using (var session = store.LightweightSession())
        {
            session.Events.Append(StreamKeyFactory.Create(typeof(RoadSegment), removedRoadSegmentId), new RoadSegmentWasRemovedBecauseOfMigration
            {
                RoadSegmentId = removedRoadSegmentId,
                Geometry = removedRoadSegmentGeometry,
                StartNodeId = null,
                EndNodeId = null,
                Provenance = provenanceData
            });

            await session.SaveChangesAsync();
        }

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            NisCode = nisCode,
            RoadSegmentId = removedRoadSegmentId,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
        var act = () => handler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        MediatorMock.Verify(x => x.Send(
            It.Is<DataValidationSqsRequest>(r => r.MigrateRoadNetworkSqsRequest == command.MigrateRoadNetworkSqsRequest && r.TicketId == command.TicketId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(_extractRequestsMock.Object)
            .AddSingleton(new FakeExtractsDbContextFactory().CreateDbContext())
            .AddScoped<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
    }
}
