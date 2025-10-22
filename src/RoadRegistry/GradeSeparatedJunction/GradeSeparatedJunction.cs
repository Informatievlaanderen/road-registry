namespace RoadRegistry.GradeSeparatedJunction;

using System;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using RoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction : AggregateRootEntity
{
    public Problems VerifyWithinRoadNetwork(RoadNetworkChangeContext context)
    {
        //TODO-pr implement verifyafter logic
        throw new NotImplementedException();
    }
}
