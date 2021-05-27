namespace RoadRegistry.Editor.Projections.DutchTranslations
{
    using System;
    using System.Linq;
    using BackOffice;
    using BackOffice.Core;

    public static class ProblemWithChange
    {
        public static readonly Converter<BackOffice.Messages.Problem, string> Translator =
            problem =>
            {
                string translation;
                switch (problem.Reason)
                {
                    case nameof(RoadNodeNotFound):
                        translation = "De wegknoop is niet langer onderdeel van het wegen netwerk.";
                        break;
                    case nameof(RoadSegmentNotFound):
                        translation = "Het wegsegment is niet langer onderdeel van het wegen netwerk.";
                        break;
                    case nameof(GradeSeparatedJunctionNotFound):
                        translation = "De ongelijkgrondse kruising is niet langer onderdeel van het wegen netwerk.";
                        break;
                    case nameof(EuropeanRoadNumberNotFound):
                        translation = $"Het wegsegment is reeds geen onderdeel meer van deze europese weg met nummer {problem.Parameters[0].Value}.";
                        break;
                    case nameof(NationalRoadNumberNotFound):
                        translation = $"Het wegsegment is reeds geen onderdeel meer van deze nationale weg met nummer {problem.Parameters[0].Value}.";
                        break;
                    case nameof(NumberedRoadNumberNotFound):
                        translation = $"Het wegsegment is reeds geen onderdeel meer van deze genummerde weg met nummer {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadNodeGeometryTaken):
                        translation = $"De geometrie werd reeds ingenomen door een andere wegknoop met id {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadNodeTooClose):
                        translation = $"De geometrie ligt te dicht bij wegsegment met id {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadNodeNotConnectedToAnySegment):
                        translation = "De wegknoop is met geen enkel wegsegment verbonden.";
                        break;
                    case nameof(RoadNodeTypeMismatch):
                        translation = $"Het opgegeven wegknoop type {RoadNodeType.Parse(problem.Parameters.Single(p => p.Name == "Actual").Value).Translation.Name} van knoop {problem.Parameters.Single(p => p.Name == "RoadNodeId").Value} komt niet overeen met een van de verwachte wegknoop types: {string.Join(',', problem.Parameters.Where(p => p.Name == "Expected").Select(parameter => RoadNodeType.Parse(parameter.Value).Translation.Name))}. De wegknoop is verbonden met {problem.Parameters[1].Value} wegsegment(-en)";
                        if (problem.Parameters.Any(p => p.Name == "ConnectedSegmentId"))
                        {
                            translation += $": {string.Join(',', problem.Parameters.Where(p => p.Name == "ConnectedSegmentId").Select(parameter => parameter.Value))}.";
                        }
                        else
                        {
                            translation += ".";
                        }

                        break;
                    case nameof(FakeRoadNodeConnectedSegmentsDoNotDiffer):
                        translation = $"De attributen van de verbonden wegsegmenten ({problem.Parameters[1].Value} en {problem.Parameters[2].Value}) verschillen onvoldoende voor deze schijnknoop.";
                        break;
                    case nameof(RoadSegmentGeometryTaken):
                        translation = $"De geometrie werd reeds ingenomen door een ander wegsegment met id {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadSegmentStartNodeMissing):
                        translation = "De start wegknoop van het wegsegment ontbreekt.";
                        break;
                    case nameof(RoadSegmentEndNodeMissing):
                        translation = "De eind wegknoop van het wegsegment ontbreekt.";
                        break;
                    case nameof(RoadSegmentGeometryLengthIsZero):
                        translation = "De lengte van het wegsegment is 0.";
                        break;
                    case nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry):
                        translation = "De positie van het start punt van het wegsegment stemt niet overeen met de geometrie van de start wegknoop.";
                        break;
                    case nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry):
                        translation = "De positie van het eind punt van het wegsegment stemt niet overeen met de geometrie van de eind wegknoop.";
                        break;
                    case nameof(RoadSegmentGeometrySelfOverlaps):
                        translation = "De geometrie van het wegsegment overlapt zichzelf.";
                        break;
                    case nameof(RoadSegmentGeometrySelfIntersects):
                        translation = "De geometrie van het wegsegment kruist zichzelf.";
                        break;
                    case nameof(RoadSegmentMissing):
                        translation = $"Het wegsegment met id {problem.Parameters[0].Value} ontbreekt.";
                        break;
                    case nameof(RoadSegmentLaneAttributeFromPositionNotEqualToZero):
                        translation = $"De van positie ({problem.Parameters[2].Value}) van het eerste rijstroken attribuut met id {problem.Parameters[0].Value} is niet 0.0.";
                        break;
                    case nameof(RoadSegmentLaneAttributesNotAdjacent):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het rijstroken attribuut met id {problem.Parameters[2].Value}.";
                        break;
                    case nameof(RoadSegmentLaneAttributeToPositionNotEqualToLength):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het laatste rijstroken attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).";
                        break;
                    case nameof(RoadSegmentLaneAttributeHasLengthOfZero):
                        translation = $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} heeft lengte 0.";
                        break;
                    case nameof(RoadSegmentWidthAttributeFromPositionNotEqualToZero):
                        translation = $"De van positie ({problem.Parameters[1].Value}) van het eerste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet 0.0.";
                        break;
                    case nameof(RoadSegmentWidthAttributesNotAdjacent):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het wegbreedte attribuut met id {problem.Parameters[2].Value}.";
                        break;
                    case nameof(RoadSegmentWidthAttributeToPositionNotEqualToLength):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).";
                        break;
                    case nameof(RoadSegmentWidthAttributeHasLengthOfZero):
                        translation = $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} heeft lengte 0.";
                        break;
                    case nameof(RoadSegmentSurfaceAttributeFromPositionNotEqualToZero):
                        translation = $"De van positie ({problem.Parameters[2].Value}) van het eerste wegverharding attribuut met id {problem.Parameters[0].Value} is niet 0.0.";
                        break;
                    case nameof(RoadSegmentSurfaceAttributesNotAdjacent):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het wegverharding attribuut met id {problem.Parameters[2].Value}.";
                        break;
                    case nameof(RoadSegmentSurfaceAttributeToPositionNotEqualToLength):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegverharding attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).";
                        break;
                    case nameof(RoadSegmentSurfaceAttributeHasLengthOfZero):
                        translation = $"De van ({problem.Parameters[1].Value}) en tot positie ({problem.Parameters[2].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} heeft lengte 0.";
                        break;
                    case nameof(RoadSegmentPointMeasureValueOutOfRange):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] ligt niet binnen de verwachte grenzen [{problem.Parameters[3].Value}-{problem.Parameters[4].Value}].";
                        break;
                    case nameof(RoadSegmentStartPointMeasureValueNotEqualToZero):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het start punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet 0.0.";
                        break;
                    case nameof(RoadSegmentEndPointMeasureValueNotEqualToLength):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het eind punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet gelijk aan de lengte ({problem.Parameters[3].Value}).";
                        break;
                    case nameof(RoadSegmentPointMeasureValueDoesNotIncrease):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet groter dan de meting ({problem.Parameters[3].Value}) op het vorige punt.";
                        break;
                    case nameof(UpperRoadSegmentMissing):
                        translation = "Het bovenste wegsegment ontbreekt.";
                        break;
                    case nameof(LowerRoadSegmentMissing):
                        translation = "Het onderste wegsegment ontbreekt.";
                        break;
                    case nameof(UpperAndLowerRoadSegmentDoNotIntersect):
                        translation = "Het bovenste en onderste wegsegment kruisen elkaar niet.";
                        break;
                    case nameof(RoadSegmentStartNodeRefersToRemovedNode):
                        translation = $"De begin knoop van wegsegment {problem.Parameters[0].Value} verwijst naar een verwijderde knoop {problem.Parameters[1].Value}.";
                        break;
                    case nameof(RoadSegmentEndNodeRefersToRemovedNode):
                        translation = $"De eind knoop van wegsegment {problem.Parameters[0].Value} verwijst naar een verwijderde knoop {problem.Parameters[1].Value}.";
                        break;
                    default:
                        translation = $"'{problem.Reason}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };
    }
}
