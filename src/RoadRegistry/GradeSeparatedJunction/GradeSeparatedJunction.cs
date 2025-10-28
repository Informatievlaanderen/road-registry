namespace RoadRegistry.GradeSeparatedJunction;

using System;
using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using RoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public Problems VerifyTopologyAfterChanges(RoadNetworkChangeContext context)
    {
        //TODO-pr implement verifyafter logic
        throw new NotImplementedException();
    }
}
