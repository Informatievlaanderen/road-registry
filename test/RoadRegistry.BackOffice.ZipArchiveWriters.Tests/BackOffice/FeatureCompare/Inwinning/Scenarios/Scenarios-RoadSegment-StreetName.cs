namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.Inwinning.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.FeatureCompare.Inwinning;
using RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.Inwinning;
using Xunit.Abstractions;

// GAWR-7784: only an active street name is linked to a road segment. Unlike the change upload, the inwinning does not
// refuse a retired or rejected street name: it lets go of it without a word, the same as of a removed one.
public class RoadSegmentStreetNameScenarios : FeatureCompareTranslatorScenariosBase
{
    private const int InactiveStreetNameId = 777;

    public RoadSegmentStreetNameScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenAnActiveRoadSegmentGetsARetiredOrRejectedStreetName_ThenItIsLetGoWithoutAProblem(bool leftSide)
    {
        var zipArchive = BuildZipArchive(RoadSegmentStatusV2.Gerealiseerd, leftSide);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
            streetNameContextFactory: new StreetNameContextFactory(new StreetNameContext([InactiveStreetNameId])));

        var result = await TranslateSucceeds(zipArchive, translator);

        StreetNameIds(result).Should().NotContain(new StreetNameLocalId(InactiveStreetNameId));
    }

    [Fact]
    public async Task WhenAnInactiveRoadSegmentHasARetiredOrRejectedStreetName_ThenTheStreetNameIsNotRefused()
    {
        var zipArchive = BuildZipArchive(RoadSegmentStatusV2.Gehistoreerd, leftSide: true);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
            streetNameContextFactory: new StreetNameContextFactory(new StreetNameContext([InactiveStreetNameId])));

        var act = () => TranslateSucceeds(zipArchive, translator);

        // Other rules may still refuse a historized road segment in this test data; only the street name check matters.
        try
        {
            await act();
        }
        catch (ZipArchiveValidationException exception)
        {
            exception.Problems.Should().NotContain(x =>
                x.Reason == nameof(DbaseFileProblems.LeftStreetNameIdIsNotProposedOrCurrent)
                || x.Reason == nameof(DbaseFileProblems.RightStreetNameIdIsNotProposedOrCurrent));
        }
    }

    [Fact]
    public async Task WhenAnActiveRoadSegmentGetsAnActiveStreetName_ThenTheUploadIsAccepted()
    {
        var zipArchive = BuildZipArchive(RoadSegmentStatusV2.Gerealiseerd, leftSide: true);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
            streetNameContextFactory: new StreetNameContextFactory(new StreetNameContext([])));

        var result = await TranslateSucceeds(zipArchive, translator);

        StreetNameIds(result).Should().Contain(new StreetNameLocalId(InactiveStreetNameId));
    }

    private static IEnumerable<StreetNameLocalId> StreetNameIds(TranslatedChanges changes)
    {
        return changes.OfType<ModifyRoadSegmentChange>().SelectMany(x => x.StreetNameId?.Values.Select(value => value.Value) ?? [])
            .Concat(changes.OfType<AddRoadSegmentChange>().SelectMany(x => x.StreetNameId.Values.Select(value => value.Value)));
    }

    private static System.IO.Compression.ZipArchive BuildZipArchive(RoadSegmentStatusV2 status, bool leftSide)
    {
        return new InwinningZipArchiveBuilder(fixture => fixture.Freeze(status))
            .WithChange((builder, _) =>
            {
                if (leftSide)
                {
                    builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = InactiveStreetNameId;
                }
                else
                {
                    builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = InactiveStreetNameId;
                }
            })
            .Build();
    }

    private sealed class StreetNameContextFactory : IRoadSegmentFeatureCompareStreetNameContextFactory
    {
        private readonly IRoadSegmentFeatureCompareStreetNameContext _context;

        public StreetNameContextFactory(IRoadSegmentFeatureCompareStreetNameContext context)
        {
            _context = context;
        }

        public Task<IRoadSegmentFeatureCompareStreetNameContext> Create(ICollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> changeFeatures, CancellationToken cancellationToken)
        {
            return Task.FromResult(_context);
        }
    }

    private sealed class StreetNameContext : IRoadSegmentFeatureCompareStreetNameContext
    {
        private readonly int[] _retiredOrRejectedIds;

        public StreetNameContext(int[] retiredOrRejectedIds)
        {
            _retiredOrRejectedIds = retiredOrRejectedIds;
        }

        public bool IsValid(StreetNameLocalId id) => true;
        public bool IsRemoved(StreetNameLocalId id) => false;
        public bool IsRetiredOrRejected(StreetNameLocalId id) => _retiredOrRejectedIds.Contains(id.ToInt32());

        public bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId)
        {
            renamedToId = default;
            return false;
        }

        public bool TryGetNisCode(StreetNameLocalId id, out string nisCode)
        {
            nisCode = string.Empty;
            return false;
        }
    }
}
