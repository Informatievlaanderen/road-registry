namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenListRoadSegments;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using Xunit;

// The filtering and paging come in the X-Filtering and X-Pagination headers the public API sets. The queries run on the
// in-memory document store here, through the Linq equivalent of the SQL; ReadModelQueriesTests in the integration tests
// covers the SQL itself.
public class ListRoadSegmentsTests : V2ReadEndpointTestBase
{
    private const int StreetNameId = 14291;
    private const int OtherStreetNameId = 14292;

    private readonly RoadSegmentsController _controller;

    public ListRoadSegmentsTests()
    {
        _controller = new RoadSegmentsController(CreateControllerContext(), null!);
        SetHttpContext(_controller);
    }

    [Fact]
    public async Task GivenNoFilter_ThenEveryRoadSegmentThatIsNotRemovedIsListedInTheOrderOfItsId()
    {
        Seed(BuildReadItem(3, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));
        Seed(BuildReadItem(1, RoadSegmentStatusV2.Gepland, StreetNameId));
        Seed(BuildReadItem(2, RoadSegmentStatusV2.Gehistoreerd, OtherStreetNameId));
        Seed(BuildReadItem(4, RoadSegmentStatusV2.Gerealiseerd, StreetNameId, isRemoved: true));

        var list = await List();

        list.Wegsegmenten.Select(x => (x.ObjectId, x.Wegsegmentstatus, x.Detail)).Should().Equal(
            ("1", "gepland", "https://test.basisregisters.vlaanderen/v3/wegsegmenten/1"),
            ("2", "gehistoreerd", "https://test.basisregisters.vlaanderen/v3/wegsegmenten/2"),
            ("3", "gerealiseerd", "https://test.basisregisters.vlaanderen/v3/wegsegmenten/3"));
        list.Volgende.Should().BeNull();
    }

    // A historized road segment keeps its street name, and its detail shows it, so it is listed under it.
    [Fact]
    public async Task GivenAStreetName_ThenTheRoadSegmentsLinkedToItAreListed()
    {
        Seed(BuildReadItem(1, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));
        Seed(BuildReadItem(2, RoadSegmentStatusV2.Gerealiseerd, OtherStreetNameId));
        Seed(BuildReadItem(3, RoadSegmentStatusV2.Gehistoreerd, StreetNameId));
        Seed(BuildReadItem(4, RoadSegmentStatusV2.Gerealiseerd, StreetNameId, isRemoved: true));

        var list = await List(filter: new { StreetNameId = StreetNameId.ToString() });

        list.Wegsegmenten.Select(x => x.ObjectId).Should().Equal("1", "3");
    }

    [Fact]
    public async Task GivenAStatus_ThenTheRoadSegmentsInThatStatusAreListed()
    {
        Seed(BuildReadItem(1, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));
        Seed(BuildReadItem(2, RoadSegmentStatusV2.BuitenGebruik, StreetNameId));
        Seed(BuildReadItem(3, RoadSegmentStatusV2.BuitenGebruik, OtherStreetNameId));

        var list = await List(filter: new { Status = "buiten gebruik" });

        list.Wegsegmenten.Select(x => (x.ObjectId, x.Wegsegmentstatus)).Should().Equal(
            ("2", "buiten gebruik"),
            ("3", "buiten gebruik"));
    }

    // A road segment that is not migrated yet carries its V1 status, and is listed under the V2 status its detail shows.
    [Fact]
    public async Task GivenAStatus_ThenAV1RoadSegmentIsListedUnderItsStatusInV2()
    {
        Seed(BuildV1ReadItem(1, RoadSegmentStatus.InUse, RoadSegmentGeometryDrawMethod.Outlined));
        Seed(BuildV1ReadItem(2, RoadSegmentStatus.OutOfUse, RoadSegmentGeometryDrawMethod.Measured));
        Seed(BuildV1ReadItem(3, RoadSegmentStatus.Unknown, RoadSegmentGeometryDrawMethod.Measured));
        Seed(BuildV1ReadItem(4, RoadSegmentStatus.Unknown, RoadSegmentGeometryDrawMethod.Outlined));

        var list = await List(filter: new { Status = "gerealiseerd" });

        list.Wegsegmenten.Select(x => (x.ObjectId, x.Wegsegmentstatus)).Should().Equal(
            ("1", "gerealiseerd"),
            ("3", "gerealiseerd"));
    }

    [Fact]
    public async Task GivenAStreetNameAndAStatus_ThenTheRoadSegmentsMatchingBothAreListed()
    {
        Seed(BuildReadItem(1, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));
        Seed(BuildReadItem(2, RoadSegmentStatusV2.Gepland, StreetNameId));
        Seed(BuildReadItem(3, RoadSegmentStatusV2.Gerealiseerd, OtherStreetNameId));

        var list = await List(filter: new { StreetNameId = StreetNameId.ToString(), Status = "gerealiseerd" });

        list.Wegsegmenten.Select(x => x.ObjectId).Should().Equal("1");
    }

