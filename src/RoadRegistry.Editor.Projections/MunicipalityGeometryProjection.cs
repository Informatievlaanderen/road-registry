namespace RoadRegistry.Editor.Projections
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;

    public class MunicipalityGeometryProjection : ConnectedProjection<EditorContext>
    {
        public MunicipalityGeometryProjection()
        {
            When<Envelope<BackOffice.Messages.ImportedMunicipality>>(async (context, envelope, token) =>
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
