namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using System.IO.Compression;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Xunit.Sdk;
using IZipArchiveFeatureCompareTranslator = RoadRegistry.Extracts.FeatureCompare.DomainV2.IZipArchiveFeatureCompareTranslator;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public abstract class FeatureCompareTranslatorScenariosBase
{
    protected ITestOutputHelper TestOutputHelper { get; }
    protected ILogger<ZipArchiveFeatureCompareTranslator> Logger { get; }

    protected FeatureCompareTranslatorScenariosBase(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
    {
        TestOutputHelper = testOutputHelper;
        Logger = logger;
    }

    protected async Task<TranslatedChanges> TranslateSucceeds(
        ZipArchive archive,
        IZipArchiveFeatureCompareTranslator translator = null)
    {
        using (archive)
        {
            var sut = translator ?? ZipArchiveFeatureCompareTranslatorV3Builder.Create();

            try
            {
                return await sut.TranslateAsync(archive, ZipArchiveMetadata.Empty.WithInwinning(), CancellationToken.None);
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

    protected async Task TranslateReturnsExpectedResult(ZipArchive archive, TranslatedChanges expected, IZipArchiveFeatureCompareTranslator translator = null)
    {
        TranslatedChanges result = null;
        try
        {
            result = await TranslateSucceeds(archive, translator);

            Assert.Equal(expected, result);
        }
        catch (EqualException)
        {
            TestOutputHelper.WriteLine($"Expected:\n{expected.Describe()}");
            await File.WriteAllTextAsync("expected.txt", $"Expected:\n{expected.Describe()}");
            TestOutputHelper.WriteLine($"Actual:\n{result?.Describe()}");
            await File.WriteAllTextAsync("actual.txt", $"Actual:\n{result?.Describe()}");
            throw;
        }
    }

    protected static RoadSegmentDynamicAttributeValues<StreetNameLocalId> BuildStreetNameIdAttributes(StreetNameLocalId? leftSideStreetNameId, StreetNameLocalId? rightSideStreetNameId, RoadSegmentGeometry geometry)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(StreetNameLocalId.NotApplicable, geometry);
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(new(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length)), RoadSegmentAttributeSide.Left, leftSideStreetNameId ?? StreetNameLocalId.NotApplicable)
            .Add(new(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length)), RoadSegmentAttributeSide.Right, rightSideStreetNameId ?? StreetNameLocalId.NotApplicable);
    }

    protected static RoadSegmentDynamicAttributeValues<OrganizationId> BuildOrganizationIdAttributes(OrganizationId leftSide, OrganizationId rightSide, RoadSegmentGeometry geometry)
    {
        return new RoadSegmentDynamicAttributeValues<OrganizationId>()
            .Add(new(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length)), RoadSegmentAttributeSide.Left, leftSide)
            .Add(new(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length)), RoadSegmentAttributeSide.Right, rightSide);
    }

    protected static AddRoadSegmentChange BuildAddRoadSegmentChange(RoadSegmentDbaseRecord dbaseRecord, RoadSegmentShapeRecord shapeRecord)
    {
        var geometry = shapeRecord.Geometry.ToRoadSegmentGeometry();

        return new AddRoadSegmentChange
        {
            TemporaryId = new RoadSegmentId(dbaseRecord.WS_OIDN.Value),
            OriginalId = new RoadSegmentId(dbaseRecord.WS_OIDN.Value),
            Geometry = geometry,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = RoadSegmentStatusV2.ByIdentifier[dbaseRecord.STATUS.Value],
            MaintenanceAuthorityId = BuildOrganizationIdAttributes(new OrganizationId(dbaseRecord.LBEHEER.Value!), new OrganizationId(dbaseRecord.RBEHEER.Value!), geometry),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(RoadSegmentMorphologyV2.ByIdentifier[dbaseRecord.MORF.Value], geometry),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(RoadSegmentCategoryV2.ByIdentifier[dbaseRecord.WEGCAT.Value], geometry),
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(RoadSegmentAccessRestrictionV2.ByIdentifier[dbaseRecord.TOEGANG.Value], geometry),
            StreetNameId = BuildStreetNameIdAttributes(StreetNameLocalId.FromValue(dbaseRecord.LSTRNMID.Value), StreetNameLocalId.FromValue(dbaseRecord.RSTRNMID.Value), geometry),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(RoadSegmentSurfaceTypeV2.ByIdentifier[dbaseRecord.VERHARDING.Value], geometry),
            CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.AUTOHEEN.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.AUTOTERUG.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.FIETSHEEN.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.FIETSTERUG.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.VOETGANGER.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
    }

    protected static ModifyRoadSegmentChange BuildModifyRoadSegmentChange(RoadSegmentDbaseRecord dbaseRecord, RoadSegmentShapeRecord shapeRecord)
    {
        var geometry = shapeRecord.Geometry.ToRoadSegmentGeometry();

        return new ModifyRoadSegmentChange
        {
            RoadSegmentId = new RoadSegmentId(dbaseRecord.WS_OIDN.Value),
            OriginalId = null,
            Geometry = geometry,
            Status = RoadSegmentStatusV2.ByIdentifier[dbaseRecord.STATUS.Value],
            MaintenanceAuthorityId = BuildOrganizationIdAttributes(new OrganizationId(dbaseRecord.LBEHEER.Value!), new OrganizationId(dbaseRecord.RBEHEER.Value!), geometry),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(RoadSegmentMorphologyV2.ByIdentifier[dbaseRecord.MORF.Value], geometry),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(RoadSegmentCategoryV2.ByIdentifier[dbaseRecord.WEGCAT.Value], geometry),
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(RoadSegmentAccessRestrictionV2.ByIdentifier[dbaseRecord.TOEGANG.Value], geometry),
            StreetNameId = BuildStreetNameIdAttributes(StreetNameLocalId.FromValue(dbaseRecord.LSTRNMID.Value), StreetNameLocalId.FromValue(dbaseRecord.RSTRNMID.Value), geometry),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(RoadSegmentSurfaceTypeV2.ByIdentifier[dbaseRecord.VERHARDING.Value], geometry),
            CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.AUTOHEEN.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.AUTOTERUG.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.FIETSHEEN.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.FIETSTERUG.Value.ToBooleanFromDbaseValue()!.Value, geometry),
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(dbaseRecord.VOETGANGER.Value.ToBooleanFromDbaseValue()!.Value, geometry)
        };
    }
}
