namespace RoadRegistry.Editor.Projections.DutchTranslations
{
    using System;
    using BackOffice.Messages;

    public static class RejectedChange
    {
        public static readonly Converter<BackOffice.Messages.RejectedChange, string> Translator =
            change =>
            {
                string translation;
                switch (change.Flatten())
                {
                    case AddRoadNode m:
                        translation = $"Voeg wegknoop {m.TemporaryId} toe.";
                        break;
                    case ModifyRoadNode m:
                        translation = $"Wijzig wegknoop {m.Id}.";
                        break;
                    case RemoveRoadNode m:
                        translation = $"Verwijder wegknoop {m.Id}.";
                        break;
                    case AddRoadSegment m:
                        translation = $"Voeg wegsegment {m.TemporaryId} toe.";
                        break;
                    case ModifyRoadSegment m:
                        translation = $"Wijzig wegsegment {m.Id}.";
                        break;
                    case RemoveRoadSegment m:
                        translation = $"Verwijder wegsegment {m.Id}.";
                        break;
                    case AddRoadSegmentToEuropeanRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan europese weg {m.Number}.";
                        break;
                    case RemoveRoadSegmentFromEuropeanRoad m:
                        translation = $"Verwijder wegsegment {m.SegmentId} van europese weg {m.Number}.";
                        break;
                    case AddRoadSegmentToNationalRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Number}.";
                        break;
                    case RemoveRoadSegmentFromNationalRoad m:
                        translation = $"Verwijder wegsegment {m.SegmentId} van nationale weg {m.Number}.";
                        break;
                    case AddRoadSegmentToNumberedRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan genummerde weg {m.Number}.";
                        break;
                    case ModifyRoadSegmentOnNumberedRoad m:
                        translation = $"Wijzig wegsegment {m.SegmentId} op genummerde weg {m.Number}.";
                        break;
                    case RemoveRoadSegmentFromNumberedRoad m:
                        translation = $"Verwijder wegsegment {m.SegmentId} van genummerde weg {m.Number}.";
                        break;
                    case AddGradeSeparatedJunction m:
                        translation = $"Voeg ongelijkgrondse kruising {m.TemporaryId} toe.";
                        break;
                    case ModifyGradeSeparatedJunction m:
                        translation = $"Wijzig ongelijkgrondse kruising {m.Id}.";
                        break;
                    case RemoveGradeSeparatedJunction m:
                        translation = $"Verwijder ongelijkgrondse kruising {m.Id}.";
                        break;

                    default:
                        translation = $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };
    }
}
