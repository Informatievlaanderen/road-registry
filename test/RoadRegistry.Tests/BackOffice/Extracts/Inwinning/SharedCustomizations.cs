namespace RoadRegistry.Tests.BackOffice.Extracts.Inwinning;

using AutoFixture;
using RoadRegistry.Extracts.Schemas.Inwinning;
using RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;

public static class SharedCustomizations
{
    public static void CustomizeInwinningRoadNodeDbaseRecord(this IFixture fixture)
    {
        fixture.Customize<RoadNodeDbaseRecord>(composer => composer
            .FromFactory(random => new RoadNodeDbaseRecord
            {
                WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                TYPE = { Value = (short)fixture.Create<RoadNodeTypeV2>().Translation.Identifier },
                GRENSKNOOP = { Value = (short)random.Next(0, 2) }
            })
            .OmitAutoProperties());
    }

    public static void CustomizeInwinningRoadSegmentDbaseRecord(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentDbaseRecord>(composer => composer
            .FromFactory(random => new RoadSegmentDbaseRecord
            {
                WS_TEMPID = { Value = random.Next(1, int.MaxValue) },
                WS_OIDN = { Value = fixture.Create<RoadSegmentId>() },
                LBEHEER = { Value = fixture.Create<OrganizationId>() },
                RBEHEER = { Value = fixture.Create<OrganizationId>() },
                MORF = { Value = fixture.Create<RoadSegmentMorphologyV2>().Translation.Identifier },
                STATUS = { Value = fixture.Create<RoadSegmentStatusV2>().Translation.Identifier },
                WEGCAT = { Value = fixture.Create<RoadSegmentCategoryV2>().Translation.Identifier },
                LSTRNMID = { Value = fixture.Create<StreetNameLocalId>() },
                RSTRNMID = { Value = fixture.Create<StreetNameLocalId>() },
                TOEGANG = { Value = fixture.Create<RoadSegmentAccessRestrictionV2>().Translation.Identifier },
                VERHARDING = { Value = fixture.Create<RoadSegmentSurfaceTypeV2>().Translation.Identifier },
                AUTOHEEN = { Value = random.Next(0, 2) },
                AUTOTERUG = { Value = random.Next(0, 2) },
                FIETSHEEN = { Value = random.Next(0, 2) },
                FIETSTERUG = { Value = random.Next(0, 2) },
                VOETGANGER = { Value = random.Next(0, 2) }
            })
            .OmitAutoProperties());
    }

    public static void CustomizeInwinningRoadSegmentEuropeanRoadAttributeDbaseRecord(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());
    }

    public static void CustomizeInwinningRoadSegmentNationalRoadAttributeDbaseRecord(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
                {
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    NWNUMMER = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());
    }

    public static void CustomizeInwinningGradeSeparatedJunctionDbaseRecord(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)fixture.Create<GradeSeparatedJunctionTypeV2>().Translation.Identifier },
                    BO_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());
    }

    public static void CustomizeInwinningTransactionZoneDbaseRecord(this IFixture fixture)
    {
        fixture.Customize<TransactionZoneDbaseRecord>(
            composer => composer
                .FromFactory(random => new TransactionZoneDbaseRecord
                {
                    TYPE = { Value = random.Next(1, 9999) },
                    BESCHRIJV = { Value = fixture.Create<Reason>().ToString() },
                    DOWNLOADID = { Value = fixture.Create<DownloadId>().ToString() },
                })
                .OmitAutoProperties());
    }
}
