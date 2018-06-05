namespace RoadRegistry.Projections
{
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Events;

    public class RoadSegmentRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly IOrganisationRetreiver _organisationRetreiver;

        public RoadSegmentRecordProjection(IOrganisationRetreiver organisationRetreiver)
        {
            _organisationRetreiver = organisationRetreiver;
            When<ImportedRoadSegment>((context, @event, token) =>
            {
                return context.AddAsync(
                    new RoadSegmentRecord(),
                    token);
            });
        }
    }
}
