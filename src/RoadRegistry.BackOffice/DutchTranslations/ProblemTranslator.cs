namespace RoadRegistry.BackOffice.DutchTranslations;

using BackOffice;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.ProblemCodes;
using Problem = Messages.Problem;

public static class ProblemTranslator
{
    private static readonly Dictionary<ProblemCode, Converter<Problem, ProblemTranslation>> ProblemCodeDutchTranslators = new()
    {
        {ProblemCode.Common.IncorrectObjectId, problem => new(problem.Severity, "IncorrectObjectId", $"De waarde '{problem.Parameters[0].Value}' is ongeldig.")},
        {ProblemCode.Common.IsRequired, problem => new(problem.Severity, "IsRequired", "De waarde is verplicht.")},
        {ProblemCode.Common.JsonInvalid, problem => new(problem.Severity, "JsonInvalid", "Ongeldig JSON formaat.")},
        {ProblemCode.Common.NotFound, problem => new(problem.Severity, "NotFound", "De waarde ontbreekt.")},
        {ProblemCode.Count.IsRequired, problem => new(problem.Severity, "AantalVerplicht", "Aantal is verplicht.")},
        {ProblemCode.Count.NotValid, problem => new(problem.Severity, "AantalNietCorrect", $"Aantal is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.Direction.IsRequired, problem => new(problem.Severity, "RichtingVerplicht", "Richting is verplicht.")},
        {ProblemCode.Direction.NotValid, problem => new(problem.Severity, "RichtingNietCorrect", $"Richting is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.EuropeanRoad.NumberNotFound, problem => new(problem.Severity, problem.Reason, $"Het wegsegment is reeds geen onderdeel meer van deze europese weg met nummer {problem.Parameters[0].Value}.") },
        {ProblemCode.FromPosition.IsRequired, problem => new(problem.Severity, "VanPositieVerplicht", "VanPositie is verplicht.")},
        {ProblemCode.FromPosition.NotValid, problem => new(problem.Severity, "VanPositieNietCorrect", $"VanPositie is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.GradeSeparatedJunction.NotFound, problem => new(problem.Severity, problem.Reason, "De ongelijkgrondse kruising is niet langer onderdeel van het wegen netwerk.") },
        {ProblemCode.NationalRoad.NumberNotFound, problem => new(problem.Severity, problem.Reason, $"Het wegsegment is reeds geen onderdeel meer van deze nationale weg met nummer {problem.Parameters[0].Value}.") },
        {ProblemCode.NumberedRoad.NumberNotFound, problem => new(problem.Severity, problem.Reason, $"Het wegsegment is reeds geen onderdeel meer van deze genummerde weg met nummer {problem.Parameters[0].Value}.") },
        {ProblemCode.RoadNetwork.NotFound, problem => new(problem.Severity, "NotFound", "Onbestaand wegen netwerk.")},
        {ProblemCode.RoadNode.NotConnectedToAnySegment, problem => new(problem.Severity, problem.Reason, "De wegknoop is met geen enkel wegsegment verbonden.") },
        {ProblemCode.RoadNode.NotFound, problem => new(problem.Severity, problem.Reason, "De wegknoop is niet langer onderdeel van het wegen netwerk.") },
        {ProblemCode.RoadNode.TooClose, problem => new(problem.Severity, problem.Reason, $"De geometrie ligt te dicht bij wegsegment met id {problem.Parameters[0].Value}.") },
        {ProblemCode.RoadNode.TypeMismatch, problem => new(problem.Severity, problem.Reason, GetRoadNodeTypeMismatch(problem)) },
        {ProblemCode.RoadNode.Fake.ConnectedSegmentsDoNotDiffer, problem => new(problem.Severity, problem.Reason, $"De attributen van de verbonden wegsegmenten ({problem.Parameters[1].Value} en {problem.Parameters[2].Value}) verschillen onvoldoende voor deze schijnknoop.") },
        {ProblemCode.RoadNode.Geometry.Taken, problem => new(problem.Severity, problem.Reason, $"De geometrie werd reeds ingenomen door een andere wegknoop met id {problem.Parameters[0].Value}.") },
        {ProblemCode.RoadSegment.ChangeAttributesAttributeNotValid, problem => new(problem.Severity, "AttribuutNietGekend", $"De waarde '{problem.Parameters[0].Value}' komt niet overeen met een attribuut uit het Wegenregister dat via dit endpoint gewijzigd kan worden.") },
        {ProblemCode.RoadSegment.ChangeAttributesRequestNull, problem => new(problem.Severity, "NotFound", "Ten minste één attribuut moet opgegeven worden.") },
        {ProblemCode.RoadSegment.IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction, problem => new(problem.Severity, problem.Reason, $"Het wegsegment {problem.Parameters[0].Value} mag niet kruisen met wegsegment {problem.Parameters[1].Value}.") },
        {ProblemCode.RoadSegment.LowerMissing, problem => new(problem.Severity, problem.Reason, "Het onderste wegsegment ontbreekt.") },
        {ProblemCode.RoadSegment.Missing, problem => new(problem.Severity, problem.Reason, $"Het wegsegment met id {problem.Parameters[0].Value} ontbreekt.") },
        {ProblemCode.RoadSegment.NotFound, problem => new(problem.Severity, "NotFound", "Dit wegsegment bestaat niet.") },
        {ProblemCode.RoadSegment.OutlinedNotFound, problem => new(problem.Severity, "NotFound", "Dit wegsegment bestaat niet of heeft niet de geometriemethode 'ingeschetst'.") },
        {ProblemCode.RoadSegment.UpperAndLowerDoNotIntersect, problem => new(problem.Severity, problem.Reason, "Het bovenste en onderste wegsegment kruisen elkaar niet.") },
        {ProblemCode.RoadSegment.UpperMissing, problem => new(problem.Severity, problem.Reason, "Het bovenste wegsegment ontbreekt.") },
        {ProblemCode.RoadSegment.AccessRestriction.IsRequired, problem => new(problem.Severity, "ToegangsbeperkingVerplicht", "Toegangsbeperking is verplicht.") },
        {ProblemCode.RoadSegment.AccessRestriction.NotValid, problem => new(problem.Severity, "ToegangsbeperkingNietCorrect", $"Toegangsbeperking is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.Category.IsRequired, problem => new(problem.Severity, "WegcategorieVerplicht", "Wegcategorie is verplicht.") },
        {ProblemCode.RoadSegment.Category.NotValid, problem => new(problem.Severity, "WegcategorieNietCorrect", $"Wegcategorie is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.EndNode.Missing, problem => new(problem.Severity, problem.Reason, "De eind wegknoop van het wegsegment ontbreekt.") },
        {ProblemCode.RoadSegment.EndNode.RefersToRemovedNode, problem => new(problem.Severity, problem.Reason, $"De eind knoop van wegsegment {problem.Parameters[0].Value} verwijst naar een verwijderde knoop {problem.Parameters[1].Value}.") },
        {ProblemCode.RoadSegment.EndPoint.DoesNotMatchNodeGeometry, problem => new(problem.Severity, problem.Reason, "De positie van het eind punt van het wegsegment stemt niet overeen met de geometrie van de eind wegknoop.") },
        {ProblemCode.RoadSegment.EndPoint.MeasureValueNotEqualToLength, problem => new(problem.Severity, problem.Reason, $"De meting ({problem.Parameters[2].Value}) op het eind punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet gelijk aan de lengte ({problem.Parameters[3].Value}).") },
        {ProblemCode.RoadSegment.Geometry.IsRequired, problem => new(problem.Severity, "MiddellijnGeometrieVerplicht", "Middellijngeometrie is verplicht.") },
        {ProblemCode.RoadSegment.Geometry.NotValid, problem => new(problem.Severity, "MiddellijnGeometrieNietCorrect", "De opgegeven geometrie is geen geldige LineString in gml 3.2.") },
        {ProblemCode.RoadSegment.Geometry.LengthIsZero, problem => new(problem.Severity, problem.Reason, "De lengte van het wegsegment is 0.") },
        {ProblemCode.RoadSegment.Geometry.LengthLessThanMinimum, problem => new(problem.Severity, "MiddellijnGeometrieKorterDanMinimum", $"De opgegeven geometrie heeft niet de minimale lengte van {problem.Parameters[0].Value} meter.") },
        {ProblemCode.RoadSegment.Geometry.SelfIntersects, problem => new(problem.Severity, problem.Reason, "De geometrie van het wegsegment kruist zichzelf.") },
        {ProblemCode.RoadSegment.Geometry.SelfOverlaps, problem => new(problem.Severity, problem.Reason, "De geometrie van het wegsegment overlapt zichzelf.") },
        {ProblemCode.RoadSegment.Geometry.SridNotValid, problem => new(problem.Severity, "MiddellijnGeometrieCRSNietCorrect", "De opgegeven geometrie heeft niet het coördinatenstelsel Lambert 72.") },
        {ProblemCode.RoadSegment.Geometry.Taken, problem => new(problem.Severity, problem.Reason, $"De geometrie werd reeds ingenomen door een ander wegsegment met id {problem.Parameters[0].Value}.") },
        {ProblemCode.RoadSegment.GeometryDrawMethod.NotOutlined, problem => new(problem.Severity, "GeometriemethodeNietIngeschetst", "De geometriemethode van dit wegsegment komt niet overeen met 'ingeschetst'.") },
        {ProblemCode.RoadSegment.GeometryDrawMethod.NotValid, problem => new(problem.Severity, "GeometriemethodeNietCorrect", $"Geometriemethode is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.Lane.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason, $"De van positie ({problem.Parameters[1].Value}) van het eerste rijstroken attribuut met id {problem.Parameters[0].Value} is niet 0.0.") },
        {ProblemCode.RoadSegment.Lane.HasLengthOfZero, problem => new(problem.Severity, problem.Reason, $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} heeft lengte 0.") },
        {ProblemCode.RoadSegment.Lane.NotAdjacent, problem => new(problem.Severity, problem.Reason, $"De tot positie ({problem.Parameters[1].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het rijstroken attribuut met id {problem.Parameters[2].Value}.") },
        {ProblemCode.RoadSegment.Lane.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason, $"De tot positie ({problem.Parameters[1].Value}) van het laatste rijstroken attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).") },
        {ProblemCode.RoadSegment.Lane.IsRequired, problem => new(problem.Severity, "AantalRijstrokenVerplicht", "Aantal rijstroken is verplicht.")},
        {ProblemCode.RoadSegment.Lane.GreaterThanZero, problem => new(problem.Severity, "AantalRijstrokenGroterDanNul", "Aantal rijstroken moet groter dan nul zijn.")},
        {ProblemCode.RoadSegment.Lane.LessThanOrEqualToMaximum, problem => new(problem.Severity, "AantalRijstrokenKleinerOfGelijkAanMaximum", $"Aantal rijstroken mag niet groter dan {RoadSegmentLaneCount.Maximum} zijn.")},
        {ProblemCode.RoadSegment.Lanes.CountGreaterThanOne, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft meer dan 1 rijstrook.")},
        {ProblemCode.RoadSegment.Lanes.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkel rijstrook.")},
        {ProblemCode.RoadSegment.LaneDirection.IsRequired, problem => new(problem.Severity, "AantalRijstrokenRichtingVerplicht", "Aantal rijstroken richting is verplicht.")},
        {ProblemCode.RoadSegment.LaneDirection.NotValid, problem => new(problem.Severity, "AantalRijstrokenRichtingNietCorrect", $"Aantal rijstroken richting is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired, problem => new(problem.Severity, "WegbeheerderVerplicht", "Wegbeheerder is verplicht.") },
        {ProblemCode.RoadSegment.MaintenanceAuthority.NotValid, problem => new(problem.Severity, "WegbeheerderNietCorrect", $"Wegbeheerder is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.Morphology.NotValid, problem => new(problem.Severity, "MorfologischeWegklasseNietCorrect", $"Morfologische wegklasse is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.Morphology.IsRequired, problem => new(problem.Severity, "MorfologischeWegklasseVerplicht", "Morfologische wegklasse is verplicht.") },
        {ProblemCode.RoadSegment.Point.MeasureValueDoesNotIncrease, problem => new(problem.Severity, problem.Reason, $"De meting ({problem.Parameters[2].Value}) op het punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet groter dan de meting ({problem.Parameters[3].Value}) op het vorige punt.") },
        {ProblemCode.RoadSegment.Point.MeasureValueOutOfRange, problem => new(problem.Severity, problem.Reason, $"De meting ({problem.Parameters[2].Value}) op het punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] ligt niet binnen de verwachte grenzen [{problem.Parameters[3].Value}-{problem.Parameters[4].Value}].") },
        {ProblemCode.RoadSegment.StartNode.Missing, problem => new(problem.Severity, problem.Reason, "De start wegknoop van het wegsegment ontbreekt.") },
        {ProblemCode.RoadSegment.StartNode.RefersToRemovedNode, problem => new(problem.Severity, problem.Reason, $"De begin knoop van wegsegment {problem.Parameters[0].Value} verwijst naar een verwijderde knoop {problem.Parameters[1].Value}.") },
        {ProblemCode.RoadSegment.StartPoint.DoesNotMatchNodeGeometry, problem => new(problem.Severity, problem.Reason, "De positie van het start punt van het wegsegment stemt niet overeen met de geometrie van de start wegknoop.") },
        {ProblemCode.RoadSegment.StartPoint.MeasureValueNotEqualToZero, problem => new(problem.Severity, problem.Reason, $"De meting ({problem.Parameters[2].Value}) op het start punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet 0.0.") },
        {ProblemCode.RoadSegment.Status.NotValid, problem => new(problem.Severity, "WegsegmentStatusNietCorrect", $"Wegsegment status is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.Status.IsRequired, problem => new(problem.Severity, "WegsegmentStatusVerplicht", "Wegsegment status is verplicht.") },
        {ProblemCode.RoadSegment.StreetName.NotProposedOrCurrent, problem => new(problem.Severity, "WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.") },
        {ProblemCode.RoadSegment.StreetName.Left.NotLinked, problem => new(problem.Severity, "LinkerstraatnaamNietGekoppeld", $"Het wegsegment '{problem.Parameters[0].Value}' is niet gekoppeld aan de linkerstraatnaam '{problem.Parameters[1].Value}'") },
        {ProblemCode.RoadSegment.StreetName.Left.NotUnlinked, problem => new(problem.Severity, "LinkerstraatnaamNietOntkoppeld", $"Het wegsegment '{problem.Parameters[0].Value}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.") },
        {ProblemCode.RoadSegment.StreetName.Right.NotLinked, problem => new(problem.Severity, "RechterstraatnaamNietGekoppeld", $"Het wegsegment '{problem.Parameters[0].Value}' is niet gekoppeld aan de rechterstraatnaam '{problem.Parameters[1].Value}'") },
        {ProblemCode.RoadSegment.StreetName.Right.NotUnlinked, problem => new(problem.Severity, "RechterstraatnaamNietOntkoppeld", $"Het wegsegment '{problem.Parameters[0].Value}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.") },
        {ProblemCode.RoadSegment.Surface.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason, $"De van positie ({problem.Parameters[1].Value}) van het eerste wegverharding attribuut met id {problem.Parameters[0].Value} is niet 0.0.") },
        {ProblemCode.RoadSegment.Surface.HasLengthOfZero, problem => new(problem.Severity, problem.Reason, $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} heeft lengte 0.") },
        {ProblemCode.RoadSegment.Surface.NotAdjacent, problem => new(problem.Severity, problem.Reason, $"De tot positie ({problem.Parameters[1].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het wegverharding attribuut met id {problem.Parameters[2].Value}.") },
        {ProblemCode.RoadSegment.Surface.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason, $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegverharding attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).") },
        {ProblemCode.RoadSegment.Surfaces.CountGreaterThanOne, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft meer dan 1 wegverharding.")},
        {ProblemCode.RoadSegment.Surfaces.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele wegverharding.")},
        {ProblemCode.RoadSegment.SurfaceType.IsRequired, problem => new(problem.Severity, "WegverhardingVerplicht", "Wegverharding is verplicht.")},
        {ProblemCode.RoadSegment.SurfaceType.NotValid, problem => new(problem.Severity, "WegverhardingNietCorrect", $"Wegverharding is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.RoadSegment.Width.FromPositionNotEqualToZero, problem => new(problem.Severity, problem.Reason, $"De van positie ({problem.Parameters[1].Value}) van het eerste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet 0.0.") },
        {ProblemCode.RoadSegment.Width.HasLengthOfZero, problem => new(problem.Severity, problem.Reason, $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} heeft lengte 0.") },
        {ProblemCode.RoadSegment.Width.IsRequired, problem => new(problem.Severity, "WegbreedteVerplicht", "Wegbreedte is verplicht.") },
        {ProblemCode.RoadSegment.Width.NotAdjacent, problem => new(problem.Severity, problem.Reason, $"De tot positie ({problem.Parameters[1].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het wegbreedte attribuut met id {problem.Parameters[2].Value}.") },
        {ProblemCode.RoadSegment.Width.NotValid, problem => new(problem.Severity, "WegbreedteNietCorrect", $"Wegbreedte is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.") },
        {ProblemCode.RoadSegment.Width.ToPositionNotEqualToLength, problem => new(problem.Severity, problem.Reason, $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).") },
        {ProblemCode.RoadSegment.Width.LessThanOrEqualToMaximum, problem => new(problem.Severity, "WegbreedteKleinerOfGelijkAanMaximum", $"Wegbreedte mag niet groter dan {RoadSegmentWidth.Maximum} meter zijn.")},
        {ProblemCode.RoadSegment.Widths.CountGreaterThanOne, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft meer dan 1 wegbreedte.")},
        {ProblemCode.RoadSegment.Widths.HasCountOfZero, problem => new(problem.Severity, problem.Reason, "Wegsegment heeft geen enkele wegbreedte.")},
        {ProblemCode.RoadSegments.NotFound, problem => new(problem.Severity, "NotFound", $"Onbestaande of verwijderde wegsegmenten gevonden '{problem.Parameters[0].Value}'.") },
        {ProblemCode.ShapeFile.GeometrySridMustBeEqual, problem => new(problem.Severity, problem.Reason, "SRID van alle geometrieën moeten dezelfde zijn.")},
        {ProblemCode.ShapeFile.GeometryTypeMustBePolygon, problem => new(problem.Severity, problem.Reason, "Geometrie type moet een polygoon of multi polygoon zijn.")},
        {ProblemCode.ShapeFile.HasNoValidPolygons, problem => new(problem.Severity, problem.Reason, "Het shape bestand bevat geen geldige polygonen.")},
        {ProblemCode.ShapeFile.InvalidHeader, problem => new(problem.Severity, problem.Reason, $"Kan header van de shape file niet lezen: '{problem.Parameters[0].Value}'")},
        {ProblemCode.ShapeFile.InvalidPolygonShellOrientation, problem => new(problem.Severity, problem.Reason, "De orientatie van de polygoon moet in wijzerzin zijn.")},
        {ProblemCode.StreetName.NotFound, problem => new(problem.Severity, "StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.")},
        {ProblemCode.ToPosition.IsRequired, problem => new(problem.Severity, "TotPositieVerplicht", "TotPositie is verplicht.")},
        {ProblemCode.ToPosition.NotEqualToNextFromPosition, problem => new(problem.Severity, "TotPositieNietGelijkAanVolgendeVanPositie", $"De totPositie verschilt van de volgende vanPositie.")},
        {ProblemCode.ToPosition.NotValid, problem => new(problem.Severity, "TotPositieNietCorrect", $"TotPositie is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.Type.IsRequired, problem => new(problem.Severity, "TypeVerplicht", "Type is verplicht.")},
        {ProblemCode.Type.NotValid, problem => new(problem.Severity, "TypeNietCorrect", $"Type is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
        {ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce, problem => new(problem.Severity, "CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce", "Kan geen meerdere uploads uitvoeren voor hetzelfde extractaanvraag.")},
        {ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload, problem => new(problem.Severity, "CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload", "Upload is enkel toegelaten voor de laatste download van het extractaanvraag.")},
        {ProblemCode.Upload.UploadNotAllowedForInformativeExtract, problem => new(problem.Severity, "ExtractRequestMarkedInformative", "Upload is niet toegelaten voor een informatieve extractaanvraag.")},
        {ProblemCode.Width.IsRequired, problem => new(problem.Severity, "BreedteVerplicht", "Breedte is verplicht.")},
        {ProblemCode.Width.NotValid, problem => new(problem.Severity, "BreedteNietCorrect", $"Breedte is foutief. '{problem.Parameters[0].Value}' is geen geldige waarde.")},
    };
    
    public static readonly Converter<Problem, ProblemTranslation> Dutch = problem =>
    {
        var problemCode = ProblemCode.FromReason(problem.Reason);
        if (problemCode is not null && ProblemCodeDutchTranslators.TryGetValue(problemCode, out Converter<Problem, ProblemTranslation> converter))
        {
            return converter(problem);
        }

        return new ProblemTranslation(problem.Severity, problem.Reason, CreateMissingTranslationMessage(problemCode));
    };

    public static string CreateMissingTranslationMessage(ProblemCode problemCode)
    {
        return $"'{problemCode}' has no translation. Please fix it.";
    }

    static string GetRoadNodeTypeMismatch(Problem problem)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Het opgegeven wegknoop type {0} van knoop {1} komt niet overeen met een van de verwachte wegknoop types: ",
            RoadNodeType.Parse(problem.Parameters.Single(p => p.Name == "Actual").Value).Translation.Name,
            problem.Parameters.Single(p => p.Name == "RoadNodeId").Value);
        sb.AppendFormat("{0} ", string.Join(',', problem.Parameters
            .Where(p => p.Name == "Expected")
            .Select(parameter => RoadNodeType.Parse(parameter.Value).Translation.Name)));
        sb.AppendFormat("De wegknoop is verbonden met {0} wegsegment(-en)",
            problem.Parameters.Single(p => p.Name == "ConnectedSegmentCount").Value);
        if (problem.Parameters.Any(p => p.Name == "ConnectedSegmentId"))
        {
            sb.AppendFormat("{0} ", string.Join(',', problem.Parameters
                .Where(p => p.Name == "ConnectedSegmentId")
                .Select(parameter => parameter.Value)));
        }
        sb.Append(".");

        return sb.ToString();
    }
}
