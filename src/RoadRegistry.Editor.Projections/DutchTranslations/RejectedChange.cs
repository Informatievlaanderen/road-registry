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
                    case BackOffice.Messages.AddRoadNode m:
                        translation = $"Voeg wegknoop {m.TemporaryId} toe.";
                        break;
                    case BackOffice.Messages.ModifyRoadNode m:
                        translation = $"Wijzig wegknoop {m.Id}.";
                        break;
                    case BackOffice.Messages.RemoveRoadNode m:
                        translation = $"Verwijder wegknoop {m.Id}.";
                        break;
                    case BackOffice.Messages.AddRoadSegment m:
                        translation = $"Voeg wegsegment {m.TemporaryId} toe.";
                        break;
                    case BackOffice.Messages.ModifyRoadSegment m:
                        translation = $"Wijzig wegsegment {m.Id}.";
                        break;
                    case BackOffice.Messages.RemoveRoadSegment m:
                        translation = $"Verwijder wegsegment {m.Id}.";
                        break;
                    case BackOffice.Messages.AddRoadSegmentToEuropeanRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan europese weg {m.Number}.";
                        break;
                    case BackOffice.Messages.RemoveRoadSegmentFromEuropeanRoad m:
                        translation = $"Verwijder wegsegment {m.SegmentId} van europese weg {m.Number}.";
                        break;
                    case BackOffice.Messages.AddRoadSegmentToNationalRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Number}.";
                        break;
                    case BackOffice.Messages.RemoveRoadSegmentFromNationalRoad m:
                        translation = $"Verwijder wegsegment {m.SegmentId} van nationale weg {m.Number}.";
                        break;
                    case BackOffice.Messages.AddRoadSegmentToNumberedRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Number}.";
                        break;
                    case BackOffice.Messages.RemoveRoadSegmentFromNumberedRoad m:
                        translation = $"Verwijder wegsegment {m.SegmentId} van nationale weg {m.Number}.";
                        break;
                    case BackOffice.Messages.AddGradeSeparatedJunction m:
                        translation = $"Voeg ongelijkgrondse kruising {m.TemporaryId} toe.";
                        break;
                    case BackOffice.Messages.RemoveGradeSeparatedJunction m:
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