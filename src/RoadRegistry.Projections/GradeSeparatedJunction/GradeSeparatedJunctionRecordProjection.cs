namespace RoadRegistry.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly GradeSeparatedJunctionTypeTranslator _typeTranslator;

        public GradeSeparatedJunctionRecordProjection(GradeSeparatedJunctionTypeTranslator typeTranslator)
        {
            _typeTranslator = typeTranslator;

            When<Envelope<ImportedGradeSeparatedJunction>>((context, message, token) => HandleImportedGradeSeperatedJunction(context, message.Message, token));
        }

        private Task HandleImportedGradeSeperatedJunction(ShapeContext context, ImportedGradeSeparatedJunction @event, CancellationToken token)
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
                }.ToBytes()
            };

            return context.AddAsync(junctionRecord, token);

            throw new System.NotImplementedException();
        }
    }
}
