namespace RoadRegistry.Extracts.DutchTranslations;

using System.Text;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.Messages;
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

    protected override ProblemTranslation? InnerTranslate(FileProblem fileProblem)
    {
        var translation = new ProblemTranslation(fileProblem.Severity, fileProblem.Reason);

        return fileProblem.Reason switch
        {
            nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08) => translation with { Message = "Projectie formaat is niet 'Belge_Lambert_2008'." },
            nameof(DbaseFileProblems.RoadNodeGrensknoopMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige grensknoop in veld {nameof(RoadNodeDbaseRecord.Schema.GRENSKNOOP)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            //nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodV2Mismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige methode in veld {nameof(RoadSegmentDbaseRecord.Schema.METHODE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." }, //TODO-pr uncomment voor recurrente bijhouding
            nameof(DbaseFileProblems.RoadSegmentAccessRestrictionV2Mismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentDbaseRecord.Schema.TOEGANG)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMorphologyV2Mismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige morfologie in veld {nameof(RoadSegmentDbaseRecord.Schema.MORF)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentStatusV2Mismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige status in veld {nameof(RoadSegmentDbaseRecord.Schema.STATUS)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentSurfaceTypeV2Mismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentDbaseRecord.Schema.VERHARDING)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentAutoHeenMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.AUTOHEEN)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentAutoTerugMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.AUTOTERUG)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentFietsHeenMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.FIETSHEEN)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentFietsTerugMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.FIETSTERUG)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentVoetgangerMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige waarde in veld {nameof(RoadSegmentDbaseRecord.Schema.VOETGANGER)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            _ => DomainV1.Translate(fileProblem)
        };

        string DbaseRecordLabel(string? identifierField = null, string? identifierValue = null)
        {
            var sb = new StringBuilder();
            sb.Append("dbase record ");
            sb.Append(fileProblem.GetParameterValue("RecordNumber"));

            identifierField ??= fileProblem.GetParameterValue("IdentifierField", required: false);
            identifierValue ??= fileProblem.GetParameterValue("IdentifierValue", required: false);
            if (!string.IsNullOrEmpty(identifierField) && !string.IsNullOrEmpty(identifierValue))
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
