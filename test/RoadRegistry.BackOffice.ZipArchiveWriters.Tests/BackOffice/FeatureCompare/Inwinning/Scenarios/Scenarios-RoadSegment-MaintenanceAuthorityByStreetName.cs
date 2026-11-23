namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.Inwinning.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.FeatureCompare.Inwinning;
using RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.Inwinning;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.Inwinning.TranslatedChanges;

// GAWR-7780: the street names lead the maintenance authority of a road segment on a municipality border.
public class RoadSegmentMaintenanceAuthorityByStreetNameScenarios : FeatureCompareTranslatorScenariosBase
{
    private const int LeftStreetNameId = 1;
    private const int RightStreetNameId = 2;
    private const string MunicipalityA = "23000";
    private const string MunicipalityB = "24038";
    private const string OtherMaintenanceAuthority = "AWV112";

    public RoadSegmentMaintenanceAuthorityByStreetNameScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Theory]
    [InlineData(MunicipalityB, MunicipalityB)]
    [InlineData(MunicipalityA, MunicipalityA)]
    [InlineData(MunicipalityB, MunicipalityA)]
    [InlineData(MunicipalityA, MunicipalityB)]
    public async Task WhenStreetNamesInDifferentMunicipalitiesAndMaintenanceAuthorityIsOneOfThem_ThenLeftAndRightFollowTheStreetNames(string leftMaintenanceAuthority, string rightMaintenanceAuthority)
    {
        var change = await TranslateRoadSegment1(MunicipalityA, MunicipalityB, leftMaintenanceAuthority, rightMaintenanceAuthority);

        ValueAt(change, RoadSegmentAttributeSide.Links).Should().Be(new OrganizationId(MunicipalityA));
        ValueAt(change, RoadSegmentAttributeSide.Rechts).Should().Be(new OrganizationId(MunicipalityB));
    }

    [Theory]
    [InlineData(OtherMaintenanceAuthority, OtherMaintenanceAuthority)]
    [InlineData(MunicipalityA, OtherMaintenanceAuthority)]
    [InlineData(OtherMaintenanceAuthority, MunicipalityB)]
    public async Task WhenMaintenanceAuthorityIsNotOneOfBothMunicipalities_ThenItIsLeftAlone(string leftMaintenanceAuthority, string rightMaintenanceAuthority)
    {
        var change = await TranslateRoadSegment1(MunicipalityA, MunicipalityB, leftMaintenanceAuthority, rightMaintenanceAuthority);

        ValueAt(change, RoadSegmentAttributeSide.Links).Should().Be(new OrganizationId(leftMaintenanceAuthority));
        ValueAt(change, RoadSegmentAttributeSide.Rechts).Should().Be(new OrganizationId(rightMaintenanceAuthority));
    }

    [Fact]
    public async Task WhenStreetNamesInSameMunicipality_ThenMaintenanceAuthorityIsLeftAlone()
    {
        var change = await TranslateRoadSegment1(MunicipalityA, MunicipalityA, MunicipalityB, MunicipalityB);

        ValueAt(change, RoadSegmentAttributeSide.Links).Should().Be(new OrganizationId(MunicipalityB));
        ValueAt(change, RoadSegmentAttributeSide.Rechts).Should().Be(new OrganizationId(MunicipalityB));
    }

    [Fact]
    public async Task WhenOneSideHasNoStreetName_ThenMaintenanceAuthorityIsLeftAlone()
    {
        var change = await TranslateRoadSegment1(MunicipalityA, null, MunicipalityB, MunicipalityB);

        ValueAt(change, RoadSegmentAttributeSide.Links).Should().Be(new OrganizationId(MunicipalityB));
        ValueAt(change, RoadSegmentAttributeSide.Rechts).Should().Be(new OrganizationId(MunicipalityB));
    }

    [Fact]
    public void WhenAlreadyCorrect_ThenNoCorrection()
    {
        MaintenanceAuthorityByStreetNameMunicipality.TryCorrect(
                MunicipalityA, MunicipalityB, new OrganizationId(MunicipalityA), new OrganizationId(MunicipalityB), out _, out _)
            .Should().BeFalse();
    }

    private async Task<ModifyRoadSegmentChange> TranslateRoadSegment1(
        string leftStreetNameNisCode,
        string? rightStreetNameNisCode,
        string leftMaintenanceAuthority,
        string rightMaintenanceAuthority)
    {
        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(LeftStreetNameId, "Links", string.Empty, nisCode: leftStreetNameNisCode);
        if (rightStreetNameNisCode is not null)
        {
            streetNameCache.AddStreetName(RightStreetNameId, "Rechts", string.Empty, nisCode: rightStreetNameNisCode);
        }

        var roadSegmentId = 0;
        var zipArchive = new InwinningZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                var record = builder.TestData.RoadSegment1DbaseRecord;
                roadSegmentId = record.WS_OIDN.Value!.Value;
                record.LSTRNMID.Value = LeftStreetNameId;
                record.RSTRNMID.Value = rightStreetNameNisCode is not null ? RightStreetNameId : StreetNameLocalId.NotApplicable;
                record.LBEHEER.Value = leftMaintenanceAuthority;
                record.RBEHEER.Value = rightMaintenanceAuthority;

                foreach (var streetNameId in builder.DataSet.RoadSegmentDbaseRecords
                             .SelectMany(x => new[] { x.LSTRNMID.Value, x.RSTRNMID.Value })
                             .Where(x => x is > 0 && x != LeftStreetNameId && x != RightStreetNameId)
                             .Select(x => x!.Value)
                             .Distinct())
                {
                    streetNameCache.AddStreetName(streetNameId, string.Empty, string.Empty);
                }
            })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache));
        var result = await TranslateSucceeds(zipArchive, translator);

        return FindModifyRoadSegmentChange(result, roadSegmentId);
    }

    private static ModifyRoadSegmentChange FindModifyRoadSegmentChange(TranslatedChanges changes, int roadSegmentId)
    {
        return changes
            .OfType<ModifyRoadSegmentChange>()
            .Single(x => x.RoadSegmentIdReference.RoadSegmentId == new RoadSegmentId(roadSegmentId));
    }

    private static OrganizationId ValueAt(ModifyRoadSegmentChange change, RoadSegmentAttributeSide side)
    {
        return change.MaintenanceAuthorityId!.Values
            .Single(x => x.Side == side || x.Side == RoadSegmentAttributeSide.Beide)
            .Value;
    }
}
