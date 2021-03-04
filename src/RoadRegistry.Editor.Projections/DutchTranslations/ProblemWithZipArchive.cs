namespace RoadRegistry.Editor.Projections.DutchTranslations
{
    using System;
    using BackOffice.Uploads;
    using BackOffice.Uploads.Schema;

    public static class ProblemWithZipArchive
    {
        public static readonly Converter<BackOffice.Messages.FileProblem, string> Translator =
            problem =>
            {
                string translation;
                switch (problem.Reason)
                {
                    case nameof(ZipArchiveProblems.RequiredFileMissing):
                        translation = "Het bestand ontbreekt in het archief.";
                        break;

                    case nameof(DbaseFileProblems.HasNoDbaseRecords):
                        translation = "Het bestand bevat geen rijen.";
                        break;

                    case nameof(DbaseFileProblems.HasDbaseHeaderFormatError):
                        translation = "De hoofding van het bestand is niet correct geformateerd.";
                        break;

                    case nameof(DbaseFileProblems.HasDbaseSchemaMismatch):
                        translation =
                            $"Het verwachte dbase schema {problem.Parameters[0].Value} stemt niet overeen met het eigenlijke dbase schema {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.HasDbaseRecordFormatError):
                        translation =
                            $"De dbase record na record {problem.Parameters[0].Value} is niet correct geformateerd.";
                        break;

                    case nameof(DbaseFileProblems.IdentifierZero):
                        translation = $"De dbase record {problem.Parameters[0].Value} bevat een identificator die 0 is.";
                        break;

                    case nameof(DbaseFileProblems.RequiredFieldIsNull):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft geen waarde voor veld {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.IdentifierNotUnique):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat dezelfde identifier {problem.Parameters[1].Value} als dbase record {problem.Parameters[2].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige wegsegment identificator: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.NotNumberedRoadNumber):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen genummerd wegnummer is.";
                        break;

                    case nameof(DbaseFileProblems.NotEuropeanRoadNumber):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen europees wegnummer is.";
                        break;

                    case nameof(DbaseFileProblems.NotNationalRoadNumber):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen nationaal wegnummer is.";
                        break;

                    case nameof(DbaseFileProblems.FromPositionOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige van positie: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.ToPositionOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige tot positie: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.GradeSeparatedJunctionTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type ongelijkgrondse kruising in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige bovenliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.BO_WS_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige onderliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.ON_WS_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.NumberedRoadOrdinalOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldig volgnummer in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.VOLGNUMMER)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.NumberedRoadDirectionMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige richting in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.RICHTING)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadNodeTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type wegknoop in veld {nameof(RoadNodeChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentAccessRestrictionMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.TGBEP)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentStatusMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige status in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.STATUS)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentCategoryMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegcategorie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.CATEGORIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige methode in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.METHODE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentMorphologyMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige morfologie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.BeginRoadNodeIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige begin wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.EndRoadNodeIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige eind wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.LaneCountOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig rijstrook aantal in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.AANTAL)}: {problem.Parameters[2].Value}.";
                        break;

                    case nameof(DbaseFileProblems.LaneDirectionMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige rijstrook richting in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.RICHTING)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.SurfaceTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentSurfaceChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.WidthOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegbreedte in veld {nameof(RoadSegmentWidthChangeDbaseRecord.Schema.BREEDTE)}: {problem.Parameters[2].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RecordTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig record type in veld RECORDTYPE: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RecordTypeNotSupported):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een niet ondersteund record type in veld RECORDTYPE: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(ShapeFileProblems.HasNoShapeRecords):
                        translation = "Het bestand bevat geen enkele geometrie.";
                        break;

                    case nameof(ShapeFileProblems.ShapeHeaderFormatError):
                        translation = "De hoofding van het bestand is niet correct geformateerd.";
                        break;

                    case nameof(ShapeFileProblems.HasShapeRecordFormatError):
                        translation =
                            $"De shape record na record {problem.Parameters[0].Value} is niet correct geformateerd.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordShapeTypeMismatch):
                        translation =
                            $"De shape record {problem.Parameters[0].Value} bevat geen {problem.Parameters[1].Value} maar een {problem.Parameters[2].Value}.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometryMismatch):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie is ongeldig.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometryLineCountMismatch):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie heeft meer lijnen dan verwacht.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometrySelfOverlaps):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie overlapt zichzelf.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometrySelfIntersects):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie kruist zichzelf.";
                        break;

                    default:
                        translation = $"'{problem.Reason}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };
    }
}
