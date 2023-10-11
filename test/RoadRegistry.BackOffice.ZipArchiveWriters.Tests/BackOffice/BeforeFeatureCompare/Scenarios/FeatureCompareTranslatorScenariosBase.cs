namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Exceptions;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using Uploads;
using Xunit.Abstractions;
using Xunit.Sdk;

public abstract class FeatureCompareTranslatorScenariosBase
{
    protected ITestOutputHelper TestOutputHelper { get; }
    protected ILogger<ZipArchiveFeatureCompareTranslator> Logger { get; }
    protected readonly Encoding Encoding = Encoding.UTF8;

    protected FeatureCompareTranslatorScenariosBase(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
    {
        TestOutputHelper = testOutputHelper;
        Logger = logger;
    }

    protected Fixture CreateFixture(RoadRegistry.Tests.BackOffice.ExtractsZipArchiveTestData testData)
    {
        var fixture = testData.Fixture;

        fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<GradeSeparatedJunctionDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE =
                        { Value = (short)fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                    BO_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
                {
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT2 = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentNumberedRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentNumberedRoadAttributeDbaseRecord
                {
                    GW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT8 = { Value = fixture.Create<NumberedRoadNumber>().ToString() },
                    RICHTING =
                    {
                        Value = (short)fixture.Create<RoadSegmentNumberedRoadDirection>().Translation
                            .Identifier
                    },
                    VOLGNUMMER = { Value = fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadNodeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadNodeDbaseRecord
                {
                    TYPE = { Value = (short)fixture.Create<RoadNodeType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                    METHODE =
                    {
                        Value = (short)fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier
                    },
                    BEHEERDER = { Value = fixture.Create<OrganizationId>() },
                    MORFOLOGIE =
                        { Value = (short)fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                    STATUS = { Value = fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                    CATEGORIE = { Value = fixture.Create<RoadSegmentCategory>().Translation.Identifier },
                    B_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    E_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    LSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    RSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    TGBEP =
                    {
                        Value = (short)fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier
                    }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentLaneAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    AANTAL = { Value = (short)fixture.Create<RoadSegmentLaneCount>().ToInt32() },
                    RICHTING =
                    {
                        Value = (short)fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier
                    }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentSurfaceAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TYPE = { Value = (short)fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentWidthAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    BREEDTE = { Value = (short)fixture.Create<RoadSegmentWidth>().ToInt32() }
                })
                .OmitAutoProperties());

        return fixture;
    }

    protected async Task<TranslatedChanges> TranslateSucceeds(ZipArchive archive)
    {
        using (archive)
        {
            var sut = new ZipArchiveFeatureCompareTranslator(Encoding, Logger, new UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle(true));

            try
            {
                return await sut.Translate(archive, CancellationToken.None);
            }
            catch (ZipArchiveValidationException ex)
            {
                foreach (var problem in ex.Problems)
                {
                    TestOutputHelper.WriteLine(problem.Describe());
                }
                throw;
            }
        }
    }

    protected async Task TranslateReturnsExpectedResult(ZipArchive archive, TranslatedChanges expected)
    {
        TranslatedChanges result = null;
        try
        {
            result = await TranslateSucceeds(archive);

            Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
        }
        catch (EqualException)
        {
            TestOutputHelper.WriteLine($"Expected:\n{expected.Describe()}");
            TestOutputHelper.WriteLine($"Actual:\n{result?.Describe()}");
            throw;
        }
    }
}
