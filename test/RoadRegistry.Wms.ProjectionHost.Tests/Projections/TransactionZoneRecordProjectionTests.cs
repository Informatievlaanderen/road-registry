namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Wms.Projections;

public class TransactionZoneRecordProjectionTests
{
    private readonly Fixture _fixture;

    public TransactionZoneRecordProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeRoadNetworkExtractGotRequested();
        _fixture.CustomizeRoadNetworkExtractGotRequestedV2();
        _fixture.CustomizeRoadNetworkExtractClosed();
    }

    [Fact]
    public Task When_extract_got_requested()
    {
        var extract1 = _fixture.Create<RoadNetworkExtractGotRequested>();
        extract1.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
        };
        var extract2 = _fixture.Create<RoadNetworkExtractGotRequested>();
        extract2.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
        };
        var events = new[] { extract1, extract2 };

        var expected = new List<object>();
        expected.AddRange(events
            .Select(requested => new TransactionZoneRecord
            {
                DownloadId = requested.DownloadId,
                Description = requested.Description,
                Contour = (Geometry)GeometryTranslator.Translate(requested.Contour)
            }));

        var geometry = new WKTReader(WellKnownGeometryFactories.Default).Read("POLYGON((10 10, 10 5, 5 5, 5 10, 10 10))");
        geometry.SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();

        expected.Add(new OverlappingTransactionZonesRecord
        {
            DownloadId1 = extract1.DownloadId,
            DownloadId2 = extract2.DownloadId,
            Description1 = extract1.Description,
            Description2 = extract2.Description,
            Contour = geometry
        });

        return new TransactionZoneRecordProjection(new NullLoggerFactory())
            .Scenario()
            .Given(events.Cast<object>())
            .Expect(expected);
    }

    [Fact]
    public Task When_extract_got_requested_v2()
    {
        var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        extract1.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
        };
        var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        extract2.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
        };
        var events = new[] { extract1, extract2 };

        var expected = new List<object>();
        expected.AddRange(events
            .Select(requested => new TransactionZoneRecord
            {
                DownloadId = requested.DownloadId,
                Description = requested.Description,
                Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
            }));

        var geometry = new WKTReader(WellKnownGeometryFactories.Default).Read("POLYGON((10 10, 10 5, 5 5, 5 10, 10 10))");
        geometry.SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();

        expected.Add(new OverlappingTransactionZonesRecord
        {
            DownloadId1 = extract1.DownloadId,
            DownloadId2 = extract2.DownloadId,
            Description1 = extract1.Description,
            Description2 = extract2.Description,
            Contour = geometry
        });

        return new TransactionZoneRecordProjection(new NullLoggerFactory())
            .Scenario()
            .Given(events.Cast<object>())
            .Expect(expected);
    }

    [Fact]
    public Task When_extract_closed()
    {
        var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        extract1.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
        };
        var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        extract2.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
        };

        var extract1Closed = new RoadNetworkExtractClosed
        {
            RequestId = extract1.RequestId,
            ExternalRequestId = extract1.ExternalRequestId,
            DownloadIds = [extract1.DownloadId.ToString("N")],
            DateRequested = DateTime.UtcNow,
            Reason = _fixture.Create<RoadNetworkExtractCloseReason>(),
            When = extract1.When
        };

        var expectedContour = (Geometry)GeometryTranslator.Translate(extract2.Contour);
        var expected = new List<object>
        {
            new TransactionZoneRecord
            {
                DownloadId = extract2.DownloadId,
                Description = extract2.Description,
                Contour = expectedContour
            }
        };

        return new TransactionZoneRecordProjection(new NullLoggerFactory())
            .Scenario()
            .Given(new[] { extract1, extract2 }.Cast<object>())
            .Given(extract1Closed)
            .Expect(expected);
    }

    [Fact]
    public Task When_extract_upload_changes_got_accepted()
    {
        var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        extract1.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
        };
        var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
        extract2.Contour = new RoadNetworkExtractGeometry
        {
            WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
        };

        var extract1Accepted = new RoadNetworkChangesAccepted
        {
            RequestId = extract1.RequestId,
            DownloadId = extract1.DownloadId,
            When = extract1.When
        };

        var expectedContour = (Geometry)GeometryTranslator.Translate(extract2.Contour);
        var expected = new List<object>
        {
            new TransactionZoneRecord
            {
                DownloadId = extract2.DownloadId,
                Description = extract2.Description,
                Contour = expectedContour
            }
        };

        return new TransactionZoneRecordProjection(new NullLoggerFactory())
            .Scenario()
            .Given(new[] { extract1, extract2 }.Cast<object>())
            .Given(extract1Accepted)
            .Expect(expected);
    }
}
