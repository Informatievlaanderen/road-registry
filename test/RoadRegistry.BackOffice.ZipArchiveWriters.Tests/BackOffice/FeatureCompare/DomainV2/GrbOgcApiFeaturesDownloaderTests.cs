namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using FluentAssertions;
using NetTopologySuite.IO;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Infrastructure;

public class GrbOgcApiFeaturesDownloaderTests
{
    [Fact]
    public async Task DownloadFeaturesSucceeds()
    {
        var downloader = new GrbOgcApiFeaturesDownloader(new HttpClient(), "https://geo.api.vlaanderen.be/GRB/ogc/features/v1");

        var bbox = new WKTReader().Read("POLYGON ((139400 203697, 141400 203697, 141400 205697, 139400 205697, 139400 203697))").TransformFromLambert72To08().EnvelopeInternal;
        var features = await downloader.DownloadFeaturesAsync(
            ["WBN", "KNW"],
            bbox,
            WellknownSrids.Lambert08);

        features.Should().NotBeEmpty();
        features.Should().Contain(x => x.CollectionId == "WBN");
        features.Should().Contain(x => x.CollectionId == "KNW");
    }
}
