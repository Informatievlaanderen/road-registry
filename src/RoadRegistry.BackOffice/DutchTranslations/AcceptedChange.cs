namespace RoadRegistry.BackOffice.DutchTranslations;

using System;
using RoadRegistry.BackOffice.Messages;

public static class AcceptedChange
{
    public static readonly Converter<BackOffice.Messages.AcceptedChange, string> Translator =
        change =>
        {
            string translation = change.Flatten() switch
            {
                RoadNodeAdded m => $"Wegknoop {m.TemporaryId} toegevoegd met id {m.Id}.",
                RoadNodeModified m => $"Wegknoop {m.Id} gewijzigd.",
                RoadNodeRemoved m => $"Wegknoop {m.Id} verwijderd.",
                RoadSegmentAdded m => $"Wegsegment {m.OriginalId ?? m.TemporaryId} toegevoegd met id {m.Id}.",
                RoadSegmentModified m => $"Wegsegment {m.Id} gewijzigd.",
                RoadSegmentAttributesModified m => $"Wegsegment {m.Id} attributen gewijzigd.",
                RoadSegmentGeometryModified m => $"Wegsegment {m.Id} geometrie gewijzigd.",
                RoadSegmentRemoved m => $"Wegsegment {m.Id} verwijderd.",
                RoadSegmentAddedToEuropeanRoad m => $"Wegsegment {m.SegmentId} toegevoegd aan europese weg {m.Number}.",
                RoadSegmentRemovedFromEuropeanRoad m => $"Wegsegment {m.SegmentId} verwijderd van europese weg {m.Number}.",
                RoadSegmentAddedToNationalRoad m => $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Number}.",
                RoadSegmentRemovedFromNationalRoad m => $"Wegsegment {m.SegmentId} verwijderd van nationale weg {m.Number}.",
                RoadSegmentAddedToNumberedRoad m => $"Wegsegment {m.SegmentId} toegevoegd aan genummerde weg {m.Number}.",
                RoadSegmentOnNumberedRoadModified m => $"Wegsegment {m.SegmentId} gewijzigd op genummerde weg {m.Number}.",
                RoadSegmentRemovedFromNumberedRoad m => $"Wegsegment {m.SegmentId} verwijderd van genummerde weg {m.Number}.",
                GradeSeparatedJunctionAdded m => $"Ongelijkgrondse kruising {m.TemporaryId} toegevoegd met id {m.Id}.",
                GradeSeparatedJunctionModified m => $"Ongelijkgrondse kruising {m.Id} gewijzigd.",
                GradeSeparatedJunctionRemoved m => $"Ongelijkgrondse kruising {m.Id} verwijderd.",
                _ => $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.",
            };
            return translation;
        };
}
