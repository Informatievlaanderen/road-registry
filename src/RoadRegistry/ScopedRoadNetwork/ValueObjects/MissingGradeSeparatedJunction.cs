namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

// What to make of two road segments that cross away from a road node, with traffic types that overlap at the
// crossing, and no ongelijkgrondse kruising between them.
public enum MissingGradeSeparatedJunction
{
    // The inwinning and the editing actions: the crossing has to be either a road node or an ongelijkgrondse
    // kruising, so the change is refused.
    Error,

    // The bijhouding upload. Filling in traffic types is not part of what a GRB dienstverlener is asked to do, and
    // they may leave them unknown (-8) on what they draw, so the bijhouding neither refuses such a crossing nor reports
    // it - not here and not in the feature compare.
    Allowed
}
