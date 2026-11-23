namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Xunit.Abstractions;

// GAWR-7784: only an active street name can be linked to an active road segment, the same as the edit API demands.
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
    public async Task WhenAnActiveRoadSegmentGetsARetiredOrRejectedStreetName_ThenTheUploadIsRefused(bool leftSide)
    {
        var (zipArchive, _) = new DomainV2ZipArchiveBuilder(fixture => { fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd); })
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
            .BuildWithContext();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
            streetNameContextFactory: new StreetNameContextFactory(new StreetNameContext([InactiveStreetNameId])));

        var act = () => TranslateSucceeds(zipArchive, translator);

        var exception = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        exception.Which.Problems.Should().Contain(x => x.Reason == (leftSide
            ? nameof(DbaseFileProblems.LeftStreetNameIdIsNotProposedOrCurrent)
            : nameof(DbaseFileProblems.RightStreetNameIdIsNotProposedOrCurrent)));
    }

    [Fact]
    public async Task WhenAnActiveRoadSegmentGetsAnActiveStreetName_ThenTheUploadIsAccepted()
    {
        var (zipArchive, _) = new DomainV2ZipArchiveBuilder(fixture => { fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd); })
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = InactiveStreetNameId; })
            .BuildWithContext();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(
            streetNameContextFactory: new StreetNameContextFactory(new StreetNameContext([])));

        await TranslateSucceeds(zipArchive, translator);
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
    }
}
