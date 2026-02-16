namespace RoadRegistry.Extracts.DutchTranslations;

using System.Text;
using RoadRegistry.Infrastructure.Messages;
using Schemas.Inwinning.RoadNodes;
using Schemas.Inwinning.RoadSegments;
using Uploads;
using FileProblem = Messages.FileProblem;

public static partial class FileProblemTranslator
{
    private static readonly Converter<FileProblem, ProblemTranslation?> TranslateInwinning = problem =>
    {
        var translation = new ProblemTranslation(problem.Severity, problem.Reason);

        string DbaseRecordLabel(string? identifierField = null, string? identifierValue = null)
        {
            var sb = new StringBuilder();
            sb.Append("dbase record ");
            sb.Append(problem.GetParameterValue("RecordNumber"));

            identifierField ??= problem.GetParameterValue("IdentifierField", required: false);
            identifierValue ??= problem.GetParameterValue("IdentifierValue", required: false);
            if (!string.IsNullOrEmpty(identifierField) && !string.IsNullOrEmpty(identifierValue))
            {
                sb.Append(" met ");
                sb.Append(identifierField);
                sb.Append(" ");
                sb.Append(identifierValue);
            }

            return sb.ToString();
        }

        return problem.Reason switch
        {
            nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08) => translation with { Message = "Projectie formaat is niet 'Belge_Lambert_2008'." },
            nameof(DbaseFileProblems.RoadNodeGrensknoopMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige grensknoop in veld {nameof(RoadNodeDbaseRecord.Schema.GRENSKNOOP)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentSurfaceTypeV2Mismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentDbaseRecord.Schema.VERHARDING)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentAutoHeenMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.AUTOHEEN)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentAutoTerugMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.AUTOTERUG)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentFietsHeenMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.FIETSHEEN)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentFietsTerugMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.FIETSTERUG)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentVoetgangerMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.VOETGANGER)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            _ => null
        };
    };
}
