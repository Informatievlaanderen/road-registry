namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

public static class DbaseFileProblems
{
    private static readonly NumberFormatInfo Provider = new()
    {
        NumberDecimalSeparator = "."
    };

    public static IDbaseFileRecordProblemBuilder WithIdentifier(this IDbaseFileRecordProblemBuilder builder, string field, int? value)
    {
        return WithDbaseIdentifier(builder, field, value);
    }
    public static IDbaseFileRecordProblemBuilder WithIdentifier(this IDbaseFileRecordProblemBuilder builder, string field, string value)
    {
        return WithDbaseIdentifier(builder, field, value);
    }

    private static IDbaseFileRecordProblemBuilder WithDbaseIdentifier(this IDbaseFileRecordProblemBuilder builder, string field, object value)
    {
        return (IDbaseFileRecordProblemBuilder)builder
            .WithParameters(
                new ProblemParameter("IdentifierField", field),
                new ProblemParameter("IdentifierValue", value?.ToString(Provider))
            );
    }

    public static FileError BeginRoadNodeIdEqualsEndRoadNode(this IDbaseFileRecordProblemBuilder builder,
        int beginNode,
        int endNode)
    {
        return builder
            .Error(nameof(BeginRoadNodeIdEqualsEndRoadNode))
            .WithParameter(new ProblemParameter("Begin", beginNode.ToString()))
            .WithParameter(new ProblemParameter("End", endNode.ToString()))
            .Build();
    }

    public static FileError BeginRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(BeginRoadNodeIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static string Describe(this DbaseSchema schema)
    {
        var builder = new StringBuilder();
        var index = 0;
        foreach (var field in schema.Fields)
        {
            if (index > 0) builder.Append(", ");
            builder.Append(field.Name.ToString());
            builder.Append("[");
            builder.Append(field.FieldType.ToString());
            builder.Append("(");
            builder.Append(field.Length.ToString());
            builder.Append(",");
            builder.Append(field.DecimalCount.ToString());
            builder.Append(")");
            builder.Append("]");
            index++;
        }

        return builder.ToString();
    }

    public static FileError DownloadIdDiffersFromMetadata(this IDbaseFileRecordProblemBuilder builder, string value, string expectedValue)
    {
        return builder
            .Error(nameof(DownloadIdDiffersFromMetadata))
            .WithParameter(new ProblemParameter("Actual", value))
            .WithParameter(new ProblemParameter("Expected", expectedValue))
            .Build();
    }

    public static FileError DownloadIdInvalidFormat(this IDbaseFileRecordProblemBuilder builder, string value)
    {
        return builder
            .Error(nameof(DownloadIdInvalidFormat))
            .WithParameter(new ProblemParameter("Actual", value))
            .Build();
    }

    public static FileError EndRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(EndRoadNodeIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError FromPositionEqualToOrGreaterThanToPosition(this IDbaseFileRecordProblemBuilder builder,
        double from, double to)
    {
        return builder
            .Error(nameof(FromPositionEqualToOrGreaterThanToPosition))
            .WithParameter(new ProblemParameter("From", from.ToString(Provider)))
            .WithParameter(new ProblemParameter("To", to.ToString(Provider)))
            .Build();
    }

    public static FileError FromPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
    {
        return builder
            .Error(nameof(FromPositionOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
            .Build();
    }

    public static FileError GradeSeparatedJunctionTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(GradeSeparatedJunctionTypeMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", GradeSeparatedJunctionType.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment(this IDbaseFileRecordProblemBuilder builder, RoadSegmentId roadSegmentId)
    {
        return builder
            .Error(nameof(GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment))
            .WithParameter(new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()))
            .Build();
    }

    public static FileError GradeSeparatedJunctionMissing(this IDbaseFileRecordProblemBuilder builder, RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2, double intersectionX, double intersectionY)
    {
        return builder
            .Error(nameof(GradeSeparatedJunctionMissing))
            .WithParameter(new ProblemParameter("RoadSegmentId1", roadSegmentId1.ToString()))
            .WithParameter(new ProblemParameter("RoadSegmentId2", roadSegmentId2.ToString()))
            .WithParameter(new ProblemParameter("IntersectionX", intersectionX.ToRoundedMeasurementString()))
            .WithParameter(new ProblemParameter("IntersectionY", intersectionY.ToRoundedMeasurementString()))
            .Build();
    }

    public static FileError ExpectedGradeSeparatedJunctionsCountDiffersFromActual(this IDbaseFileRecordProblemBuilder builder, RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2, int intersectionsCount, int gradeSeparatedJunctionsCount)
    {
        return builder
            .Error(nameof(ExpectedGradeSeparatedJunctionsCountDiffersFromActual))
            .WithParameter(new ProblemParameter("RoadSegmentId1", roadSegmentId1.ToString()))
            .WithParameter(new ProblemParameter("RoadSegmentId2", roadSegmentId2.ToString()))
            .WithParameter(new ProblemParameter("IntersectionsCount", intersectionsCount.ToString()))
            .WithParameter(new ProblemParameter("GradeSeparatedJunctionsCount", gradeSeparatedJunctionsCount.ToString()))
            .Build();
    }

    public static FileError HasDbaseHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return builder
            .Error(nameof(HasDbaseHeaderFormatError))
            .WithParameter(new ProblemParameter("Exception", exception.ToString()))
            .Build();
    }

    // record
    public static FileError HasDbaseRecordFormatError(this IDbaseFileRecordProblemBuilder builder, Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return builder
            .Error(nameof(HasDbaseRecordFormatError))
            .WithParameter(new ProblemParameter("Exception", exception.ToString()))
            .Build();
    }

    public static FileError HasDbaseSchemaMismatch(this IFileProblemBuilder builder, DbaseSchema expectedSchema, DbaseSchema actualSchema)
    {
        if (expectedSchema == null) throw new ArgumentNullException(nameof(expectedSchema));
        if (actualSchema == null) throw new ArgumentNullException(nameof(actualSchema));

        return builder
            .Error(nameof(HasDbaseSchemaMismatch))
            .WithParameters(
                new ProblemParameter("ExpectedSchema", Describe(expectedSchema)),
                new ProblemParameter("ActualSchema", Describe(actualSchema))
            )
            .Build();
    }

    // file
    public static FileProblem HasNoDbaseRecords(this IFileProblemBuilder builder, bool treatAsError = false)
    {
        if (treatAsError)
        {
            return builder.Error(nameof(HasNoDbaseRecords)).Build();
        }
        return builder.Warning(nameof(HasNoDbaseRecords)).Build();
    }

    public static FileProblem HasTooManyDbaseRecords(this IFileProblemBuilder builder, int expectedCount, int actualCount)
    {
        return builder
            .Error(nameof(HasNoDbaseRecords))
            .WithParameters(
                new ProblemParameter("ExpectedCount", expectedCount.ToString()),
                new ProblemParameter("ActualCount", actualCount.ToString()))
            .Build();
    }

    public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
        string identifier,
        RecordNumber takenByRecordNumber)
    {
        return builder
            .Error(nameof(IdentifierNotUnique))
            .WithParameters(
                new ProblemParameter("Identifier", identifier),
                new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
            .Build();
    }

    public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
        AttributeId identifier,
        RecordNumber takenByRecordNumber)
    {
        return IdentifierNotUnique(builder, identifier.ToString(), takenByRecordNumber);
    }

    // grade separated junction

    public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
        GradeSeparatedJunctionId identifier,
        RecordNumber takenByRecordNumber)
    {
        return IdentifierNotUnique(builder, identifier.ToString(), takenByRecordNumber);
    }

    // road node

    public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
        RoadNodeId identifier,
        RecordNumber takenByRecordNumber)
    {
        return IdentifierNotUnique(builder, identifier.ToString(), takenByRecordNumber);
    }

