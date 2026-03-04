namespace RoadRegistry.Extracts.DutchTranslations;

using System.Text;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.Messages;
using RoadRegistry.ValueObjects.ProblemCodes;
using Schemas.Inwinning.RoadNodes;
using Schemas.Inwinning.RoadSegments;
using Uploads;
using FileProblem = Messages.FileProblem;

public sealed class DomainV2FileProblemTranslator : FileProblemTranslator
{
    public DomainV2FileProblemTranslator()
        : base(WellKnownProblemTranslators.Default)
    {
    }

    protected override ProblemTranslation? InnerTranslate(FileProblem problem)
    {
        var translation = new ProblemTranslation(problem.Severity, problem.Reason);

        var problemCodeTranslations = new Dictionary<ProblemCode, Func<ProblemTranslation>>
        {
            {
                ProblemCode.FromReason(nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08))!, () =>
                    translation with { Message = "Projectie formaat is niet 'Belge_Lambert_2008'." }
            },

            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadNodeGrensknoopMismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige grensknoop in veld {nameof(RoadNodeDbaseRecord.Schema.GRENSKNOOP)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },

            {
                ProblemCode.RoadSegment.Geometry.LengthIsZero, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} lengte is 0.")
            },
            {
                ProblemCode.RoadSegment.Geometry.LineCountMismatch, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} heeft meer lijnen dan verwacht.")
            },
            {
                ProblemCode.RoadSegment.Geometry.SelfIntersects, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} kruist zichzelf.")
            },
            {
                ProblemCode.RoadSegment.Geometry.SelfOverlaps, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} overlapt zichzelf.")
            },
            {
                ProblemCode.RoadSegment.Geometry.LengthLessThanMinimum, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} heeft niet de minimale lengte van {problem.GetParameterValue("Minimum")} meter.")
            },
            {
                ProblemCode.RoadSegment.Geometry.LengthTooLong, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} zijn lengte is groter of gelijk dan {problem.GetParameterValue("TooLongSegmentLength")} meter.")
            },
            {
                ProblemCode.RoadSegment.Geometry.VerticesTooClose, () => new(problem.Severity, problem.Reason,
                    $"De geometrie van {ShapeRecordLabel()} zijn afstand tussen de vertices bedraagt niet overal 15cm of meer.")
            },

            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentIdOutOfRange))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel(nameof(RoadSegmentDbaseRecord.WS_OIDN), problem.GetParameterValue("Actual"))} heeft een ongeldige wegsegment identificator." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentTempIdOutOfRange))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel(nameof(RoadSegmentDbaseRecord.WS_TEMPID), problem.GetParameterValue("Actual"))} heeft een ongeldige wegsegment identificator." }
            },
            // {
            //     ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodV2Mismatch))!, () =>
            //         translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige methode in veld {nameof(RoadSegmentDbaseRecord.Schema.METHODE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." } //TODO-pr uncomment voor recurrente bijhouding
            // },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentAccessRestrictionV2Mismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentDbaseRecord.Schema.TOEGANG)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentMorphologyV2Mismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige morfologie in veld {nameof(RoadSegmentDbaseRecord.Schema.MORF)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentStatusV2Mismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige status in veld {nameof(RoadSegmentDbaseRecord.Schema.STATUS)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentSurfaceTypeV2Mismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentDbaseRecord.Schema.VERHARDING)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentAutoHeenMismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.AUTOHEEN)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentAutoTerugMismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.AUTOTERUG)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentFietsHeenMismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.FIETSHEEN)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentFietsTerugMismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.FIETSTERUG)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },
            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.RoadSegmentVoetgangerMismatch))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.VOETGANGER)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }
            },

            {
                ProblemCode.FromReason(nameof(DbaseFileProblems.GradeSeparatedJunctionNotUnique))!, () =>
                    translation with { Message = $"De {DbaseRecordLabel()} heeft hetzelfde onder- en bovenliggende wegsegment als de dbase record met {problem.GetParameterValue("IdentifierField")} {problem.GetParameterValue("OtherJunctionId")}" }
            }
        };

        var problemCode = ProblemCode.FromReason(problem.Reason);
        if (problemCode is not null && problemCodeTranslations.TryGetValue(problemCode, out var translator))
        {
            return translator();
        }

        return DomainV1.Translate(problem);

        string DbaseRecordLabel(string? identifierField = null, string? identifierValue = null)
        {
            var sb = new StringBuilder();
            sb.Append("dbase record ");
            sb.Append(problem.GetParameterValue("RecordNumber"));

            identifierField ??= problem.GetParameterValue("IdentifierField", required: false);
            identifierValue ??= problem.GetParameterValue("IdentifierValue", required: false);
            if (!string.IsNullOrEmpty(identifierField) && !string.IsNullOrEmpty(identifierValue) && identifierValue != "0")
            {
                sb.Append(" met ");
                sb.Append(identifierField);
                sb.Append(" ");
                sb.Append(identifierValue);
            }

            return sb.ToString();
        }

        string ShapeRecordLabel(string? identifierField = null, string? identifierValue = null)
        {
            var sb = new StringBuilder();
            sb.Append("shape record ");
            sb.Append(problem.GetParameterValue("RecordNumber"));

            identifierField ??= problem.GetParameterValue("IdentifierField", required: false);
            identifierValue ??= problem.GetParameterValue("IdentifierValue", required: false);
            if (!string.IsNullOrEmpty(identifierField) && !string.IsNullOrEmpty(identifierValue) && identifierValue != "0")
            {
                sb.Append(" met ");
                sb.Append(identifierField);
                sb.Append(" ");
                sb.Append(identifierValue);
            }

            return sb.ToString();
        }
    }
}
