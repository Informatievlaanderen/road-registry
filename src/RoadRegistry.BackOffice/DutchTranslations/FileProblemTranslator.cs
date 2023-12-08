namespace RoadRegistry.BackOffice.DutchTranslations;

using Core;
using System;
using Uploads;
using Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using FileProblem = Messages.FileProblem;
using Problem = Messages.Problem;

public static class FileProblemTranslator
{
    public static readonly Converter<FileProblem, ProblemTranslation> Dutch = problem =>
    {
        var translation = new ProblemTranslation(problem.Severity, problem.Reason);

        return problem.Reason switch
        {
            nameof(ZipArchiveProblems.RequiredFileMissing) => translation with { Message = "Het bestand ontbreekt in het archief." },
            nameof(ProjectionFormatFileProblems.ProjectionFormatInvalid) => translation with { Message = "Projectie formaat is niet 'Belge_Lambert_1972'." },
            nameof(DbaseFileProblems.HasNoDbaseRecords) => translation with { Message = "Het bestand bevat geen rijen." },
            nameof(DbaseFileProblems.HasDbaseHeaderFormatError) => translation with { Message = "De hoofding van het bestand is niet correct geformateerd." },
            nameof(DbaseFileProblems.HasDbaseSchemaMismatch) => translation with { Message = $"Het verwachte dbase schema {problem.Parameters[0].Value} stemt niet overeen met het eigenlijke dbase schema {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.HasDbaseRecordFormatError) => translation with { Message = $"De dbase record na record {problem.Parameters[0].Value} is niet correct geformateerd." },
            nameof(DbaseFileProblems.IdentifierZero) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een identificator die 0 is." },
            nameof(DbaseFileProblems.RequiredFieldIsNull) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft geen waarde voor veld {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.IdentifierNotUnique) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat dezelfde identifier {problem.Parameters[1].Value} als dbase record {problem.Parameters[2].Value}." },
            nameof(DbaseFileProblems.IdentifierNotUniqueButAllowed) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} ({problem.Parameters[1].Value}) bevat dezelfde identifier {problem.Parameters[2].Value} als dbase record {problem.Parameters[3].Value} ({problem.Parameters[4].Value})." },
            nameof(DbaseFileProblems.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} (iWegsegment.dbf) bevat dezelfde identifier {problem.Parameters[1].Value} als dbase record {problem.Parameters[2].Value} (Wegsegment.dbf)." },
            nameof(DbaseFileProblems.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} (iWegknoop.dbf) bevat dezelfde identifier {problem.Parameters[1].Value} als dbase record {problem.Parameters[2].Value} (Wegknoop.dbf)." },
            nameof(DbaseFileProblems.RoadSegmentIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige wegsegment identificator: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadNodeIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige wegknoop identificator: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.NotNumberedRoadNumber) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen genummerd wegnummer is." },
            nameof(DbaseFileProblems.NotEuropeanRoadNumber) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen europees wegnummer is." },
            nameof(DbaseFileProblems.EuropeanRoadNotUnique) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} met EU_OIDN {problem.Parameters[1].Value} heeft hetzelfde WS_OIDN en EUNUMMER als de dbase record {problem.Parameters[2].Value} met EU_OIDN {problem.Parameters[3].Value}." },
            nameof(DbaseFileProblems.NotNationalRoadNumber) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen nationaal wegnummer is." },
            nameof(DbaseFileProblems.NationalRoadNotUnique) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} met NW_OIDN {problem.Parameters[1].Value} heeft hetzelfde WS_OIDN en IDENT2 als de dbase record {problem.Parameters[2].Value} met NW_OIDN {problem.Parameters[3].Value}." },
            nameof(DbaseFileProblems.FromPositionOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige van positie: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.ToPositionOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige tot positie: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.FromPositionEqualToOrGreaterThanToPosition) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een van positie ({problem.Parameters[1].Value}) die gelijk aan of groter dan de tot positie ({problem.Parameters[2].Value}) is." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionTypeMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type ongelijkgrondse kruising in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment) => translation with { Message = "Het onder- en bovenliggende wegsegment van een ongelijkgrondse kruising moeten van elkaar verschillen." },
            nameof(DbaseFileProblems.GradeSeparatedJunctionMissing) => translation with { Message = $"De wegsegmenten {problem.GetParameterValue("RoadSegmentId1")} en {problem.GetParameterValue("RoadSegmentId2")} snijden elkaar op de locatie [{problem.GetParameterValue("IntersectionX")} {problem.GetParameterValue("IntersectionY")}], maar er is geen ongelijkgrondse kruising met deze wegsegmenten als onder- en bovenliggend wegsegment." },
            nameof(DbaseFileProblems.ExpectedGradeSeparatedJunctionsCountDiffersFromActual) => translation with { Message = $"TODO" },
            nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige bovenliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.BO_WS_OIDN)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige onderliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.ON_WS_OIDN)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.NumberedRoadOrdinalOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldig volgnummer in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.VOLGNUMMER)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.NumberedRoadDirectionMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige richting in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.RICHTING)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadNodeTypeMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type wegknoop in veld {nameof(RoadNodeChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadNodeGeometryMissing) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} met WK_OIDN ({problem.Parameters[1].Value}) bevat geen geometrie." },
            nameof(DbaseFileProblems.RoadNodeIsAlreadyProcessed) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} verwijst naar een wegknoop via veld WK_OIDN ({problem.Parameters[1].Value}) dat te dicht bij het wegknoop met WK_OIDN ({problem.Parameters[2].Value}) ligt." },
            nameof(DbaseFileProblems.RoadSegmentAccessRestrictionMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.TGBEP)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadSegmentStatusMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige status in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.STATUS)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadSegmentMaintenanceAuthorityCodeNotValid) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegbeheerder in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.BEHEERDER)}: {problem.Parameters[2].Value}. De opgegeven wegbeheerdercode '{problem.GetParameterValue(MaintenanceAuthorityNotKnown.ParameterName.OrganizationId)}' komt niet overeen met een (OVO-code die correspondeert met een) code gekend door het Wegenregister." },
            nameof(DbaseFileProblems.RoadSegmentCategoryMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegcategorie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.CATEGORIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige methode in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.METHODE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadSegmentMorphologyMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige morfologie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.BeginRoadNodeIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige begin wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.EndRoadNodeIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige eind wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.LeftStreetNameIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige straat naam id in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.LSTRNMID)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RightStreetNameIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige straat naam id in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.RSTRNMID)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.BeginRoadNodeIdEqualsEndRoadNode) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} heeft een begin wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN)}: {problem.Parameters[1].Value} die gelijk is aan de eindwegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN)}: {problem.Parameters[2].Value}." },
            nameof(DbaseFileProblems.DownloadIdInvalidFormat) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een download id in veld {nameof(TransactionZoneDbaseRecord.Schema.DOWNLOADID)}: {problem.Parameters[1].Value} dat een ongeldig formaat heeft (verwacht formaat: {Guid.Empty:N})" },
            nameof(DbaseFileProblems.DownloadIdDiffersFromMetadata) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een download id in veld {nameof(TransactionZoneDbaseRecord.Schema.DOWNLOADID)}: {problem.Parameters[1].Value} dat niet overeen komt met het download id aangeleverd in de metadata: {problem.Parameters[2].Value}." },
            nameof(DbaseFileProblems.OrganizationIdOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig organisatie identificator in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.ExtractDescriptionOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige extractbeschrijving in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)}: {problem.Parameters[1].Value}." }, 
            nameof(DbaseFileProblems.OperatorNameOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige operator in veld {nameof(TransactionZoneDbaseRecord.Schema.ORG)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.LaneCountOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig rijstrook aantal in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.AANTAL)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.LaneDirectionMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige rijstrook richting in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.RICHTING)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.SurfaceTypeMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentSurfaceChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.WidthOutOfRange) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegbreedte in veld {nameof(RoadSegmentWidthChangeDbaseRecord.Schema.BREEDTE)}: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RecordTypeMismatch) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig record type in veld RECORDTYPE: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RecordTypeNotSupported) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} bevat een niet ondersteund record type in veld RECORDTYPE: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}." },
            nameof(DbaseFileProblems.RoadSegmentMissing) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} verwijst naar een wegsegment via veld WS_OIDN ({problem.Parameters[1].Value}) dat niet in WEGSEGMENT.DBF kon worden teruggevonden." },
            nameof(DbaseFileProblems.RoadSegmentIsAlreadyProcessed) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} verwijst naar een wegsegment via veld WS_OIDN ({problem.Parameters[1].Value}) dat te dicht bij het wegsegment met WS_OIDN ({problem.Parameters[2].Value}) ligt." },
            nameof(DbaseFileProblems.RoadSegmentGeometryMissing) => translation with { Message = $"De dbase record {problem.Parameters[0].Value} met WS_OIDN ({problem.Parameters[1].Value}) bevat geen geometrie." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutLaneAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit WEGSEGMENTEN.DBF werd geen enkel rijstroken attribuut teruggevonden: {problem.Parameters[0].Value}." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutWidthAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit WEGSEGMENTEN.DBF werd geen enkel wegbreedte attribuut teruggevonden: {problem.Parameters[0].Value}." },
            nameof(DbaseFileProblems.RoadSegmentsWithoutSurfaceAttributes) => translation with { Message = $"Voor de volgende wegsegmenten uit WEGSEGMENTEN.DBF werd geen enkel wegverharding attribuut teruggevonden: {problem.Parameters[0].Value}." },
            nameof(ShapeFileProblems.HasNoShapeRecords) => translation with { Message = "Het bestand bevat geen enkele geometrie." },
            nameof(ShapeFileProblems.ShapeHeaderFormatError) => translation with { Message = "De hoofding van het bestand is niet correct geformateerd." },
            nameof(ShapeFileProblems.HasShapeRecordFormatError) => translation with { Message = $"De shape record na record {problem.Parameters[0].Value} is niet correct geformateerd." },
            nameof(ShapeFileProblems.DbaseRecordMissing) => translation with { Message = $"De shape record {problem.Parameters[0].Value} bevat geen dbase record." },
            nameof(ShapeFileProblems.ShapeRecordShapeTypeMismatch) => translation with { Message = $"De shape record {problem.Parameters[0].Value} bevat geen {problem.Parameters[1].Value} maar een {problem.Parameters[2].Value}." },
            nameof(ShapeFileProblems.ShapeRecordShapeGeometryTypeMismatch) => translation with { Message = $"De shape record {problem.Parameters[0].Value} bevat geen {problem.Parameters[1].Value} maar een {problem.Parameters[2].Value}." },
            nameof(ShapeFileProblems.ShapeRecordGeometryMismatch) => translation with { Message = $"De shape record {problem.Parameters[0].Value} geometrie is ongeldig." },
            nameof(ShapeFileProblems.ShapeRecordGeometryLineCountMismatch) => translation with { Message = $"De shape record {problem.Parameters[0].Value} geometrie heeft meer lijnen dan verwacht." },
            nameof(ShapeFileProblems.ShapeRecordGeometrySelfOverlaps) => translation with { Message = $"De shape record {problem.Parameters[0].Value} geometrie overlapt zichzelf." },
            nameof(ShapeFileProblems.ShapeRecordGeometrySelfIntersects) => translation with { Message = $"De shape record {problem.Parameters[0].Value} geometrie kruist zichzelf." },
            nameof(ShapeFileProblems.ShapeRecordGeometryHasInvalidMeasureOrdinates) => translation with { Message = $"De shape record {problem.Parameters[0].Value} geometrie bevat ongeldige measure waarden." },
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