    public static FileError RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange(this IDbaseFileRecordProblemBuilder builder,
        RoadNodeId roadNodeId,
        RecordNumber takenByRecordNumber)
    {
        return builder
            .Error(nameof(RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange))
            .WithParameters(
                new ProblemParameter("Identifier", roadNodeId.ToString()),
                new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
            .Build();
    }

    public static FileError RoadNodeIsAlreadyProcessed(this IDbaseFileRecordProblemBuilder builder, RoadNodeId identifier, RoadNodeId processedId)
    {
        return builder
            .Error(nameof(RoadNodeIsAlreadyProcessed))
            .WithParameter(new ProblemParameter("Identifier", identifier.ToString()))
            .WithParameter(new ProblemParameter("ProcessedId", processedId.ToString()))
            .Build();
    }

    // road segment

    public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
        RoadSegmentId identifier,
        RecordNumber takenByRecordNumber)
    {
        return IdentifierNotUnique(builder, identifier.ToString(), takenByRecordNumber);
    }

    public static FileWarning IdentifierNotUniqueButAllowed(this IDbaseFileRecordProblemBuilder builder,
        RoadNodeId identifier,
        RecordType recordType,
        RecordNumber takenByRecordNumber,
        RecordType takenByRecordType)
    {
        return builder
            .Warning(nameof(IdentifierNotUniqueButAllowed))
            .WithParameters(
                new ProblemParameter("RecordType", recordType.ToString()),
                new ProblemParameter("Identifier", identifier.ToString()),
                new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()),
                new ProblemParameter("TakenByRecordType", takenByRecordType.ToString())
            )
            .Build();
    }

    public static FileError RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange(this IDbaseFileRecordProblemBuilder builder,
        RoadSegmentId roadSegmentId,
        RecordNumber takenByRecordNumber)
    {
        return builder
            .Error(nameof(RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange))
            .WithParameters(
                new ProblemParameter("Identifier", roadSegmentId.ToString()),
                new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
            .Build();
    }

    public static FileError IdentifierZero(this IDbaseFileRecordProblemBuilder builder)
    {
        return builder.Error(nameof(IdentifierZero)).Build();
    }

    public static FileError RoadSegmentIsAlreadyProcessed(this IDbaseFileRecordProblemBuilder builder, RoadSegmentId identifier, RoadSegmentId processedId)
    {
        return builder
            .Error(nameof(RoadSegmentIsAlreadyProcessed))
            .WithParameter(new ProblemParameter("Identifier", identifier.ToString()))
            .WithParameter(new ProblemParameter("ProcessedId", processedId.ToString()))
            .Build();
    }

    // lane

    public static FileError LaneCountOutOfRange(this IDbaseFileRecordProblemBuilder builder, int count)
    {
        return builder
            .Error(nameof(LaneCountOutOfRange))
            .WithParameter(new ProblemParameter("Actual", count.ToString()))
            .Build();
    }

    public static FileError LaneDirectionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(LaneDirectionMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadSegmentLaneDirection.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError LeftStreetNameIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(LeftStreetNameIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError LowerRoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(LowerRoadSegmentIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    // european road

    public static FileError NotEuropeanRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return builder
            .Error(nameof(NotEuropeanRoadNumber))
            .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
            .Build();
    }
    public static FileError EuropeanRoadNotUnique(this IDbaseFileRecordProblemBuilder builder, AttributeId attributeId, RecordNumber takenByRecordNumber, AttributeId takenByAttributeId)
    {
        ArgumentNullException.ThrowIfNull(attributeId);
        ArgumentNullException.ThrowIfNull(takenByRecordNumber);
        ArgumentNullException.ThrowIfNull(takenByAttributeId);

        return builder
            .Error(nameof(EuropeanRoadNotUnique))
            .WithParameter(new ProblemParameter("AttributeId", attributeId.ToString()))
            .WithParameter(new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
            .WithParameter(new ProblemParameter("TakenByAttributeId", takenByAttributeId.ToString()))
            .Build();
    }

    // national road

    public static FileError NotNationalRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return builder
            .Error(nameof(NotNationalRoadNumber))
            .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
            .Build();
    }
    public static FileError NationalRoadNotUnique(this IDbaseFileRecordProblemBuilder builder, AttributeId attributeId, RecordNumber takenByRecordNumber, AttributeId takenByAttributeId)
    {
        ArgumentNullException.ThrowIfNull(attributeId);
        ArgumentNullException.ThrowIfNull(takenByRecordNumber);
        ArgumentNullException.ThrowIfNull(takenByAttributeId);

        return builder
            .Error(nameof(NationalRoadNotUnique))
            .WithParameter(new ProblemParameter("AttributeId", attributeId.ToString()))
            .WithParameter(new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
            .WithParameter(new ProblemParameter("TakenByAttributeId", takenByAttributeId.ToString()))
            .Build();
    }

    // numbered road

    public static FileError NotNumberedRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return builder
            .Error(nameof(NotNumberedRoadNumber))
            .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
            .Build();
    }

    public static FileError NumberedRoadDirectionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(NumberedRoadDirectionMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadSegmentNumberedRoadDirection.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError NumberedRoadOrdinalOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(NumberedRoadOrdinalOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError OrganizationIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, string value)
    {
        return builder
            .Error(nameof(OrganizationIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value))
            .Build();
    }

    public static FileError ExtractDescriptionOutOfRange(this IDbaseFileRecordProblemBuilder builder, string value)
    {
        return builder
            .Error(nameof(ExtractDescriptionOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value))
            .Build();
    }

    public static FileError OperatorNameOutOfRange(this IDbaseFileRecordProblemBuilder builder, string value)
    {
        return builder
            .Error(nameof(OperatorNameOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value))
            .Build();
    }

    // record type

    public static FileError RecordTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(RecordTypeMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RecordType.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RecordTypeNotSupported(this IDbaseFileRecordProblemBuilder builder, int actual, params int[] expected)
    {
        return builder
            .Error(nameof(RecordTypeNotSupported))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", expected.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RequiredFieldIsNull(this IDbaseFileRecordProblemBuilder builder, DbaseField field)
    {
        return RequiredFieldIsNull(builder, field?.Name.ToString());
    }

    public static FileError RequiredFieldIsNull(this IDbaseFileRecordProblemBuilder builder, string field)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(field);

        return builder
            .Error(nameof(RequiredFieldIsNull))
            .WithParameter(new ProblemParameter("Field", field))
            .Build();
    }

    public static FileError RightStreetNameIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RightStreetNameIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError RoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadNodeIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError RoadNodeTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(RoadNodeTypeMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadNodeType.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }
    public static FileError RoadNodeGeometryMissing(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadNodeGeometryMissing))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError RoadSegmentAccessRestrictionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(RoadSegmentAccessRestrictionMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadSegmentAccessRestriction.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RoadSegmentCategoryMismatch(this IDbaseFileRecordProblemBuilder builder, string actual)
    {
        return builder
            .Error(nameof(RoadSegmentCategoryMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadSegmentCategory.ByIdentifier.Keys.Select(key => key))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual))
            .Build();
    }

    public static FileError RoadSegmentGeometryDrawMethodMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(RoadSegmentGeometryDrawMethodMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadSegmentGeometryDrawMethod.Allowed.Select(geometryDrawMethod => geometryDrawMethod.Translation.Identifier.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadSegmentIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError RoadSegmentMissing(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadSegmentMissing))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }
    public static FileError RoadSegmentStartNodeMissing(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadSegmentStartNodeMissing))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }
    public static FileError RoadSegmentEndNodeMissing(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadSegmentEndNodeMissing))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }
    public static FileError RoadSegmentGeometryMissing(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(RoadSegmentGeometryMissing))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    public static FileError RoadSegmentMorphologyMismatch(this IDbaseFileRecordProblemBuilder builder, int actual, bool outline = false)
    {
        return builder
            .Error(nameof(RoadSegmentMorphologyMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", outline
                        ? RoadSegmentMorphology.Edit.Editable.Select(status => status.Translation.Identifier.ToString())
                        : RoadSegmentMorphology.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RoadSegmentStatusMismatch(this IDbaseFileRecordProblemBuilder builder, int actual, bool outline = false)
    {
        return builder
            .Error(nameof(RoadSegmentStatusMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", outline
                        ? RoadSegmentStatus.Edit.Editable.Select(status => status.Translation.Identifier.ToString())
                        : RoadSegmentStatus.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError RoadSegmentMaintenanceAuthorityNotKnown(this IDbaseFileRecordProblemBuilder builder, OrganizationId organizationId)
    {
        return builder
            .Error(nameof(RoadSegmentMaintenanceAuthorityNotKnown))
            .WithParameter(new ProblemParameter(MaintenanceAuthorityNotKnown.ParameterName.OrganizationId, organizationId.ToString()))
            .Build();
    }

    public static FileError RoadSegmentsWithoutLaneAttributes(this IFileProblemBuilder builder, RoadSegmentId[] segments)
    {
        if (segments == null) throw new ArgumentNullException(nameof(segments));

        return builder
            .Error(nameof(RoadSegmentsWithoutLaneAttributes))
            .WithParameter(new ProblemParameter("Segments", string.Join(",", segments.Select(segment => segment.ToString()))))
            .Build();
    }

    public static FileError RoadSegmentsWithoutSurfaceAttributes(this IFileProblemBuilder builder, RoadSegmentId[] segments)
    {
        if (segments == null) throw new ArgumentNullException(nameof(segments));

        return builder
            .Error(nameof(RoadSegmentsWithoutSurfaceAttributes))
            .WithParameter(new ProblemParameter("Segments", string.Join(",", segments.Select(segment => segment.ToString()))))
            .Build();
    }

    public static FileError RoadSegmentsWithoutWidthAttributes(this IFileProblemBuilder builder, RoadSegmentId[] segments)
    {
        if (segments == null) throw new ArgumentNullException(nameof(segments));

        return builder
            .Error(nameof(RoadSegmentsWithoutWidthAttributes))
            .WithParameter(new ProblemParameter("Segments", string.Join(",", segments.Select(segment => segment.ToString()))))
            .Build();
    }

    // surface

    public static FileError SurfaceTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
    {
        return builder
            .Error(nameof(SurfaceTypeMismatch))
            .WithParameter(
                new ProblemParameter(
                    "ExpectedOneOf",
                    string.Join(",", RoadSegmentSurfaceType.ByIdentifier.Keys.Select(key => key.ToString()))
                )
            )
            .WithParameter(new ProblemParameter("Actual", actual.ToString()))
            .Build();
    }

    public static FileError ToPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
    {
        return builder
            .Error(nameof(ToPositionOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
            .Build();
    }

    public static FileError UpperRoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
    {
        return builder
            .Error(nameof(UpperRoadSegmentIdOutOfRange))
            .WithParameter(new ProblemParameter("Actual", value.ToString()))
            .Build();
    }

    // width

    public static FileError WidthOutOfRange(this IDbaseFileRecordProblemBuilder builder, int count)
    {
        return builder
            .Error(nameof(WidthOutOfRange))
            .WithParameter(new ProblemParameter("Actual", count.ToString()))
            .Build();
    }
}
