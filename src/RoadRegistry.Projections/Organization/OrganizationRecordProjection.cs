namespace RoadRegistry.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class OrganizationRecordProjection : ConnectedProjection<ShapeContext>
    {
        public OrganizationRecordProjection()
        {
            When<Envelope<ImportedOrganization>>((content, message, token) => HandleImportedOrganization(content, message.Message, token));
        }

        private Task HandleImportedOrganization(ShapeContext content, ImportedOrganization @event, CancellationToken token)
        {
            var organization = new OrganizationRecord
            {
                Code = @event.Code,
                DbaseRecord = new OrganizationDbaseRecord
                {
                    ORG = { Value = @event.Code },
                    LBLORG = { Value = @event.Name },
                }.ToBytes()
            };

            return content.AddAsync(organization, token);
        }
    }
}
