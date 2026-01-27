namespace RoadRegistry.Infrastructure.DutchTranslations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoadRegistry.Infrastructure.Messages;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using Problem = Messages.Problem;

public static class ProblemTranslator
{
    private static readonly Dictionary<ProblemCode, Converter<Problem, ProblemTranslation>> ProblemCodeDutchTranslators = new()
    {
        {
            ProblemCode.Common.IncorrectObjectId, problem => new(problem.Severity, "IncorrectObjectId",
                $"De waarde '{problem.Parameters[0].Value}' is ongeldig.")
        },
        {
            ProblemCode.Common.IsRequired, problem => new(problem.Severity, "IsRequired",
                "De waarde is verplicht.")
        },
        {
            ProblemCode.Common.JsonInvalid, problem => new(problem.Severity, "JsonInvalid",
                "Ongeldig JSON formaat.")
        },
        {
            ProblemCode.Common.NotFound, problem => new(problem.Severity, "NotFound",
                "De waarde ontbreekt.")
        },

        {
            ProblemCode.Count.IsRequired, problem => new(problem.Severity, "AantalVerplicht",
                "Aantal is verplicht.")
        },
        {
            ProblemCode.Count.NotValid, problem => new(problem.Severity, "AantalNietCorrect",
                $"Aantal is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.Direction.IsRequired, problem => new(problem.Severity, "RichtingVerplicht",
                "Richting is verplicht.")
        },
        {
            ProblemCode.Direction.NotValid, problem => new(problem.Severity, "RichtingNietCorrect",
                $"Richting is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.EuropeanRoad.NumberNotFound, problem => new(problem.Severity, problem.Reason,
                $"Het wegsegment is reeds geen onderdeel meer van deze europese weg met nummer {problem.Parameters[0].Value}.")
        },

        {
            ProblemCode.Extract.NotFound, problem => new(problem.Severity, "ExtractNietGekend",
                "Het extract werd niet gevonden.")
        },
        {
            ProblemCode.Extract.ContourIsRequired, problem => new(problem.Severity, "ContourVerplicht",
                "Contour is verplicht.")
        },
        {
            ProblemCode.Extract.ContourInvalid, problem => new(problem.Severity, "ContourOngeldig",
                "Contour is ongeldig.")
        },
        {
            ProblemCode.Extract.BeschrijvingIsRequired, problem => new(problem.Severity, "BeschrijvingVerplicht",
                "Beschrijving is verplicht.")
        },
        {
            ProblemCode.Extract.BeschrijvingTooLong, problem => new(problem.Severity, "BeschrijvingTeLang",
                $"Beschrijving mag niet langer zijn dan {ExtractDescription.MaxLength} karakters.")
        },
        {
            ProblemCode.Extract.ExterneIdInvalid, problem => new(problem.Severity, "ExterneIdOngeldig",
                "ExterneId is ongeldig.")
        },
        {
            ProblemCode.Extract.ProjectionInvalid, problem => new(problem.Severity, "ProjectieOngeldig",
                "Projectie formaat moet Lambert 1972 zijn")
        },
        {
            ProblemCode.Extract.NisCodeIsRequired, problem => new(problem.Severity, "NisCodeVerplicht",
                "NIS-code is verplicht.")
        },
        {
            ProblemCode.Extract.NisCodeInvalid, problem => new(problem.Severity, "NisCodeOngeldig",
                "NIS-code is ongeldig. Verwacht formaat: '12345'")
        },

        {
            ProblemCode.Extract.CanNotUploadForSupersededDownload, problem => new(problem.Severity,
                "UploadEnkelLaatsteDownload",
                "Upload is enkel toegelaten voor de laatste download van het extractaanvraag.")
        },
        {
            ProblemCode.Extract.ExtractHasNotBeenDownloaded, problem => new(problem.Severity,
                "ExtractNogNietGedownload",
                "Upload is enkel toegelaten voor een extract dat minstens 1 keer is gedownload.")
        },
        {
            ProblemCode.Extract.CanNotUploadForInformativeExtract, problem => new(problem.Severity,
                "ExtractIsInformatief",
                "Upload is niet toegelaten voor een informatieve extractaanvraag.")
        },
        {
            ProblemCode.Extract.CanNotUploadForClosedExtract, problem => new(problem.Severity,
                "ExtractIsGesloten",
                "Upload is niet toegelaten voor een gesloten extractaanvraag.")
        },
        {
            ProblemCode.Extract.InwinningszoneCompleted, problem => new(problem.Severity,
                "InwinningszoneIsGesloten",
                "Upload is niet toegelaten voor een gesloten inwinningszone.")
        },
        {
            ProblemCode.Extract.DownloadIdIsRequired, problem => new(problem.Severity,
                "DownloadIdVerplicht",
                "Download id is verplicht.")
        },
        {
            ProblemCode.Extract.CorruptArchive, problem => new(problem.Severity,
                "ArchiefCorrupt",
                "Het geüploade archief is corrupt of onleesbaar.")
        },

        {
            ProblemCode.FromPosition.IsRequired, problem => new(problem.Severity, "VanPositieVerplicht",
                "VanPositie is verplicht.")
        },
        {
            ProblemCode.FromPosition.NotEqualToZero, problem => new(problem.Severity, "VanPositieNulOntbreekt",
                "De VanPositie van het eerste element is niet gelijk aan 0.")
        },
        {
            ProblemCode.FromPosition.NotValid, problem => new(problem.Severity, "VanPositieNietCorrect",
                $"VanPositie is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.GradeSeparatedJunction.NotFound, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De ongelijkgrondse kruising met id {problem.GetParameterValue("Identifier")} is niet langer onderdeel van het wegen netwerk."
                : "De ongelijkgrondse kruising is niet langer onderdeel van het wegen netwerk.")
        },
        {
            ProblemCode.GradeSeparatedJunction.LowerSegmentMissing, problem => new(problem.Severity, problem.Reason,
                $"De ongelijkgrondse kruising zijn onderste wegsegment {problem.GetParameterValue("RoadSegmentId")} ontbreekt.")
        },
        {
            ProblemCode.GradeSeparatedJunction.UpperSegmentMissing, problem => new(problem.Severity, problem.Reason,
                $"De ongelijkgrondse kruising zijn bovenste wegsegment {problem.GetParameterValue("RoadSegmentId")} ontbreekt.")
        },
        {
            ProblemCode.GradeSeparatedJunction.UpperAndLowerDoNotIntersect, problem => new(problem.Severity, problem.Reason,
                $"De ongelijkgrondse kruising zijn bovenste wegsegment {problem.GetParameterValue("UpperRoadSegmentId")} en onderste wegsegment {problem.GetParameterValue("LowerRoadSegmentId")} kruisen elkaar niet.")
        },
        {
            ProblemCode.NationalRoad.NumberNotFound, problem => new(problem.Severity, problem.Reason,
                $"Het wegsegment is reeds geen onderdeel meer van deze nationale weg met nummer {problem.Parameters[0].Value}.")
        },
        {
            ProblemCode.NumberedRoad.NumberNotFound, problem => new(problem.Severity, problem.Reason,
                $"Het wegsegment is reeds geen onderdeel meer van deze genummerde weg met nummer {problem.Parameters[0].Value}.")
        },

        {
            ProblemCode.RoadNetwork.NotFound, problem => new(problem.Severity, "NotFound",
                "Onbestaand wegen netwerk.")
        },
        {
            ProblemCode.RoadNetwork.Disconnected, problem => new(problem.Severity, "WegenNetwerkNietVerbonden",
                $"De wegknoop {problem.GetParameterValue("StartNodeId")} heeft geen verbinding meer met wegknoop {problem.GetParameterValue("EndNodeId")}.")
        },
        {
            ProblemCode.RoadNetwork.NoChanges, problem => new(problem.Severity, "GeenWijzigingen",
                "Er werden geen wijzigingen gevonden.")
        },

        {
            ProblemCode.RoadNode.NotConnectedToAnySegment, problem => new(problem.Severity, problem.Reason,
                $"De wegknoop {problem.Parameters[0].Value} is met geen enkel wegsegment verbonden.")
        },
        {
            ProblemCode.RoadNode.NotFound, problem => new(problem.Severity, problem.Reason,
                "De wegknoop is niet langer onderdeel van het wegen netwerk.")
        },
        {
            ProblemCode.RoadNode.TemporaryIdNotUnique, problem => new(problem.Severity, problem.Reason,
                $"De opgegeven tijdelijke wegknoop ID {problem.Parameters[0].Value} is niet uniek.")
        },
        {
            ProblemCode.RoadNode.TooClose, problem => new(problem.Severity, problem.Reason,
                $"De geometrie ligt te dicht bij wegsegment met id {problem.Parameters[0].Value}.")
        },
        {
            ProblemCode.RoadNode.TypeMismatch, problem => new(problem.Severity, problem.Reason,
                GetRoadNodeTypeMismatch(problem))
        },
        {
            ProblemCode.RoadNode.TypeV2Mismatch, problem => new(problem.Severity, problem.Reason,
                GetRoadNodeTypeV2Mismatch(problem))
        },
        {
            ProblemCode.RoadNode.Fake.ConnectedSegmentsDoNotDiffer, problem => new(problem.Severity, problem.Reason,
                $"De attributen van de verbonden wegsegmenten ({problem.Parameters[1].Value} en {problem.Parameters[2].Value}) verschillen onvoldoende voor deze schijnknoop.")
        },
        {
            ProblemCode.RoadNode.Geometry.Taken, problem => new(problem.Severity, problem.Reason,
                $"De geometrie werd reeds ingenomen door een andere wegknoop met id {problem.Parameters[0].Value}.")
        },

        {
            ProblemCode.RoadSegment.ChangeAttributesAttributeNotValid, problem => new(problem.Severity, "AttribuutNietGekend",
                $"De waarde '{problem.Parameters[0].Value}' komt niet overeen met een attribuut uit het Wegenregister dat via dit endpoint gewijzigd kan worden.")
        },
        {
            ProblemCode.RoadSegment.ChangeAttributesRequestNull, problem => new(problem.Severity, "NotFound",
                "Ten minste één attribuut moet opgegeven worden.")
        },
        {
            ProblemCode.RoadSegment.IdsNotUnique, problem => new(problem.Severity, "WegsegmentenNietUniek",
                "De wegsegmenten moeten uniek zijn.")
        },
        {
            ProblemCode.RoadSegment.IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction, problem => new(problem.Severity, problem.Reason,
                $"Het wegsegment {problem.Parameters[0].Value} mag niet kruisen met wegsegment {problem.Parameters[1].Value}.")
        },
        {
            ProblemCode.RoadSegment.LowerMissing, problem => new(problem.Severity, problem.Reason,
                "Het onderste wegsegment ontbreekt.")
        },
        {
            ProblemCode.RoadSegment.Missing, problem => new(problem.Severity, problem.Reason,
                $"Het wegsegment met id {problem.Parameters[0].Value} ontbreekt.")
        },
        {
            ProblemCode.RoadSegment.NotFound, problem => new(problem.Severity, "NotFound", problem.HasParameter("SegmentId")
                ? $"Het wegsegment met id {problem.GetParameterValue("SegmentId")} bestaat niet."
                : "Dit wegsegment bestaat niet.")
        },
        {
            ProblemCode.RoadSegment.OutlinedNotFound, problem => new(problem.Severity, "NotFound",
                "Dit wegsegment bestaat niet of heeft niet de geometriemethode 'ingeschetst'.")
        },
        {
            ProblemCode.RoadSegment.TemporaryIdNotUnique, problem => new(problem.Severity, problem.Reason,
                $"De opgegeven tijdelijke wegsegment ID {problem.Parameters[0].Value} is niet uniek.")
        },
        {
            ProblemCode.RoadSegment.UpperAndLowerDoNotIntersect, problem => new(problem.Severity, problem.Reason,
                "Het bovenste en onderste wegsegment kruisen elkaar niet.")
        },
        {
            ProblemCode.RoadSegment.UpperMissing, problem => new(problem.Severity, problem.Reason,
                "Het bovenste wegsegment ontbreekt.")
        },
        {
            ProblemCode.RoadSegment.NotRemovedBecauseCategoryIsInvalid, problem => new(problem.Severity, "WegsegmentOngeldigeCategorie",
                $"Wegsegment {problem.GetParameterValue("Identifier")} mag niet verwijderd worden omwille van zijn categorie '{problem.GetParameterValue("Category")}'.")
        },
        {
            ProblemCode.RoadSegment.EndNode.Missing, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier") || problem.HasParameter("Actual")
                ? $"De eind wegknoop van het wegsegment met id {problem.GetOptionalParameterValue("Identifier") ?? problem.GetOptionalParameterValue("Actual")} ontbreekt."
                : "De eind wegknoop van het wegsegment ontbreekt.")
        },
        {
            ProblemCode.RoadSegment.EndNode.RefersToRemovedNode, problem => new(problem.Severity, problem.Reason,
                $"De eind knoop van wegsegment {problem.Parameters[0].Value} verwijst naar een verwijderde knoop {problem.Parameters[1].Value}.")
        },
        {
            ProblemCode.RoadSegment.EndPoint.DoesNotMatchNodeGeometry, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De positie van het eind punt van het wegsegment met id {problem.GetParameterValue("Identifier")} stemt niet overeen met de geometrie van de eind wegknoop."
                : "De positie van het eind punt van het wegsegment stemt niet overeen met de geometrie van de eind wegknoop.")
        },
        {
            ProblemCode.RoadSegment.EndPoint.MeasureValueNotEqualToLength, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De meting ({problem.GetParameterValue("Measure")}) op het eind punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] van wegsegment met id {problem.GetParameterValue("Identifier")} is niet gelijk aan de lengte ({problem.GetParameterValue("Length")})."
                : $"De meting ({problem.GetParameterValue("Measure")}) op het eind punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] is niet gelijk aan de lengte ({problem.GetParameterValue("Length")}).")
        },

        {
            ProblemCode.RoadSegment.EuropeanRoads.NotUnique, problem => new(problem.Severity, "EuropeseWegenNietUniek",
                "De Europese wegen moeten uniek zijn o.b.v. hun EU-nummer.")
        },
        {
            ProblemCode.RoadSegment.EuropeanRoadNumber.IsRequired, problem => new(problem.Severity, "EuropeseWegEuNummerVerplicht",
                "EU-nummer is verplicht.")
        },
        {
            ProblemCode.RoadSegment.EuropeanRoadNumber.NotValid, problem => new(problem.Severity, "EuropeseWegEuNummerNietCorrect",
                $"EU-nummer is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.RoadSegment.NationalRoads.NotUnique, problem => new(problem.Severity, "NationaleWegenNietUniek",
                "De nationale wegen moeten uniek zijn o.b.v. hun ident2.")
        },
        {
            ProblemCode.RoadSegment.NationalRoadNumber.IsRequired, problem => new(problem.Severity, "NationaleWegIdent2Verplicht",
                "Ident2 is verplicht.")
        },
        {
            ProblemCode.RoadSegment.NationalRoadNumber.NotValid, problem => new(problem.Severity, "NationaleWegIdent2NietCorrect",
                $"Ident2 is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.RoadSegment.NumberedRoads.NotUnique, problem => new(problem.Severity, "GenummerdeWegenNietUniek",
                "De genummerde wegen moeten uniek zijn o.b.v. hun ident8.")
        },
        {
            ProblemCode.RoadSegment.NumberedRoadNumber.IsRequired, problem => new(problem.Severity, "GenummerdeWegIdent8Verplicht",
                "Ident8 is verplicht.")
        },
        {
            ProblemCode.RoadSegment.NumberedRoadNumber.NotValid, problem => new(problem.Severity, "GenummerdeWegIdent8NietCorrect",
                $"Ident8 is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.RoadSegment.NumberedRoadDirection.IsRequired, problem => new(problem.Severity, "GenummerdeWegRichtingVerplicht",
                "Richting is verplicht.")
        },
        {
            ProblemCode.RoadSegment.NumberedRoadDirection.NotValid, problem => new(problem.Severity, "GenummerdeWegRichtingNietCorrect",
                $"Richting is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.RoadSegment.NumberedRoadOrdinal.IsRequired, problem => new(problem.Severity, "GenummerdeWegVolgnummerVerplicht",
                "Volgnummer is verplicht.")
        },
        {
            ProblemCode.RoadSegment.NumberedRoadOrdinal.NotValid, problem => new(problem.Severity, "GenummerdeWegVolgnummerNietCorrect",
                $"Volgnummer is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.RoadSegment.Geometry.IsRequired, problem => new(problem.Severity, "MiddellijnGeometrieVerplicht",
                "Middellijngeometrie is verplicht.")
        },
        {
            ProblemCode.RoadSegment.Geometry.NotValid, problem => new(problem.Severity, "MiddellijnGeometrieNietCorrect",
                "De opgegeven geometrie is geen geldige LineString in gml 3.2.")
        },
        {
            ProblemCode.RoadSegment.Geometry.LengthIsZero, problem => new(problem.Severity, problem.Reason, problem.HasParameter("RecordNumber")
                ? $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie lengte is 0."
                : problem.HasParameter("Identifier")
                    ? $"De lengte van het wegsegment met id {problem.GetParameterValue("Identifier")} is 0."
                    : "De lengte van het wegsegment is 0.")
        },
        {
            ProblemCode.RoadSegment.Geometry.SelfIntersects, problem => new(problem.Severity, problem.Reason, problem.HasParameter("RecordNumber")
                ? $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie kruist zichzelf."
                : problem.HasParameter("Identifier")
                    ? $"De geometrie van het wegsegment met id {problem.GetParameterValue("Identifier")} kruist zichzelf."
                    : "De geometrie van het wegsegment kruist zichzelf.")
        },
        {
            ProblemCode.RoadSegment.Geometry.SelfOverlaps, problem => new(problem.Severity, problem.Reason, problem.HasParameter("RecordNumber")
                ? $"De shape record {problem.GetParameterValue("RecordNumber")} geometrie overlapt zichzelf."
                : problem.HasParameter("Identifier")
                    ? $"De geometrie van het wegsegment met id {problem.GetParameterValue("Identifier")} overlapt zichzelf."
                    : "De geometrie van het wegsegment overlapt zichzelf.")
        },
        {
            ProblemCode.RoadSegment.Geometry.LengthLessThanMinimum, problem => new(problem.Severity, "MiddellijnGeometrieKorterDanMinimum", problem.HasParameter("Identifier")
                ? $"De opgegeven geometrie van wegsegment met id {problem.GetParameterValue("Identifier")} heeft niet de minimale lengte van {problem.GetParameterValue("Minimum")} meter."
                : $"De opgegeven geometrie heeft niet de minimale lengte van {problem.GetParameterValue("Minimum")} meter.")
        },
        {
            ProblemCode.RoadSegment.Geometry.LengthTooLong, problem => new(problem.Severity, "MiddellijnGeometrieTeLang", problem.HasParameter("Identifier")
                ? $"De opgegeven geometrie van wegsegment met id {problem.GetParameterValue("Identifier")} zijn lengte is groter of gelijk dan {problem.GetParameterValue("TooLongSegmentLength")} meter."
                : $"De opgegeven geometrie zijn lengte is groter of gelijk dan {problem.GetParameterValue("TooLongSegmentLength")} meter.")
        },
        {
            ProblemCode.RoadSegment.Geometry.SridNotValid, problem => new(problem.Severity, "MiddellijnGeometrieCRSNietCorrect",
                "De opgegeven geometrie heeft niet het coördinatenstelsel Lambert 72.")
        },
        {
            ProblemCode.RoadSegment.Geometry.Taken, problem => new(problem.Severity, problem.Reason,
                $"De geometrie werd reeds ingenomen door een ander wegsegment met id {problem.Parameters[0].Value}.")
        },
        {
            ProblemCode.RoadSegment.GeometryDrawMethod.NotOutlined, problem => new(problem.Severity, "GeometriemethodeNietIngeschetst",
                "De geometriemethode van dit wegsegment komt niet overeen met 'ingeschetst'.")
        },
        {
            ProblemCode.RoadSegment.GeometryDrawMethod.NotValid, problem => new(problem.Severity, "GeometriemethodeNietCorrect",
                $"Geometriemethode is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.Lane.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste rijstroken attribuut met id {problem.Parameters[0].Value} is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.Lane.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.Lane.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het rijstroken attribuut met id {problem.Parameters[2].Value}.")
        },
        {
            ProblemCode.RoadSegment.Lane.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste rijstroken attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.Lane.IsRequired, problem => new(problem.Severity, "AantalRijstrokenVerplicht",
                "Aantal rijstroken is verplicht.")
        },
        {
            ProblemCode.RoadSegment.Lane.GreaterThanZero, problem => new(problem.Severity, "AantalRijstrokenGroterDanNul",
                "Aantal rijstroken moet groter dan nul zijn.")
        },
        {
            ProblemCode.RoadSegment.Lane.LessThanOrEqualToMaximum, problem => new(problem.Severity, "AantalRijstrokenKleinerOfGelijkAanMaximum",
                $"Aantal rijstroken mag niet groter dan {RoadSegmentLaneCount.Maximum} zijn.")
        },
        {
            ProblemCode.RoadSegment.Lanes.CountGreaterThanOne, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"Wegsegment met id {problem.GetParameterValue("Identifier")} heeft meer dan 1 rijstrook."
                : "Wegsegment heeft meer dan 1 rijstrook.")
        },
        {
            ProblemCode.RoadSegment.Lanes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"Wegsegment met id {problem.GetParameterValue("Identifier")} heeft geen enkel rijstrook."
                : "Wegsegment heeft geen enkel rijstrook.")
        },
        {
            ProblemCode.RoadSegment.LaneCount.IsRequired, problem => new(problem.Severity, "AantalRijstrokenVerplicht",
                "Aantal rijstroken is verplicht.")
        },
        {
            ProblemCode.RoadSegment.LaneCount.NotValid, problem => new(problem.Severity, "AantalRijstrokenNietCorrect",
                $"Aantal rijstroken is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.LaneDirection.IsRequired, problem => new(problem.Severity, "AantalRijstrokenRichtingVerplicht",
                "Aantal rijstroken richting is verplicht.")
        },
        {
            ProblemCode.RoadSegment.LaneDirection.NotValid, problem => new(problem.Severity, "AantalRijstrokenRichtingNietCorrect",
                $"Aantal rijstroken richting is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired, problem => new(problem.Severity, "WegbeheerderVerplicht",
                "Wegbeheerder is verplicht.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.NotValid, problem => new(problem.Severity, "WegbeheerderNietCorrect",
                $"Wegbeheerder is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown, problem => new(problem.Severity, "WegbeheerderNietGekend",
                $"De opgegeven wegbeheerdercode '{problem.GetParameterValue(MaintenanceAuthorityNotKnown.ParameterName.OrganizationId)}' komt niet overeen met een (OVO-code die correspondeert met een) code gekend door het Wegenregister.")
        },
        {
            ProblemCode.RoadSegment.Morphology.NotValid, problem => new(problem.Severity, "MorfologischeWegklasseNietCorrect",
                $"Morfologische wegklasse is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.Morphology.IsRequired, problem => new(problem.Severity, "MorfologischeWegklasseVerplicht",
                "Morfologische wegklasse is verplicht.")
        },
        {
            ProblemCode.RoadSegment.Point.MeasureValueDoesNotIncrease, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De meting ({problem.GetParameterValue("Measure")}) van het wegsegment met id {problem.GetParameterValue("Identifier")} op het punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] is niet groter dan de meting ({problem.GetParameterValue("PreviousMeasure")}) op het vorige punt."
                : $"De meting ({problem.GetParameterValue("Measure")}) op het punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] is niet groter dan de meting ({problem.GetParameterValue("PreviousMeasure")}) op het vorige punt.")
        },
        {
            ProblemCode.RoadSegment.Point.MeasureValueOutOfRange, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De meting ({problem.GetParameterValue("Measure")}) van het wegsegment met id {problem.GetParameterValue("Identifier")} op het punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] ligt niet binnen de verwachte grenzen [{problem.GetParameterValue("MeasureLowerBoundary")}-{problem.GetParameterValue("MeasureUpperBoundary")}]."
                : $"De meting ({problem.GetParameterValue("Measure")}) op het punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] ligt niet binnen de verwachte grenzen [{problem.GetParameterValue("MeasureLowerBoundary")}-{problem.GetParameterValue("MeasureUpperBoundary")}].")
        },
        {
            ProblemCode.RoadSegment.StartNode.Missing, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier") || problem.HasParameter("Actual")
                ? $"De start wegknoop van het wegsegment met id {problem.GetOptionalParameterValue("Identifier") ?? problem.GetOptionalParameterValue("Actual")} ontbreekt."
                : "De start wegknoop van het wegsegment ontbreekt.")
        },
        {
            ProblemCode.RoadSegment.StartNode.RefersToRemovedNode, problem => new(problem.Severity, problem.Reason,
                $"De begin knoop van wegsegment met id {problem.Parameters[0].Value} verwijst naar een verwijderde knoop {problem.Parameters[1].Value}.")
        },
        {
            ProblemCode.RoadSegment.StartPoint.DoesNotMatchNodeGeometry, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De positie van het start punt van het wegsegment met id {problem.GetParameterValue("Identifier")} stemt niet overeen met de geometrie van de start wegknoop."
                : "De positie van het start punt van het wegsegment stemt niet overeen met de geometrie van de start wegknoop.")
        },
        {
            ProblemCode.RoadSegment.StartPoint.MeasureValueNotEqualToZero, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"De meting ({problem.GetParameterValue("Measure")}) van het wegsegment met id {problem.GetParameterValue("Identifier")} op het start punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] is niet 0.0."
                : $"De meting ({problem.GetParameterValue("Measure")}) op het start punt [X={problem.GetParameterValue("PointX")},Y={problem.GetParameterValue("PointY")}] is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.Status.NotValid, problem => new(problem.Severity, "WegsegmentStatusNietCorrect",
                $"Wegsegment status is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.Status.IsRequired, problem => new(problem.Severity, "WegsegmentStatusVerplicht",
                "Wegsegment status is verplicht.")
        },
        {
            ProblemCode.RoadSegment.StreetName.NotProposedOrCurrent, problem => new(problem.Severity, "WegsegmentStraatnaamNietVoorgesteldOfInGebruik",
                "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.")
        },

        {
            ProblemCode.RoadSegment.StreetName.Left.NotLinked, problem => new(problem.Severity, "LinkerstraatnaamNietGekoppeld",
                $"Het wegsegment met id {problem.Parameters[0].Value} is niet gekoppeld aan de linkerstraatnaam '{problem.Parameters[1].Value}'")
        },
        {
            ProblemCode.RoadSegment.StreetName.Left.NotUnlinked, problem => new(problem.Severity, "LinkerstraatnaamNietOntkoppeld",
                $"Het wegsegment met id {problem.Parameters[0].Value} heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.")
        },
        {
            ProblemCode.RoadSegment.StreetName.Left.NotProposedOrCurrent, problem => new(problem.Severity, "LinkerstraatnaamNietVoorgesteldOfInGebruik",
                $"De linkerstraatnaam voor wegsegment met id {problem.GetParameterValue("SegmentId")} moet status 'voorgesteld' of 'in gebruik' hebben.")
        },
        {
            ProblemCode.RoadSegment.StreetName.Left.NotFound, problem => new(problem.Severity, "LinkerstraatnaamNietGekend",
                $"De linkerstraatnaam voor het wegsegment met id {problem.GetParameterValue("SegmentId")} is niet gekend in het Straatnamenregister.")
        },
        {
            ProblemCode.RoadSegment.StreetName.Left.NotValid, problem => new(problem.Severity, "LinkerstraatnaamNietCorrect",
                $"De linkstraatnaam '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.RoadSegment.StreetName.Right.NotLinked, problem => new(problem.Severity, "RechterstraatnaamNietGekoppeld",
                $"Het wegsegment met id {problem.Parameters[0].Value} is niet gekoppeld aan de rechterstraatnaam '{problem.Parameters[1].Value}'")
        },
        {
            ProblemCode.RoadSegment.StreetName.Right.NotUnlinked, problem => new(problem.Severity, "RechterstraatnaamNietOntkoppeld",
                $"Het wegsegment met id {problem.Parameters[0].Value} heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.")
        },
        {
            ProblemCode.RoadSegment.StreetName.Right.NotProposedOrCurrent, problem => new(problem.Severity, "RechterstraatnaamNietVoorgesteldOfInGebruik",
                $"De rechterstraatnaam voor wegsegment met id {problem.GetParameterValue("SegmentId")} moet status 'voorgesteld' of 'in gebruik' hebben.")
        },
        {
            ProblemCode.RoadSegment.StreetName.Right.NotFound, problem => new(problem.Severity, "RechterstraatnaamNietGekend",
                $"De rechterstraatnaam voor het wegsegment met id {problem.GetParameterValue("SegmentId")} is niet gekend in het Straatnamenregister.")
        },
        {
            ProblemCode.RoadSegment.StreetName.Right.NotValid, problem => new(problem.Severity, "LinkerstraatnaamNietCorrect",
                $"De linkstraatnaam '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        // {
        //     ProblemCode.RoadSegment.Surface.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
        //         $"De van positie ({problem.Parameters[1].Value}) van het eerste wegverharding attribuut met id {problem.Parameters[0].Value} is niet 0.0.")
        // },
        // {
        //     ProblemCode.RoadSegment.Surface.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
        //         $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} heeft lengte 0.")
        // },
        {
            ProblemCode.RoadSegment.Surface.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het wegverharding attribuut met id {problem.Parameters[2].Value}.")
        },
        // {
        //     ProblemCode.RoadSegment.Surface.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
        //         $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegverharding attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        // },
        {
            ProblemCode.RoadSegment.Surfaces.CountGreaterThanOne, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"Wegsegment met id {problem.GetParameterValue("Identifier")} heeft meer dan 1 wegverharding."
                : "Wegsegment heeft meer dan 1 wegverharding.")
        },
        {
            ProblemCode.RoadSegment.Surfaces.HasCountOfZero, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"Wegsegment met id {problem.GetParameterValue("Identifier")} heeft geen enkele wegverharding."
                : "Wegsegment heeft geen enkele wegverharding.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.IsRequired, problem => new(problem.Severity, "WegverhardingVerplicht",
                "Wegverharding is verplicht.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.NotValid, problem => new(problem.Severity, "WegverhardingNietCorrect",
                $"Wegverharding is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.Width.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.Width.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.Width.IsRequired, problem => new(problem.Severity, "WegbreedteVerplicht",
                "Wegbreedte is verplicht.")
        },
        {
            ProblemCode.RoadSegment.Width.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het wegbreedte attribuut met id {problem.Parameters[2].Value}.")
        },
        {
            ProblemCode.RoadSegment.Width.NotValid, problem => new(problem.Severity, "WegbreedteNietCorrect",
                $"Wegbreedte is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.Width.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.Width.LessThanOrEqualToMaximum, problem => new(problem.Severity, "WegbreedteKleinerOfGelijkAanMaximum",
                $"Wegbreedte mag niet groter dan {RoadSegmentWidth.Maximum} meter zijn.")
        },
        {
            ProblemCode.RoadSegment.Widths.CountGreaterThanOne, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"Wegsegment met id {problem.GetParameterValue("Identifier")} heeft meer dan 1 wegbreedte."
                : "Wegsegment heeft meer dan 1 wegbreedte.")
        },
        {
            ProblemCode.RoadSegment.Widths.HasCountOfZero, problem => new(problem.Severity, problem.Reason, problem.HasParameter("Identifier")
                ? $"Wegsegment met id {problem.GetParameterValue("Identifier")} heeft geen enkele wegbreedte."
                : "Wegsegment heeft geen enkele wegbreedte.")
        },
        {
            ProblemCode.RoadSegments.NotFound, problem => new(problem.Severity, "NotFound",
                $"Onbestaande of verwijderde wegsegmenten gevonden: {problem.Parameters[0].Value}")
        },

        {
            ProblemCode.ShapeFile.GeometrySridMustBeEqual, problem => new(problem.Severity, problem.Reason,
                "SRID van alle geometrieën moeten dezelfde zijn.")
        },
        {
            ProblemCode.ShapeFile.GeometryTypeMustBePolygon, problem => new(problem.Severity, problem.Reason,
                "Geometrie type moet een polygoon of multi polygoon zijn.")
        },
        {
            ProblemCode.ShapeFile.HasNoValidPolygons, problem => new(problem.Severity, problem.Reason,
                "Het shape bestand bevat geen geldige polygonen.")
        },
        {
            ProblemCode.ShapeFile.InvalidHeader, problem => new(problem.Severity, problem.Reason,
                $"Kan header van de shape file niet lezen: '{problem.Parameters[0].Value}'")
        },
        {
            ProblemCode.ShapeFile.InvalidPolygonShellOrientation, problem => new(problem.Severity, problem.Reason,
                "De orientatie van de polygoon moet in wijzerzin zijn.")
        },

        { ProblemCode.StreetName.NotFound, problem => new(problem.Severity, "StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.") },
        {
            ProblemCode.StreetName.RegistryUnexpectedError, problem => new(problem.Severity, "StraatnamenregisterOnverwachteFout",
                $"Het Straatnamenregister gaf een onverwachte fout {problem.GetParameterValue("StatusCode")}.")
        },

        {
            ProblemCode.ToPosition.IsRequired, problem => new(problem.Severity, "TotPositieVerplicht",
                "TotPositie is verplicht.")
        },
        {
            ProblemCode.ToPosition.NotEqualToNextFromPosition, problem => new(problem.Severity, "TotPositieNietGelijkAanVolgendeVanPositie",
                $"De totPositie verschilt van de volgende vanPositie.")
        },
        {
            ProblemCode.ToPosition.LessThanOrEqualFromPosition, problem => new(problem.Severity, "TotPositieKleinerOfGelijkAanVanPositie",
                $"De totPositie moet groter zijn dan de vanPositie.")
        },
        {
            ProblemCode.ToPosition.NotValid, problem => new(problem.Severity, "TotPositieNietCorrect",
                $"TotPositie is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.Type.IsRequired, problem => new(problem.Severity, "TypeVerplicht",
                "Type is verplicht.")
        },
        {
            ProblemCode.Type.NotValid, problem => new(problem.Severity, "TypeNietCorrect",
                $"Type is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },

        {
            ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce, problem => new(problem.Severity, "CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce",
                "Kan geen meerdere uploads uitvoeren voor hetzelfde extractaanvraag.")
        },
        {
            ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload, problem => new(problem.Severity, "CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload",
                "Upload is enkel toegelaten voor de laatste download van het extractaanvraag.")
        },
        {
            ProblemCode.Upload.UploadNotAllowedForInformativeExtract, problem => new(problem.Severity, "ExtractRequestMarkedInformative",
                "Upload is niet toegelaten voor een informatieve extractaanvraag.")
        },
        {
            ProblemCode.Upload.UnexpectedError, problem => new(problem.Severity, "OnverwachteFout",
                "Onverwachte fout bij de verwerking van het zip-bestand.")
        },
        {
            ProblemCode.Upload.UnsupportedMediaType, problem => new(problem.Severity, "UnsupportedMediaType", problem.HasParameter("ContentType")
                ? $"Bestandstype is foutief. '{problem.GetParameterValue("ContentType")}' is geen geldige waarde."
                : "Ongeldig bestandstype.")
        },
        {
            ProblemCode.Upload.DownloadIdIsRequired, problem => new(problem.Severity, "DownloadIdIsRequired",
                "Download id is verplicht.")
        },
        {
            ProblemCode.Width.IsRequired, problem => new(problem.Severity, "BreedteVerplicht",
                "Breedte is verplicht.")
        },
        {
            ProblemCode.Width.NotValid, problem => new(problem.Severity, "BreedteNietCorrect",
                $"Breedte is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.IsRequired, problem => new(problem.Severity, "ToegangsbeperkingVerplicht",
                "Toegangsbeperking is verplicht.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.NotValid, problem => new(problem.Severity, "ToegangsbeperkingNietCorrect",
                $"Toegangsbeperking is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een toegangsbeperking attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste toegangsbeperking attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele toegangsbeperking.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een toegangsbeperking attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een toegangsbeperking attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het toegangsbeperking attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste toegangsbeperking attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"Het toegangsbeperking bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.Category.IsRequired, problem => new(problem.Severity, "WegcategorieVerplicht",
                "Wegcategorie is verplicht.")
        },
        {
            ProblemCode.RoadSegment.Category.NotValid, problem => new(problem.Severity, "WegcategorieNietCorrect",
                $"Wegcategorie is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.Category.NotChangedBecauseCurrentIsNewerVersion, problem => new(problem.Severity, "WegcategorieNietVeranderdHuidigeBevatRecentereVersie",
                $"Wegcategorie werd niet gewijzigd voor wegsegment {problem.GetParameterValue("Identifier")} omdat het record reeds een recentere versie bevat.")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een categorie attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste categorie attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele categorie.")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een categorie attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een categorie attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het categorie attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste categorie attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"De categorie bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.IsRequired, problem => new(problem.Severity, "AutoToegangVerplicht",
                "Auto toegang is verplicht.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.NotValid, problem => new(problem.Severity, "AutoToegangNietCorrect",
                $"Auto toegang is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een auto toegang attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste auto toegang attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele auto toegang.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een auto toegang attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een auto toegang attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het auto toegang attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste auto toegang attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"Het auto toegang bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.IsRequired, problem => new(problem.Severity, "FietsToegangVerplicht",
                "Fiets toegang is verplicht.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.NotValid, problem => new(problem.Severity, "FietsToegangNietCorrect",
                $"Fiets toegang is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een fiets toegang attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste fiets toegang attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele fiets toegang.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een fiets toegang attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een fiets toegang attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het fiets toegang attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste fiets toegang attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"Het fiets toegang bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een morfologie attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste morfologie attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele morfologie.")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een morfologie attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een morfologie attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het morfologie attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste morfologie attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"Het morfologie bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.IsRequired, problem => new(problem.Severity, "VoetgangersToegangVerplicht",
                "Voetgangers toegang is verplicht.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.NotValid, problem => new(problem.Severity, "VoetgangersToegangNietCorrect",
                $"Voetgangers toegang is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een voetgangers toegang attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste voetgangers toegang attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele voetgangers toegang.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een voetgangers toegang attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een voetgangers toegang attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het voetgangers toegang attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste voetgangers toegang attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"Het voetgangers toegang bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een status attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste status attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele status.")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een status attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een status attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het status attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste status attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"De status bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een straatnaam attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste straatnaam attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele straatnaam.")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een straatnaam attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een straatnaam attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het straatnaam attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste straatnaam attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"Het straatnaam bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een wegbeheerder attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste wegbeheerder attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele wegbeheerder.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een wegbeheerder attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een wegbeheerder attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het wegbeheerder attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegbeheerder attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"De wegbeheerder bevat meerdere attributen voor dezelfde dekking.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.FromOrToPositionIsNull, problem => new(problem.Severity, problem.Reason,
                $"De van of naar positie van een wegverharding attribuut is leeg.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason,
                $"De van positie ({problem.Parameters[1].Value}) van het eerste wegverharding attribuut is niet 0.0.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele wegverharding.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.HasLengthOfZero, problem => new(problem.Severity, problem.Reason,
                $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van een wegverharding attribuut heeft lengte 0.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.LeftOrRightNotAllowedWhenUsingBoth, problem => new(problem.Severity, problem.Reason,
                $"Een wegverharding attribuut mag geen kant 'beide' gebruiken wanneer er 'links' of 'rechts' wordt gebruikt voor de dekking.")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.NotAdjacent, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het wegverharding attribuut sluit niet aan op de van positie ({problem.Parameters[3].Value}).")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason,
                $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegverharding attribuut is niet gelijk aan de lengte van het wegsegment ({problem.Parameters[2].Value}).")
        },
        {
            ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes.ValueNotUniqueWithinSegment, problem => new(problem.Severity, problem.Reason,
                $"De wegverharding bevat meerdere attributen voor dezelfde dekking.")
        },
    };

    public static readonly Converter<Problem, ProblemTranslation> Dutch = problem =>
    {
        var problemCode = ProblemCode.FromReason(problem.Reason);
        if (problemCode is not null && ProblemCodeDutchTranslators.TryGetValue(problemCode, out Converter<Problem, ProblemTranslation> converter))
        {
            return converter(problem);
        }

        return new ProblemTranslation(problem.Severity, problem.Reason, CreateMissingTranslationMessage(problemCode!));
    };

    public static string CreateMissingTranslationMessage(ProblemCode problemCode)
    {
        return $"'{problemCode}' has no translation. Please fix it.";
    }

    static string GetRoadNodeTypeMismatch(Problem problem)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Het opgegeven wegknoop type {0} van knoop {1} komt niet overeen met een van de verwachte wegknoop types: ",
            RoadNodeType.Parse(problem.GetParameterValue("Actual")).Translation.Name,
            problem.GetParameterValue("RoadNodeId"));
        sb.AppendFormat("{0} ", string.Join(',', problem.Parameters
            .Where(p => p.Name == "Expected")
            .Select(parameter => RoadNodeType.Parse(parameter.Value).Translation.Name)));
        sb.AppendFormat(". De wegknoop is verbonden met {0} wegsegment(-en)",
            problem.GetParameterValue("ConnectedSegmentCount"));
        if (problem.Parameters.Any(p => p.Name == "ConnectedSegmentId"))
        {
            sb.AppendFormat(": {0} ", string.Join(',', problem.Parameters
                .Where(p => p.Name == "ConnectedSegmentId")
                .Select(parameter => parameter.Value)));
        }

        sb.Append(".");

        return sb.ToString();
    }

    static string GetRoadNodeTypeV2Mismatch(Problem problem)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Het opgegeven wegknoop type {0} van knoop {1} komt niet overeen met een van de verwachte wegknoop types: ",
            RoadNodeTypeV2.Parse(problem.GetParameterValue("Actual")).Translation.Name,
            problem.GetParameterValue("RoadNodeId"));
        sb.AppendFormat("{0} ", string.Join(',', problem.Parameters
            .Where(p => p.Name == "Expected")
            .Select(parameter => RoadNodeTypeV2.Parse(parameter.Value).Translation.Name)));
        sb.AppendFormat(". De wegknoop is verbonden met {0} wegsegment(-en)",
            problem.GetParameterValue("ConnectedSegmentCount"));
        if (problem.Parameters.Any(p => p.Name == "ConnectedSegmentId"))
        {
            sb.AppendFormat(": {0} ", string.Join(',', problem.Parameters
                .Where(p => p.Name == "ConnectedSegmentId")
                .Select(parameter => parameter.Value)));
        }

        sb.Append(".");

        return sb.ToString();
    }
}
