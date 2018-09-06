namespace RoadRegistry.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using Shared;

    public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly GradeSeparatedJunctionTypeTranslator _typeTranslator;
        private readonly Encoding _encoding;

        public GradeSeparatedJunctionRecordProjection(GradeSeparatedJunctionTypeTranslator typeTranslator, Encoding encoding)
        {
            _typeTranslator = typeTranslator ?? throw new ArgumentNullException(nameof(typeTranslator));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedGradeSeparatedJunction>>((context, message, token) => HandleImportedGradeSeparatedJunction(context, message.Message, token));
        }

        private Task HandleImportedGradeSeparatedJunction(ShapeContext context, ImportedGradeSeparatedJunction @event, CancellationToken token)
        {
            var junctionRecord = new GradeSeparatedJunctionRecord
            {
                Id = @event.Id,
                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = {Value = @event.Id},
                    TYPE = {Value = _typeTranslator.TranslateToIdentifier(@event.Type)},
                    LBLTYPE = {Value = _typeTranslator.TranslateToDutchName(@event.Type)},
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
