namespace RoadRegistry.Editor.Projections
{
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;
    using MunicipalityGeometry = Schema.MunicipalityGeometry;

    public class MunicipalityGeometryProjection : ConnectedProjection<EditorContext>
    {
        public MunicipalityGeometryProjection()
        {
            When<Envelope<ImportedMunicipality>>(async (context, envelope, token) =>
            {
                await context.MunicipalityGeometries.AddAsync(new MunicipalityGeometry
                {
                    NisCode = envelope.Message.NISCode,
                    Geometry = GeometryTranslator.Translate(envelope.Message.Geometry)
                });
            });
        }
    }
}
