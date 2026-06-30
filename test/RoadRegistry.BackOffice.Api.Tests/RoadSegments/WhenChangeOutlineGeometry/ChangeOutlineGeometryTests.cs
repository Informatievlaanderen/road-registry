namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.Tests.BackOffice;

public class ChangeOutlineGeometryTests : ChangeOutlineGeometryTestBase
{
    private static PostChangeOutlineGeometryParameters ValidRequest => new()
    {
        MiddellijnGeometrie = GeometryTranslatorTestCases.ValidGmlLineString
    };

    [Fact]
    public async Task ValidRequest_AcceptedResult()
    {
        var result = await GetResultAsync(ValidRequest);
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task InvalidGeometry_MiddellijnGeometrieNietCorrect()
    {
        await ItShouldHaveExpectedError(
            ValidRequest with { MiddellijnGeometrie = "abc" },
            "MiddellijnGeometrieNietCorrect",
            "De opgegeven geometrie is geen geldige LineString in gml 3.2."
        );
    }

    [Fact]
    public async Task InvalidGeometryDrawMethod_GeometriemethodeNietIngeschetst()
    {
        var measuredRepo = new Mock<IRoadSegmentRepository>();
        measuredRepo
            .Setup(x => x.FindAsync(It.IsAny<RoadSegmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoadSegmentRecord(new RoadSegmentId(TestData.Segment1Added.Id), RoadSegmentGeometryDrawMethod.Measured, "hash"));

        await ItShouldHaveExpectedError(
            ValidRequest,
            "GeometriemethodeNietIngeschetst",
            "De geometriemethode van dit wegsegment komt niet overeen met 'ingeschetst'.",
            measuredRepo.Object
        );
    }

    [Fact]
    public async Task InvalidGeometrySrid_MiddellijnGeometrieCRSNietCorrect()
    {
        await ItShouldHaveExpectedError(
            ValidRequest with
            {
                MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/10000"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:LineString>"
            },
            "MiddellijnGeometrieCRSNietCorrect",
            "De opgegeven geometrie heeft niet het coördinatenstelsel Lambert 72."
        );
    }

    [Fact]
    public async Task TooLongGeometry_MiddellijnGeometrieTeLang()
    {
        await ItShouldHaveExpectedError(
            ValidRequest with
            {
                MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368 181577 217368 281577</gml:posList>
</gml:LineString>"
            },
            "MiddellijnGeometrieTeLang",
            "De opgegeven geometrie zijn lengte is groter of gelijk dan 100000 meter."
        );
    }

    [Fact]
    public async Task TooShortGeometry_MiddellijnGeometrieKorterDanMinimum()
    {
        await ItShouldHaveExpectedError(
            ValidRequest with
            {
                MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368 181577 217368 181578</gml:posList>
</gml:LineString>"
            },
            "MiddellijnGeometrieKorterDanMinimum",
            "De opgegeven geometrie heeft niet de minimale lengte van 2 meter."
        );
    }
}
