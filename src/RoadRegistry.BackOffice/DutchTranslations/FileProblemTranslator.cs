namespace RoadRegistry.BackOffice.DutchTranslations;

using Core;
using Extracts;
using FeatureCompare;
using System;
using System.Text;
using Uploads;
using Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using FileProblem = Messages.FileProblem;
using Problem = Messages.Problem;

public static class FileProblemTranslator
{
    public static readonly Converter<FileProblem, ProblemTranslation> Dutch = problem =>
    {
        var translation = new ProblemTranslation(problem.Severity, problem.Reason);

        string DbaseRecordIdentifier()
        {
            var sb = new StringBuilder();
            sb.Append(problem.GetParameterValue("RecordNumber"));

            var identifierField = problem.GetParameterValue("IdentifierField");
            var identifierValue = problem.GetParameterValue("IdentifierValue");
            if (!string.IsNullOrEmpty(identifierField) && !string.IsNullOrEmpty(identifierValue))
            {
                sb.Append(" met ");
                sb.Append(identifierField);
                sb.Append(" (");
                sb.Append(identifierValue);
                sb.Append(")");
            }

            return sb.ToString();
        }

        return problem.Reason switch
        {
            nameof(ZipArchiveProblems.RequiredFileMissing) => translation with { Message = "Het bestand ontbreekt in het archief." },
            nameof(ProjectionFormatFileProblems.ProjectionFormatInvalid) => translation with { Message = "Projectie formaat is niet 'Belge_Lambert_1972'." },
            nameof(DbaseFileProblems.HasNoDbaseRecords) => translation with { Message = "Het bestand bevat geen rijen." },
            nameof(DbaseFileProblems.HasDbaseHeaderFormatError) => translation with { Message = "De hoofding van het bestand is niet correct geformateerd." },
            nameof(DbaseFileProblems.HasDbaseSchemaMismatch) => translation with { Message = $"Het verwachte dbase schema ({problem.GetParameterValue("ExpectedSchema")}) stemt niet overeen met het eigenlijke dbase schema ({problem.GetParameterValue("ActualSchema")})." },
            nameof(DbaseFileProblems.HasDbaseRecordFormatError) => translation with { Message = $"De dbase record na record {problem.GetParameterValue("RecordNumber")} is niet correct geformateerd." },
            nameof(DbaseFileProblems.IdentifierZero) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} bevat een identificator die 0 is." },
            nameof(DbaseFileProblems.RequiredFieldIsNull) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} heeft geen waarde voor veld {problem.GetParameterValue("Field")}." },
            nameof(DbaseFileProblems.IdentifierNotUnique) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} bevat dezelfde identifier ({problem.GetParameterValue("Identifier")}) als dbase record {problem.GetParameterValue("TakenByRecordNumber")}." },
            nameof(DbaseFileProblems.IdentifierNotUniqueButAllowed) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} ({problem.GetParameterValue("RecordType")}) bevat dezelfde identifier ({problem.GetParameterValue("Identifier")}) als dbase record {problem.GetParameterValue("TakenByRecordNumber")} ({problem.GetParameterValue("TakenByRecordType")})." },
            nameof(DbaseFileProblems.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} ({FeatureType.Integration.ToDbaseFileName(ExtractFileName.Wegsegment)}) bevat dezelfde identifier ({problem.GetParameterValue("Identifier")}) als dbase record {problem.GetParameterValue("TakenByRecordNumber")} ({ExtractFileName.Wegsegment.ToDbaseFileName()})." },
            nameof(DbaseFileProblems.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} ({FeatureType.Integration.ToDbaseFileName(ExtractFileName.Wegknoop)}) bevat dezelfde identifier ({problem.GetParameterValue("Identifier")}) als dbase record {problem.GetParameterValue("TakenByRecordNumber")} ({ExtractFileName.Wegknoop.ToDbaseFileName()})." },
            nameof(DbaseFileProblems.RoadSegmentIdOutOfRange) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} heeft een ongeldige wegsegment identificator ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.RoadNodeIdOutOfRange) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} heeft een ongeldige wegknoop identificator ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.NotNumberedRoadNumber) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een nummer ({problem.GetParameterValue("Number")}) dat geen genummerd wegnummer is." },
            nameof(DbaseFileProblems.NotEuropeanRoadNumber) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een nummer ({problem.GetParameterValue("Number")}) dat geen europees wegnummer is." },
            nameof(DbaseFileProblems.EuropeanRoadNotUnique) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} met EU_OIDN ({problem.GetParameterValue("AttributeId")}) heeft hetzelfde WS_OIDN en EUNUMMER als de dbase record {problem.GetParameterValue("TakenByRecordNumber")} met EU_OIDN ({problem.GetParameterValue("TakenByAttributeId")})." },
            nameof(DbaseFileProblems.NotNationalRoadNumber) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een nummer ({problem.GetParameterValue("Number")}) dat geen nationaal wegnummer is." },
            nameof(DbaseFileProblems.NationalRoadNotUnique) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} met NW_OIDN ({problem.GetParameterValue("AttributeId")}) heeft hetzelfde WS_OIDN en IDENT2 als de dbase record {problem.GetParameterValue("TakenByRecordNumber")} met NW_OIDN ({problem.GetParameterValue("TakenByAttributeId")})." },
            nameof(DbaseFileProblems.FromPositionOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige van positie ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.ToPositionOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige tot positie ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.FromPositionEqualToOrGreaterThanToPosition) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een van positie ({problem.GetParameterValue("From")}) die gelijk aan of groter dan de tot positie ({problem.GetParameterValue("To")}) is." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionTypeMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldig type ongelijkgrondse kruising in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.TYPE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een onderliggende wegsegment dat hetzelfde is aan het bovenliggende wegsegment." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionMissing) => translation with { Message = $"De wegsegmenten {problem.GetParameterValue("RoadSegmentId1")} en {problem.GetParameterValue("RoadSegmentId2")} snijden elkaar op de locatie ({problem.GetParameterValue("IntersectionX")} {problem.GetParameterValue("IntersectionY")}), maar er is geen ongelijkgrondse kruising met deze wegsegmenten als onder- en bovenliggend wegsegment." },
            nameof(DbaseFileProblems.ExpectedGradeSeparatedJunctionsCountDiffersFromActual) => translation with { Message = "TODO" },
            nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldige bovenliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.BO_WS_OIDN)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldige onderliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.ON_WS_OIDN)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.NumberedRoadOrdinalOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldig volgnummer in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.VOLGNUMMER)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.NumberedRoadDirectionMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige richting in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.RICHTING)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadNodeTypeMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldig type wegknoop in veld {nameof(RoadNodeChangeDbaseRecord.Schema.TYPE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadNodeGeometryMissing) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} met WK_OIDN ({problem.GetParameterValue("Actual")}) bevat geen geometrie." },
            nameof(DbaseFileProblems.RoadNodeIsAlreadyProcessed) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} met WK_OIDN ({problem.GetParameterValue("Identifier")}) ligt te dicht bij het wegknoop met WK_OIDN ({problem.GetParameterValue("ProcessedId")})." },
            nameof(DbaseFileProblems.RoadSegmentAccessRestrictionMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.TGBEP)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentStatusMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige status in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.STATUS)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMaintenanceAuthorityNotKnown) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige wegbeheerder in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.BEHEERDER)}. De opgegeven wegbeheerdercode ({problem.GetParameterValue(MaintenanceAuthorityNotKnown.ParameterName.OrganizationId)}) komt niet overeen met een (OVO-code die correspondeert met een) code gekend door het Wegenregister." },
            nameof(DbaseFileProblems.RoadSegmentCategoryMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige wegcategorie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.CATEGORIE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige methode in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.METHODE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMorphologyMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige morfologie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.BeginRoadNodeIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldige begin wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.EndRoadNodeIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldige eind wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LeftStreetNameIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldige straat naam id in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.LSTRNMID)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.RightStreetNameIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een ongeldige straat naam id in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.RSTRNMID)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.BeginRoadNodeIdEqualsEndRoadNode) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} heeft een begin wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN)} ({problem.GetParameterValue("Begin")}) die gelijk is aan de eindwegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN)} ({problem.GetParameterValue("End")})." },
            nameof(DbaseFileProblems.DownloadIdInvalidFormat) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een download id in veld {nameof(TransactionZoneDbaseRecord.Schema.DOWNLOADID)} ({problem.GetParameterValue("Actual")}) dat een ongeldig formaat heeft (verwacht formaat: {Guid.Empty:N})" },
            nameof(DbaseFileProblems.DownloadIdDiffersFromMetadata) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een download id in veld {nameof(TransactionZoneDbaseRecord.Schema.DOWNLOADID)} ({problem.GetParameterValue("Actual")}) die niet overeen komt met het download id aangeleverd in de metadata ({problem.GetParameterValue("Expected")})." },
            nameof(DbaseFileProblems.OrganizationIdOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldig organisatie identificator in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.ExtractDescriptionOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige extractbeschrijving in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)} ({problem.GetParameterValue("Actual")})." }, 
            nameof(DbaseFileProblems.OperatorNameOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige operator in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LaneCountOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldig rijstrook aantal in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.AANTAL)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.LaneDirectionMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige rijstrook richting in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.RICHTING)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.SurfaceTypeMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentSurfaceChangeDbaseRecord.Schema.TYPE)} ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.WidthOutOfRange) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldige wegbreedte in veld {nameof(RoadSegmentWidthChangeDbaseRecord.Schema.BREEDTE)} ({problem.GetParameterValue("Actual")})." },
            nameof(DbaseFileProblems.RecordTypeMismatch) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een ongeldig record type in veld RECORDTYPE ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RecordTypeNotSupported) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} bevat een niet ondersteund record type in veld RECORDTYPE ({problem.GetParameterValue("Actual")}). Verwachte 1 van volgende waarden: {problem.GetParameterValue("ExpectedOneOf")}." },
            nameof(DbaseFileProblems.RoadSegmentMissing) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} verwijst naar een wegsegment via veld WS_OIDN ({problem.GetParameterValue("Actual")}) dat niet in {ExtractFileName.Wegsegment.ToDbaseFileName()} kon worden teruggevonden." },
            nameof(DbaseFileProblems.RoadSegmentIsAlreadyProcessed) => translation with { Message = $"De dbase record {problem.GetParameterValue("RecordNumber")} met WS_OIDN ({problem.GetParameterValue("Identifier")}) ligt te dicht bij het wegsegment met WS_OIDN ({problem.GetParameterValue("ProcessedId")})." },
            nameof(DbaseFileProblems.RoadSegmentGeometryMissing) => translation with { Message = $"De dbase record {DbaseRecordIdentifier()} met WS_OIDN ({problem.GetParameterValue("Actual")}) bevat geen geometrie." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutLaneAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit {ExtractFileName.Wegsegment.ToDbaseFileName()} werd geen enkel rijstroken attribuut teruggevonden: {problem.GetParameterValue("Segments")}." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutWidthAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit {ExtractFileName.Wegsegment.ToDbaseFileName()} werd geen enkel wegbreedte attribuut teruggevonden: {problem.GetParameterValue("Segments")}." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutSurfaceAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit {ExtractFileName.Wegsegment.ToDbaseFileName()} werd geen enkel wegverharding attribuut teruggevonden: {problem.GetParameterValue("Segments")}." },
            nameof(ShapeFileProblems.HasNoShapeRecords) => translation with { Message = "Het bestand bevat geen enkele geometrie." },
            nameof(ShapeFileProblems.ShapeHeaderFormatError) => translation with { Message = "De hoofding van het bestand is niet correct geformateerd." },
            nameof(ShapeFileProblems.HasShapeRecordFormatError) => translation with { Message = $"De shape record na record {problem.GetParameterValue("Exception")} is niet correct geformateerd." },
            nameof(ShapeFileProblems.DbaseRecordMissing) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} bevat geen dbase record." },
            nameof(ShapeFileProblems.ShapeRecordShapeTypeMismatch) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} bevat geen {problem.GetParameterValue("ExpectedShapeType")} maar een {problem.GetParameterValue("ActualShapeType")}." },
            nameof(ShapeFileProblems.ShapeRecordShapeGeometryTypeMismatch) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} bevat geen {problem.GetParameterValue("ExpectedShapeType")} maar een {problem.GetParameterValue("ActualShapeType")}." },
            nameof(ShapeFileProblems.ShapeRecordGeometryMismatch) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie is ongeldig." },
            nameof(ShapeFileProblems.ShapeRecordGeometryLineCountMismatch) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie heeft meer lijnen dan verwacht." },
            nameof(ShapeFileProblems.ShapeRecordGeometrySelfOverlaps) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie overlapt zichzelf." },
            nameof(ShapeFileProblems.ShapeRecordGeometrySelfIntersects) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie kruist zichzelf." },
            nameof(ShapeFileProblems.ShapeRecordGeometryHasInvalidMeasureOrdinates) => translation with { Message = $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie bevat ongeldige measure waarden." },
            _ => translation with
            {
                Message = ProblemTranslator.Dutch(new Problem
                {
                    Reason = problem.Reason,
                    Parameters = problem.Parameters,
                    Severity = problem.Severity
                }).Message
            }
        };
    };
}
