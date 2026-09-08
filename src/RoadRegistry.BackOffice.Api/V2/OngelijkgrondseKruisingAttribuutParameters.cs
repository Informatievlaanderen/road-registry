namespace RoadRegistry.BackOffice.Api.V2;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

/// <summary>
///     The attribute values of a grade separated junction, as a caller states them. Shared by the two endpoints that
///     take them: recording a grade junction as a grade separated one, and correcting a grade separated one.
/// </summary>
internal static class OngelijkgrondseKruisingAttribuutParameters
{
    // VAL-4, VAL-6, VAL-9, VAL-10. Everything about the crossing itself - whether these are its roads, whether they
    // differ - is left to the domain, which is the only place that knows the junction.
    public static (RoadSegmentId Lower, RoadSegmentId Upper, GradeSeparatedJunctionTypeV2 Type) TranslateAndValidate(
        string? onderliggendWegsegment,
        string? bovenliggendWegsegment,
        string? ongelijkgrondseKruisingType)
    {
        var failures = new List<ValidationFailure>();

        var lower = ParseRoadSegmentId(onderliggendWegsegment, "onderliggendWegsegment", failures);
        var upper = ParseRoadSegmentId(bovenliggendWegsegment, "bovenliggendWegsegment", failures);
        var type = ParseType(ongelijkgrondseKruisingType, failures);

        if (failures.Count > 0)
        {
            throw new FluentValidation.ValidationException(failures);
        }

        return (lower!.Value, upper!.Value, type!);
    }

    private static RoadSegmentId? ParseRoadSegmentId(string? value, string parameterName, List<ValidationFailure> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(new ValidationFailure(parameterName, $"De parameter '{parameterName}' is verplicht."));
            return null;
        }

        if (!int.TryParse(value, out var parsed) || !RoadSegmentId.Accepts(parsed))
        {
            failures.Add(new ValidationFailure(parameterName, $"De parameter '{parameterName}' heeft een ongeldige waarde."));
            return null;
        }

        return new RoadSegmentId(parsed);
    }

    private static GradeSeparatedJunctionTypeV2? ParseType(string? value, List<ValidationFailure> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(new ValidationFailure("ongelijkgrondseKruisingType", "De parameter 'ongelijkgrondseKruisingType' is verplicht."));
            return null;
        }

        var type = GradeSeparatedJunctionTypeV2.Edit.Values.FirstOrDefault(x => string.Equals(x.ToDutchString(), value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (type is null)
        {
            failures.Add(new ValidationFailure("ongelijkgrondseKruisingType", "De parameter 'ongelijkgrondseKruisingType' heeft een ongeldige waarde."));
            return null;
        }

        return type;
    }
}
