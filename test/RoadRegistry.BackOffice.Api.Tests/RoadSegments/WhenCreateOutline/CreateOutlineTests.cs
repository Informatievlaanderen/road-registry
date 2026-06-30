namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.Tests.BackOffice;

public class CreateOutlineTests : CreateOutlineTestBase
{
    [Fact]
    public async Task ValidRequest_AcceptedResult()
    {
        var result = await GetResultAsync(CreateValidRequest());
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task InvalidWegbeheerder_WegbeheerderNietCorrect()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with { Wegbeheerder = "" },
            "WegbeheerderNietCorrect",
            "Wegbeheerder is foutief."
        );
    }

    [Fact]
    public async Task UnknownWegbeheerder_WegbeheerderNietGekend()
    {
        var orgCache = new FakeOrganizationCache().Seed(new OrganizationId("ABC"), null);

        await ItShouldHaveExpectedError(
            CreateValidRequest() with { Wegbeheerder = "ABC" },
            "WegbeheerderNietGekend",
            "De opgegeven wegbeheerdercode",
            orgCache
        );
    }

    [Fact]
    public async Task InvalidStatus_WegsegmentStatusNietCorrect()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with { Wegsegmentstatus = "" },
            "WegsegmentStatusNietCorrect",
            "Wegsegment status is foutief"
        );
    }

    [Fact]
    public async Task InvalidMorfologischeWegklasse_MorfologischeWegklasseNietCorrect()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with { MorfologischeWegklasse = "" },
            "MorfologischeWegklasseNietCorrect",
            "Morfologische wegklasse is foutief"
        );
    }

    [Fact]
    public async Task InvalidToegangsbeperking_ToegangsbeperkingNietCorrect()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with { Toegangsbeperking = "" },
            "ToegangsbeperkingNietCorrect",
            "Toegangsbeperking is foutief"
        );
    }

    [Fact]
    public async Task InvalidWegverharding_WegverhardingNietCorrect()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with { Wegverharding = "" },
            "WegverhardingNietCorrect",
            "Wegverharding is foutief"
        );
    }

    [Fact]
    public async Task InvalidWegcategorie_WegcategorieNietCorrect()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with { Wegcategorie = RoadSegmentCategory.MainRoad.ToDutchString() },
            "WegcategorieNietCorrect",
            "Wegcategorie is foutief."
        );
    }

    [Fact]
    public async Task InvalidAantalRijstroken_AantalRijstrokenNietCorrect()
    {
        var request = CreateValidRequest();
        request.AantalRijstroken.Aantal = "0";

        await ItShouldHaveExpectedError(
            request,
            "AantalRijstrokenNietCorrect",
            "Aantal rijstroken is foutief. '0' is geen geldige waarde."
        );
    }

    [Fact]
    public async Task InvalidAantalRijstrokenRichting_AantalRijstrokenRichtingNietCorrect()
    {
        var request = CreateValidRequest();
        request.AantalRijstroken.Richting = "";

        await ItShouldHaveExpectedError(
            request,
            "AantalRijstrokenRichtingNietCorrect",
            "Aantal rijstroken richting is foutief."
        );
    }

    [Fact]
    public async Task TooLongGeometry_MiddellijnGeometrieTeLang()
    {
        await ItShouldHaveExpectedError(
            CreateValidRequest() with
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
            CreateValidRequest() with
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
