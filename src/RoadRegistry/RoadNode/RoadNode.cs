namespace RoadRegistry.RoadNode;

using System;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using RoadNetwork.ValueObjects;

public partial class RoadNode : AggregateRootEntity
{
    public Problems VerifyWithinRoadNetwork(RoadNetworkChangeContext context)
    {
        //TODO-pr implement verifyafter logic
        throw new NotImplementedException();
    }
}
