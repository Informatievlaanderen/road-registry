namespace RoadRegistry.Editor.Projections.DutchTranslations
{
    using System;
    using BackOffice.Messages;

    public static class AcceptedChange
    {
        public static readonly Converter<BackOffice.Messages.AcceptedChange, string> Translator =
            change =>
            {
                string translation;
                switch (change.Flatten())
                {
                    case RoadNodeAdded m:
                        translation = $"Wegknoop {m.TemporaryId} toegevoegd.";
                        break;
                    case RoadNodeModified m:
                        translation = $"Wegknoop {m.Id} gewijzigd.";
                        break;
                    case RoadNodeRemoved m:
                        translation = $"Wegknoop {m.Id} verwijderd.";
                        break;
                    case RoadSegmentAdded m:
                        translation = $"Wegsegment {m.TemporaryId} toegevoegd.";
                        break;
                    case RoadSegmentModified m:
                        translation = $"Wegsegment {m.Id} gewijzigd.";
                        break;
                    case RoadSegmentRemoved m:
                        translation = $"Wegsegment {m.Id} verwijderd.";
                        break;
                    case RoadSegmentAddedToEuropeanRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan europese weg {m.Number}.";
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad m:
                        translation = $"Wegsegment {m.SegmentId} verwijderd van europese weg {m.Number}.";
                        break;
                    case RoadSegmentAddedToNationalRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Number}.";
                        break;
                    case RoadSegmentRemovedFromNationalRoad m:
                        translation = $"Wegsegment {m.SegmentId} verwijderd van nationale weg {m.Number}.";
                        break;
                    case RoadSegmentAddedToNumberedRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Number}.";
                        break;
                    case RoadSegmentOnNumberedRoadModified m:
                        translation = $"Wegsegment {m.SegmentId} gewijzigd op genummerde weg {m.Number}.";
                        break;
                    case RoadSegmentRemovedFromNumberedRoad m:
                        translation = $"Wegsegment {m.SegmentId} verwijderd van genummerde weg {m.Number}.";
                        break;
                    case GradeSeparatedJunctionAdded m:
                        translation = $"Ongelijkgrondse kruising {m.TemporaryId} toegevoegd.";
                        break;
                    case GradeSeparatedJunctionModified m:
                        translation = $"Ongelijkgrondse kruising {m.Id} gewijzigd.";
                        break;
                    case GradeSeparatedJunctionRemoved m:
                        translation = $"Ongelijkgrondse kruising {m.Id} verwijderd.";
                        break;

                    default:
                        translation = $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };
    }
}
