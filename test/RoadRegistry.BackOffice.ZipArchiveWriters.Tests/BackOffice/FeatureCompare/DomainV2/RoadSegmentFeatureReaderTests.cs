namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using FluentAssertions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;

// The bijhouding exchange format differs from the one an inwinning delivers in two ways that matter here: it carries
// a METHODE of its own instead of having one worked out from the GRB features, and it may state -8 for a traffic
// direction it does not know.
public class RoadSegmentFeatureReaderTests
{
    private const int NietGekend = -8;

    [Theory]
    [InlineData(1, nameof(RoadSegmentGeometryDrawMethodV2.Ingeschetst))]
    [InlineData(2, nameof(RoadSegmentGeometryDrawMethodV2.Ingemeten))]
    public void MethodeIsTakenFromTheDelivery(int methode, string expectedMethod)
    {
        var (segment, problems) = Read(record => record.METHODE.Value = methode);

        problems.Should().BeEmpty();
        segment.Method!.ToString().Should().Be(expectedMethod);
    }

    [Fact]
    public void MethodeOutsideTheKnownValuesIsReported()
    {
        var (_, problems) = Read(record => record.METHODE.Value = 3);

        problems.Should().Contain(problem => problem.Reason == "RoadSegmentGeometryDrawMethodV2Mismatch");
    }

    [Fact]
    public void MethodeIsRequired()
    {
        var (_, problems) = Read(record => record.METHODE.Value = null);

        problems.Should().Contain(problem => problem.Reason == "RequiredFieldIsNull");
    }

    [Fact]
    public void AnUnknownCarTrafficDirectionIsReadAsUnknown()
    {
        var (segment, problems) = Read(record =>
        {
            record.AUTOHEEN.Value = NietGekend;
            record.AUTOTERUG.Value = NietGekend;
        });

        problems.Should().BeEmpty();
        segment.CarAccessForward.Should().BeNull();
        segment.CarAccessBackward.Should().BeNull();
    }

    [Fact]
    public void AnUnknownBikeTrafficDirectionIsReadAsUnknown()
    {
        var (segment, problems) = Read(record =>
        {
            record.FIETSHEEN.Value = NietGekend;
            record.FIETSTERUG.Value = NietGekend;
        });

        problems.Should().BeEmpty();
        segment.BikeAccessForward.Should().BeNull();
        segment.BikeAccessBackward.Should().BeNull();
    }

    [Fact]
    public void AnUnknownPedestrianTrafficDirectionIsReadAsUnknown()
    {
        var (segment, problems) = Read(record => record.VOETGANGER.Value = NietGekend);

        problems.Should().BeEmpty();
        segment.PedestrianAccess.Should().BeNull();
    }

    // Unknown is -8 and nothing else; a value that is neither that nor a boolean is still a mismatch.
    [Fact]
    public void AValueThatIsNeitherUnknownNorABooleanIsReported()
    {
        var (_, problems) = Read(record => record.AUTOHEEN.Value = 7);

        problems.Should().Contain(problem => problem.Reason == "RoadSegmentAutoHeenMismatch");
    }

    // The archive holds more than one road segment; only the one the test configured is of interest, so it is picked
    // out again by the temp id it was written with.
    private static (RoadSegmentFeatureCompareWithFlatAttributes, ZipArchiveProblems) Read(
        Action<RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments.RoadSegmentDbaseRecord> configure)
    {
        var tempId = 0;
        using var archive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                configure(builder.TestData.RoadSegment1DbaseRecord);
                tempId = builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value;
            })
            .Build();

        var (features, problems) = new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8)
            .Read(archive, FeatureType.Change, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

        return (features.SingleOrDefault(feature => feature.Attributes.TempId == new RoadSegmentTempId(tempId))?.Attributes, problems);
    }
}
