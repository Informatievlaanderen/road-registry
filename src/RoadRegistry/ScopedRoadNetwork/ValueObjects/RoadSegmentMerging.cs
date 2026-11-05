namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

// Whether a road node that turns out not to be needed any more may take the two road segments hanging off it with it,
// merging them into one - and, where it may, under whose rules the two are allowed to become one road.
public enum RoadSegmentMerging
{
    // An action that names one road segment and has to leave the roads around it as they are. Historeren is one: the
    // node it leaves holding two roads becomes the validatieknoop between them and nothing else moves.
    NotAllowed,

    // The inwinning, which reshapes a municipality as a whole. The roads are handed over as they were drawn, so two
    // that were not drawn the same way still become one and the merged road's geometriemethode is derived from the
    // two it is made of, the same as its other attributes.
    //
    // Not the bijhouding upload, which goes through the same change flow but merges nothing: see
    // ScopedRoadNetwork.AfterChangesApplied.
    AsInwinning,

    // An editing action in which the merge is a consequence rather than the request: removing a road segment can
    // leave a node holding two roads that is no longer needed. What those two roads are is already settled here, and
    // a road that would be part ingeschetst and part ingemeten is not something the register may make of them on its
    // own - VAL-5 refuses it instead of deciding for the caller (GAWR-7829).
    AsEditAction
}
