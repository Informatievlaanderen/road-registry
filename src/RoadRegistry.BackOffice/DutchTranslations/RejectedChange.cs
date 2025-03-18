namespace RoadRegistry.BackOffice.DutchTranslations;

using System;
using Messages;

public static class RejectedChange
{
    public static readonly Converter<BackOffice.Messages.RejectedChange, string> Translator = change => change.Flatten() switch
    {
        AddRoadNode m => $"Voeg wegknoop {m.OriginalId ?? m.TemporaryId} toe.",
        ModifyRoadNode m => $"Wijzig wegknoop {m.Id}.",
        RemoveRoadNode m => $"Verwijder wegknoop {m.Id}.",
        AddRoadSegment m => $"Voeg wegsegment {m.OriginalId ?? m.TemporaryId} toe.",
        ModifyRoadSegment m => $"Wijzig wegsegment {m.Id}{(m.OriginalId is not null && m.OriginalId.Value != m.Id ? $" met aangeleverde identificator {m.OriginalId}" : "")}.",
        ModifyRoadSegmentAttributes m => $"Wijzig wegsegment attributen {m.Id}.",
        ModifyRoadSegmentGeometry m => $"Wijzig wegsegment geometrie {m.Id}.",
        RemoveRoadSegment m => $"Verwijder wegsegment {m.Id}.",
        RemoveRoadSegments m => $"Verwijder wegsegmenten: {string.Join(",", m.Ids)}.",
        RemoveOutlinedRoadSegment m => $"Verwijder ingeschetst wegsegment {m.Id}.",
        RemoveOutlinedRoadSegmentFromRoadNetwork m => $"Verwijder ingeschetst wegsegment {m.Id} uit het wegennetwerk.",
        AddRoadSegmentToEuropeanRoad m => $"Voeg wegsegment {m.SegmentId} toe aan europese weg {m.Number}.",
        RemoveRoadSegmentFromEuropeanRoad m => $"Verwijder wegsegment {m.SegmentId} van europese weg {m.Number}.",
        AddRoadSegmentToNationalRoad m => $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Number}.",
        RemoveRoadSegmentFromNationalRoad m => $"Verwijder wegsegment {m.SegmentId} van nationale weg {m.Number}.",
        AddRoadSegmentToNumberedRoad m => $"Voeg wegsegment {m.SegmentId} toe aan genummerde weg {m.Number}.",
        RemoveRoadSegmentFromNumberedRoad m => $"Verwijder wegsegment {m.SegmentId} van genummerde weg {m.Number}.",
        AddGradeSeparatedJunction m => $"Voeg ongelijkgrondse kruising {m.TemporaryId} toe.",
        ModifyGradeSeparatedJunction m => $"Wijzig ongelijkgrondse kruising {m.Id}.",
        RemoveGradeSeparatedJunction m => $"Verwijder ongelijkgrondse kruising {m.Id}.",
        _ => $"'{change.Flatten().GetType().Name}' has no translation. Please fix it."
    };
}
