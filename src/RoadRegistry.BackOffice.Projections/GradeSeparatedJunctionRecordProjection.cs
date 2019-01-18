namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Model;
    using Schema;
    using Schema.GradeSeparatedJunctions;

    public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;

        public GradeSeparatedJunctionRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedGradeSeparatedJunction>>((context, message, token) => HandleImportedGradeSeparatedJunction(context, message.Message, token));
        }

        private Task HandleImportedGradeSeparatedJunction(ShapeContext context, ImportedGradeSeparatedJunction @event, CancellationToken token)
        {
            var translation = GradeSeparatedJunctionType.Parse(@event.Type).Translation;
            var junctionRecord = new GradeSeparatedJunctionRecord
            {
                Id = @event.Id,
                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = {Value = @event.Id},
                    TYPE = {Value = translation.Identifier },
                    LBLTYPE = {Value = translation.Name },
                    BO_WS_OIDN = {Value = @event.UpperRoadSegmentId},
                    ON_WS_OIDN = {Value = @event.LowerRoadSegmentId},
                    BEGINTIJD = {Value = @event.Origin.Since},
                    BEGINORG = {Value = @event.Origin.OrganizationId},
                    LBLBGNORG = {Value = @event.Origin.Organization},
                }.ToBytes(_encoding)
            };

            return context.AddAsync(junctionRecord, token);
        }
    }
}