    // The other registers' lists do the same: a filter value that cannot match anything matches nothing.
    [Theory]
    [InlineData("{\"StreetNameId\":\"abc\"}")]
    [InlineData("{\"StreetNameId\":\"0\"}")]
    [InlineData("{\"Status\":\"inGebruik\"}")]
    [InlineData("{\"Status\":\"vergunning aangevraagd\"}")]
    public async Task GivenAFilterValueThatMatchesNothing_ThenTheListIsEmpty(string filter)
    {
        Seed(BuildReadItem(1, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));

        var list = await List(rawFilter: filter);

        list.Wegsegmenten.Should().BeEmpty();
        list.Volgende.Should().BeNull();
    }

    // A full page links to the next one; the page after it is not full and does not.
    [Fact]
    public async Task GivenAPage_ThenTheNextPageIsLinkedWhileThePageIsFull()
    {
        foreach (var id in Enumerable.Range(1, 3))
        {
            Seed(BuildReadItem(id, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));
        }

        var firstPage = await List(pagination: "0,2");
        firstPage.Wegsegmenten.Select(x => x.ObjectId).Should().Equal("1", "2");
        firstPage.Volgende.Should().Be(new Uri("https://test.basisregisters.vlaanderen/v3/wegsegmenten?offset=2&limit=2"));

        var secondPage = await List(pagination: "2,2");
        secondPage.Wegsegmenten.Select(x => x.ObjectId).Should().Equal("3");
        secondPage.Volgende.Should().BeNull();
    }

    [Fact]
    public async Task GivenALimitAboveTheMaximum_ThenAtMost500AreListed()
    {
        foreach (var id in Enumerable.Range(1, 501))
        {
            Seed(BuildReadItem(id, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));
        }

        var list = await List(pagination: "0,1000");

        list.Wegsegmenten.Should().HaveCount(500);
        list.Volgende.Should().Be(new Uri("https://test.basisregisters.vlaanderen/v3/wegsegmenten?offset=500&limit=500"));
    }

    [Fact]
    public async Task GivenALimitOfZero_ThenNothingIsListed()
    {
        Seed(BuildReadItem(1, RoadSegmentStatusV2.Gerealiseerd, StreetNameId));

        var list = await List(pagination: "0,0");

        list.Wegsegmenten.Should().BeEmpty();
        list.Volgende.Should().BeNull();
    }

    private async Task<WegsegmentV2Lijst> List(object? filter = null, string? rawFilter = null, string? pagination = null)
    {
        var headers = _controller.ControllerContext.HttpContext.Request.Headers;
        if (filter is not null || rawFilter is not null)
        {
            headers["X-Filtering"] = rawFilter ?? JsonConvert.SerializeObject(filter);
        }

        if (pagination is not null)
        {
            headers["X-Pagination"] = pagination;
        }

        var result = await _controller.ListRoadSegmentsV2(ApiOptions, Store);

        return result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().BeOfType<WegsegmentV2Lijst>().Subject;
    }

    private RoadSegmentReadItem BuildReadItem(int roadSegmentId, RoadSegmentStatusV2 status, int streetNameId, bool isRemoved = false)
    {
        var added = Fixture.Create<RoadSegmentWasAdded>();
        var geometry = BuildGeometry();

        return new RoadSegmentReadItem
        {
            RoadSegmentId = new RoadSegmentId(roadSegmentId),
            Geometry = new RoadSegmentGeometryProjections
            {
                Lambert72 = geometry.EnsureLambert72(),
                Lambert08 = geometry.EnsureLambert08()
            },
            StartNodeId = null,
            EndNodeId = null,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten.ToString(),
            Status = status.ToString(),
            AccessRestriction = new ReadRoadSegmentDynamicAttribute<string>(),
            Category = new ReadRoadSegmentDynamicAttribute<string>(),
            Morphology = new ReadRoadSegmentDynamicAttribute<string>(),
            StreetNameId = new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>([
                (new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(10), RoadSegmentAttributeSide.Links, new RoadSegmentStreetNameAttributeValue
                {
                    StreetNameId = new StreetNameLocalId(streetNameId),
                    DutchName = null
                })
            ]),
            MaintenanceAuthorityId = new ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue>(),
            SurfaceType = new ReadRoadSegmentDynamicAttribute<string>(),
            CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
            BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
            PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = added.Provenance.ToEventTimestamp(),
            LastModified = added.Provenance.ToEventTimestamp(),
            IsV2 = true,
            IsRemoved = isRemoved
        };
    }

    private RoadSegmentReadItem BuildV1ReadItem(int roadSegmentId, RoadSegmentStatus status, RoadSegmentGeometryDrawMethod method)
    {
        var readItem = BuildReadItem(roadSegmentId, RoadSegmentStatusV2.Gepland, StreetNameId);
        readItem.IsV2 = false;
        readItem.Status = status.ToString();
        readItem.GeometryDrawMethod = method.ToString();
        return readItem;
    }

    private static RoadSegmentGeometry BuildGeometry()
    {
        return new MultiLineString([new LineString([
                new Coordinate(243234.8929999992, 160239.3830000013),
                new Coordinate(243279.0160000026, 160244.1570000015)
            ])])
            .WithSrid(WellknownSrids.Lambert72)
            .ToRoadSegmentGeometry();
    }
}
