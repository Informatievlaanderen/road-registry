namespace RoadRegistry.Extracts.DutchTranslations;

using System.Text;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.Messages;
using Schemas.ExtractV1;
using Schemas.ExtractV1.GradeSeparatedJuntions;
using Schemas.ExtractV1.RoadNodes;
using Schemas.ExtractV1.RoadSegments;
using Uploads;
using ValueObjects.Problems;
using FileProblem = Messages.FileProblem;

public sealed class DefaultFileProblemTranslator : FileProblemTranslator
{
    public DefaultFileProblemTranslator()
        : base(WellKnownProblemTranslators.Default)
    {
    }

    protected override ProblemTranslation? InnerTranslate(FileProblem fileProblem)
    {
        var translation = new ProblemTranslation(fileProblem.Severity, fileProblem.Reason);

        return fileProblem.Reason switch
        {
            nameof(ZipArchiveProblems.RequiredFileMissing) => translation with { Message = "Het bestand ontbreekt in het archief." },
            nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert72) => translation with { Message = "Projectie formaat is niet 'Belge_Lambert_1972'." },
            nameof(DbaseFileProblems.HasNoDbaseRecords) => translation with { Message = "Het bestand bevat geen rijen." },
            nameof(DbaseFileProblems.HasDbaseHeaderFormatError) => translation with { Message = "De hoofding van het bestand is niet correct geformateerd." },
            nameof(DbaseFileProblems.HasDbaseSchemaMismatch) => translation with { Message = $"Het verwachte dbase schema ({fileProblem.GetParameterValue("ExpectedSchema")}) stemt niet overeen met het eigenlijke dbase schema ({fileProblem.GetParameterValue("ActualSchema")})." },
            nameof(DbaseFileProblems.HasDbaseRecordFormatError) => translation with { Message = $"De dbase record na record {fileProblem.GetParameterValue("RecordNumber")} is niet correct geformateerd." },
            nameof(DbaseFileProblems.IdentifierZero) => translation with { Message = $"De dbase record {fileProblem.GetParameterValue("RecordNumber")} bevat een identificator die 0 is." },
            nameof(DbaseFileProblems.RequiredFieldIsNull) => translation with { Message = $"De dbase record {fileProblem.GetParameterValue("RecordNumber")} heeft geen waarde voor veld {fileProblem.GetParameterValue("Field")}." },
            nameof(DbaseFileProblems.IdentifierNotUnique) => translation with { Message = $"De dbase record {fileProblem.GetParameterValue("RecordNumber")} bevat dezelfde {fileProblem.GetParameterValue("IdentifierField", required: false) ?? "identifier"} {fileProblem.GetParameterValue("Identifier")} als dbase record {fileProblem.GetParameterValue("TakenByRecordNumber")}." },
            nameof(DbaseFileProblems.IdentifierNotUniqueButAllowed) => translation with { Message = $"De dbase record {fileProblem.GetParameterValue("RecordNumber")} ({fileProblem.GetParameterValue("RecordType")}) bevat dezelfde identifier {fileProblem.GetParameterValue("Identifier")} als dbase record {fileProblem.GetParameterValue("TakenByRecordNumber")} ({fileProblem.GetParameterValue("TakenByRecordType")})." },
            nameof(DbaseFileProblems.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange) => translation with { Message = $"De {DbaseRecordLabel("WS_OIDN", fileProblem.GetParameterValue("Identifier"))} ({FeatureType.Integration.ToDbaseFileName(ExtractFileName.Wegsegment)}) bevat dezelfde identifier als dbase record {fileProblem.GetParameterValue("TakenByRecordNumber")} ({ExtractFileName.Wegsegment.ToDbaseFileName()})." },
            nameof(DbaseFileProblems.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange) => translation with { Message = $"De {DbaseRecordLabel("WK_OIDN", fileProblem.GetParameterValue("Identifier"))} ({FeatureType.Integration.ToDbaseFileName(ExtractFileName.Wegknoop)}) bevat dezelfde identifier als dbase record {fileProblem.GetParameterValue("TakenByRecordNumber")} ({ExtractFileName.Wegknoop.ToDbaseFileName()})." },
            nameof(DbaseFileProblems.RoadSegmentIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel("WS_OIDN", fileProblem.GetParameterValue("Actual"))} heeft een ongeldige wegsegment identificator." },
            nameof(DbaseFileProblems.RoadNodeIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel("WK_OIDN", fileProblem.GetParameterValue("Actual"))} heeft een ongeldige wegknoop identificator." },
            nameof(DbaseFileProblems.NotNumberedRoadNumber) => translation with { Message = $"De {DbaseRecordLabel()} bevat een nummer ({fileProblem.GetParameterValue("Number")}) dat geen genummerd wegnummer is." },
            nameof(DbaseFileProblems.NotEuropeanRoadNumber) => translation with { Message = $"De {DbaseRecordLabel()} bevat een nummer ({fileProblem.GetParameterValue("Number")}) dat geen europees wegnummer is." },
            nameof(DbaseFileProblems.EuropeanRoadNotUnique) => translation with { Message = $"De {DbaseRecordLabel("EU_OIDN", fileProblem.GetParameterValue("AttributeId"))} heeft hetzelfde {fileProblem.GetParameterValue("RoadSegmentIdentifierField") ?? "WS_OIDN"} en {fileProblem.GetParameterValue("NumberField") ?? "EUNUMMER"} als de dbase record {fileProblem.GetParameterValue("TakenByRecordNumber")} met EU_OIDN {fileProblem.GetParameterValue("TakenByAttributeId")}." },
            nameof(DbaseFileProblems.NotNationalRoadNumber) => translation with { Message = $"De {DbaseRecordLabel()} bevat een nummer ({fileProblem.GetParameterValue("Number")}) dat geen nationaal wegnummer is." },
            nameof(DbaseFileProblems.NationalRoadNotUnique) => translation with { Message = $"De {DbaseRecordLabel("NW_OIDN", fileProblem.GetParameterValue("AttributeId"))} heeft hetzelfde {fileProblem.GetParameterValue("RoadSegmentIdentifierField") ?? "WS_OIDN"} en {fileProblem.GetParameterValue("NumberField") ?? "IDENT2"} als de dbase record {fileProblem.GetParameterValue("TakenByRecordNumber")} met NW_OIDN {fileProblem.GetParameterValue("TakenByAttributeId")}." },
            nameof(DbaseFileProblems.FromPositionOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige van positie ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.ToPositionOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige tot positie ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.FromPositionEqualToOrGreaterThanToPosition) => translation with { Message = $"De {DbaseRecordLabel()} heeft een van positie ({fileProblem.GetParameterValue("From")}) die gelijk aan of groter dan de tot positie ({fileProblem.GetParameterValue("To")}) is." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionTypeMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig type ongelijkgrondse kruising in veld {nameof(GradeSeparatedJunctionDbaseRecord.Schema.TYPE)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment) => translation with { Message = $"De {DbaseRecordLabel()} heeft een onderliggende wegsegment dat hetzelfde is aan het bovenliggende wegsegment." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionMissing) => translation with { Message = $"De wegsegmenten {fileProblem.GetParameterValue("RoadSegmentId1")} en {fileProblem.GetParameterValue("RoadSegmentId2")} snijden elkaar op de locatie ({fileProblem.GetParameterValue("IntersectionX")} {fileProblem.GetParameterValue("IntersectionY")}), maar er is geen ongelijkgrondse kruising met deze wegsegmenten als onder- en bovenliggend wegsegment." },
            nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} heeft een ongeldige bovenliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionDbaseRecord.Schema.BO_WS_OIDN)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} heeft een ongeldige onderliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionDbaseRecord.Schema.ON_WS_OIDN)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.NumberedRoadOrdinalOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} heeft een ongeldig volgnummer in veld {nameof(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema.VOLGNUMMER)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.NumberedRoadDirectionMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige richting in veld {nameof(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema.RICHTING)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadNodeTypeMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig type wegknoop in veld {nameof(RoadNodeDbaseRecord.Schema.TYPE)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadNodeGeometryMissing) => translation with { Message = $"De {DbaseRecordLabel()} met WK_OIDN {fileProblem.GetParameterValue("Actual")} bevat geen geometrie." },
            nameof(DbaseFileProblems.RoadNodeIsAlreadyProcessed) => translation with { Message = $"De {DbaseRecordLabel("WK_OIDN", fileProblem.GetParameterValue("Identifier"))} ligt te dicht bij het wegknoop met WK_OIDN {fileProblem.GetParameterValue("ProcessedId")}." },
            nameof(DbaseFileProblems.RoadSegmentAccessRestrictionMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentDbaseRecord.Schema.TGBEP)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentStatusMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige status in veld {nameof(RoadSegmentDbaseRecord.Schema.STATUS)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMaintenanceAuthorityNotKnown) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige wegbeheerder in veld {nameof(RoadSegmentDbaseRecord.Schema.BEHEER)}. De opgegeven wegbeheerdercode ({fileProblem.GetParameterValue(MaintenanceAuthorityNotKnown.ParameterName.OrganizationId)}) komt niet overeen met een (OVO-code die correspondeert met een) code gekend door het Wegenregister." },
            nameof(DbaseFileProblems.RoadSegmentMaintenanceAuthorityOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig organisatie identificator in veld {nameof(RoadSegmentDbaseRecord.Schema.BEHEER)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.RoadSegmentCategoryMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige wegcategorie in veld {nameof(RoadSegmentDbaseRecord.Schema.WEGCAT)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige methode in veld {nameof(RoadSegmentDbaseRecord.Schema.METHODE)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMorphologyMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige morfologie in veld {nameof(RoadSegmentDbaseRecord.Schema.MORF)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.BeginRoadNodeIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} heeft een ongeldige begin wegknoop in veld {nameof(RoadSegmentDbaseRecord.Schema.B_WK_OIDN)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.EndRoadNodeIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} heeft een ongeldige eind wegknoop in veld {nameof(RoadSegmentDbaseRecord.Schema.E_WK_OIDN)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LeftStreetNameIdOutOfRange) => translation with
            {
                Message = $"De {DbaseRecordLabel()} heeft een ongeldige straatnaam id in veld {nameof(RoadSegmentDbaseRecord.Schema.LSTRNMID)} ({fileProblem.GetParameterValue("Actual")})."
            },
            nameof(DbaseFileProblems.LeftStreetNameIdIsRemoved) => translation with
            {
                Message = $"De {DbaseRecordLabel()} heeft een ongeldige straatnaam id in veld {nameof(RoadSegmentDbaseRecord.Schema.LSTRNMID)} ({fileProblem.GetParameterValue("Actual")}). Deze verwijst naar een straatnaam dat niet meer bestaat."
            },
            nameof(DbaseFileProblems.LeftStreetNameIdIsRenamed) => translation with
            {
                Message = $"De {DbaseRecordLabel()} heeft een ongeldige straatnaam id in veld {nameof(RoadSegmentDbaseRecord.Schema.LSTRNMID)} ({fileProblem.GetParameterValue("Actual")}). Deze is hernoemt naar {fileProblem.GetParameterValue("DestinationValue")}."
            },
            nameof(DbaseFileProblems.RightStreetNameIdOutOfRange) => translation with
            {
                Message = $"De {DbaseRecordLabel()} heeft een ongeldige straatnaam id in veld {nameof(RoadSegmentDbaseRecord.Schema.RSTRNMID)} ({fileProblem.GetParameterValue("Actual")})."
            },
            nameof(DbaseFileProblems.RightStreetNameIdIsRemoved) => translation with
            {
                Message = $"De {DbaseRecordLabel()} heeft een ongeldige straatnaam id in veld {nameof(RoadSegmentDbaseRecord.Schema.RSTRNMID)} ({fileProblem.GetParameterValue("Actual")}). Deze verwijst naar een straatnaam dat niet meer bestaat."
            },
            nameof(DbaseFileProblems.RightStreetNameIdIsRenamed) => translation with
            {
                Message = $"De {DbaseRecordLabel()} heeft een ongeldige straatnaam id in veld {nameof(RoadSegmentDbaseRecord.Schema.RSTRNMID)} ({fileProblem.GetParameterValue("Actual")}). Deze is hernoemt naar {fileProblem.GetParameterValue("DestinationValue")}."
            },
            nameof(DbaseFileProblems.BeginRoadNodeIdEqualsEndRoadNode) => translation with { Message = $"De {DbaseRecordLabel()} heeft een begin wegknoop in veld {nameof(RoadSegmentDbaseRecord.Schema.B_WK_OIDN)} ({fileProblem.GetParameterValue("Begin")}) die gelijk is aan de eindwegknoop in veld {nameof(RoadSegmentDbaseRecord.Schema.E_WK_OIDN)} ({fileProblem.GetParameterValue("End")})." },
            nameof(DbaseFileProblems.DownloadIdInvalidFormat) => translation with { Message = $"De {DbaseRecordLabel()} bevat een download id in veld {nameof(TransactionZoneDbaseRecord.Schema.DOWNLOADID)} ({fileProblem.GetParameterValue("Actual")}) dat een ongeldig formaat heeft (verwacht formaat: {Guid.Empty:N})" },
            nameof(DbaseFileProblems.DownloadIdDiffersFromMetadata) => translation with { Message = $"De {DbaseRecordLabel()} bevat een download id in veld {nameof(TransactionZoneDbaseRecord.Schema.DOWNLOADID)} ({fileProblem.GetParameterValue("Actual")}) die niet overeen komt met het download id aangeleverd in de metadata ({fileProblem.GetParameterValue("Expected")})." },
            nameof(DbaseFileProblems.OrganizationIdOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig organisatie identificator in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.ExtractDescriptionOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige extractbeschrijving in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.OperatorNameOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige operator in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LaneCountOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig rijstrook aantal in veld {nameof(RoadSegmentLaneAttributeDbaseRecord.Schema.AANTAL)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LaneDirectionMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige rijstrook richting in veld {nameof(RoadSegmentLaneAttributeDbaseRecord.Schema.RICHTING)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.SurfaceTypeMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentSurfaceAttributeDbaseRecord.Schema.TYPE)} ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.WidthOutOfRange) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldige wegbreedte in veld {nameof(RoadSegmentWidthAttributeDbaseRecord.Schema.BREEDTE)} ({fileProblem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.RecordTypeMismatch) => translation with { Message = $"De {DbaseRecordLabel()} bevat een ongeldig record type in veld RECORDTYPE ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RecordTypeNotSupported) => translation with { Message = $"De {DbaseRecordLabel()} bevat een niet ondersteund record type in veld RECORDTYPE ({fileProblem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {fileProblem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMissing) => translation with { Message = $"De {DbaseRecordLabel()} verwijst naar een wegsegment via veld WS_OIDN {fileProblem.GetParameterValue("Actual")} dat niet in {ExtractFileName.Wegsegment.ToDbaseFileName()} kon worden teruggevonden." },
            nameof(DbaseFileProblems.RoadSegmentIsAlreadyProcessed) => translation with { Message = $"De {DbaseRecordLabel("WS_OIDN", fileProblem.GetParameterValue("Identifier"))} ligt te dicht bij het wegsegment met WS_OIDN {fileProblem.GetParameterValue("ProcessedId")}." },
            nameof(DbaseFileProblems.RoadSegmentGeometryMissing) => translation with { Message = $"De {DbaseRecordLabel("WS_OIDN", fileProblem.GetParameterValue("Actual"))} bevat geen geometrie." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutLaneAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit {ExtractFileName.Wegsegment.ToDbaseFileName()} werd geen enkel rijstroken attribuut teruggevonden: {fileProblem.GetParameterValue("Segments")}." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutWidthAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit {ExtractFileName.Wegsegment.ToDbaseFileName()} werd geen enkel wegbreedte attribuut teruggevonden: {fileProblem.GetParameterValue("Segments")}." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutSurfaceAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit {ExtractFileName.Wegsegment.ToDbaseFileName()} werd geen enkel wegverharding attribuut teruggevonden: {fileProblem.GetParameterValue("Segments")}." },
            nameof(ShapeFileProblems.HasNoShapeRecords) => translation with { Message = "Het bestand bevat geen enkele geometrie." },
            nameof(ShapeFileProblems.ShapeHeaderFormatError) => translation with { Message = "De hoofding van het bestand is niet correct geformateerd." },
            nameof(ShapeFileProblems.HasShapeRecordFormatError) => translation with { Message = $"De shape record na record {fileProblem.GetParameterValue("Exception")} is niet correct geformateerd." },
            nameof(ShapeFileProblems.DbaseRecordMissing) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} bevat geen dbase record." },
            nameof(ShapeFileProblems.ShapeRecordShapeTypeMismatch) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} bevat geen {fileProblem.GetParameterValue("ExpectedShapeType")} maar een {fileProblem.GetParameterValue("ActualShapeType")}." },
            nameof(ShapeFileProblems.ShapeRecordShapeGeometryTypeMismatch) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} bevat geen {fileProblem.GetParameterValue("ExpectedShapeType")} maar een {fileProblem.GetParameterValue("ActualShapeType")}." },
            nameof(ShapeFileProblems.ShapeRecordGeometryMismatch) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} geometrie is ongeldig." },
            nameof(ShapeFileProblems.ShapeRecordGeometryLineCountMismatch) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} geometrie heeft meer lijnen dan verwacht." },
            nameof(ShapeFileProblems.ShapeRecordGeometrySelfOverlaps) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} geometrie overlapt zichzelf." },
            nameof(ShapeFileProblems.ShapeRecordGeometrySelfIntersects) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} geometrie kruist zichzelf." },
            nameof(ShapeFileProblems.ShapeRecordGeometryHasInvalidMeasureOrdinates) => translation with { Message = $"De shape record {fileProblem.GetParameterValue("RecordNumber")} geometrie bevat ongeldige measure waarden." },
            _ => null
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
