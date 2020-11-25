namespace RoadRegistry.Editor.Projections
{
    using BackOffice.Core;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class MunicipalityGeometryRecordProjection : ConnectedProjection<EditorContext>
    {
        public MunicipalityGeometryRecordProjection()
        {
            When<Envelope<ImportedMunicipality>>(async (context, envelope, token) =>
            {
                await context.MunicipalityGeometries.AddAsync(new MunicipalityGeometryRecord
                {
                    NisCode = envelope.Message.NISCode,
                    Geometry = GeometryTranslator.Translate(envelope.Message.Geometry)
                });
            });
        }
    }
}
