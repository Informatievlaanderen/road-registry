namespace RoadRegistry.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using Microsoft.EntityFrameworkCore;

    public class RoadRegistryImportStateProjection : ConnectedProjection<ShapeContext>
    {
        public RoadRegistryImportStateProjection()
        {
            When<Envelope<ImportLegacyRegistryStarted>>((content, message, token) => HandleImportedStarted(content, token));
            When<Envelope<ImportLegacyRegistryFinished>>((content, message, token) => HandleImportedFinishedAsync(content, token));
        }

        private Task HandleImportedStarted(ShapeContext content, CancellationToken token)
        {
            return content.AddAsync(new RoadRegistryImportStateRecord(), token);
        }

        private async Task HandleImportedFinishedAsync(ShapeContext content, CancellationToken token)
        {
            var state = await content.RoadRegistryImportStates.SingleAsync(token);
            state.ImportComplete = true;
        }
    }
}
