namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Text;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Schema;
    using Schema.RoadSegmentDenorm;
    using Projac.Sql;
    using Projac.SqlClient;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.Threading.Tasks;

    public class RoadSegmentRecordProjection2 : SqlProjection
    {
        private static readonly SqlClientSyntax TSql = new SqlClientSyntax();

        public RoadSegmentRecordProjection2()
        {
            When<Envelope<ImportedRoadSegment>>(envelope =>
            {
                return TSql.NonQueryStatement(
                    "INSERT INTO [RoadRegistryWms].[RoadSegmentDenormRecord] ([Id], [Geometry]) VALUES (@P0, @P1)",
                    new {
                        P1 = TSql.Int(envelope.Message.Id),
                        P2 = TSql.VarBinaryMax(SqlGeometryTranslator.TranslateToSqlGeometry(envelope.Message.Geometry))
                    });
            });
        }

        //TODO: This method could be called like you would do for migrations, in Program.cs, using a sql connection that can do DDL.
        public static async Task CreateSchemaIfNotExists(SqlConnection connection)
        {
            var text = $@"
                IF NOT EXISTS (SELECT * FROM SYS.SCHEMAS WHERE [Name] = '{WellknownSchemas.WmsSchema}')
                BEGIN
                    EXEC('CREATE SCHEMA [{WellknownSchemas.WmsSchema}] AUTHORIZATION [dbo]')
                END
                IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE [Name] = 'RoadSegmentDenormRecord' AND [Type] = 'U' AND [Schema_ID] = SCHEMA_ID('{WellknownSchemas.WmsSchema}'))
                BEGIN
                    CREATE TABLE [{WellknownSchemas.WmsSchema}].[RoadSegmentDenormRecord]
                    (
                        [Id]                                       INT                NOT NULL,
                        [Geometry]                                 GEOMETRY           NOT NULL,
                        CONSTRAINT [PK_RoadSegmentDenormRecord]    PRIMARY KEY        NONCLUSTERED ([Id])
                    )
                END";

            using(var command = connection.CreateCommand())
            {
                command.CommandText = text;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30; // point of configuration, maybe
                await command.ExecuteNonQueryAsync();
            }
        }
    }


    public class RoadSegmentRecordProjection : ConnectedProjection<WmsContext>
    {
        public RoadSegmentRecordProjection(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var roadSegmentGeometryDrawMethod =
                    RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);

                var accessRestriction =
                    RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);

                var roadSegmentStatus = RoadSegmentStatus.Parse(envelope.Message.Status);

                var roadSegmentMorphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);

                var roadSegmentCategory = RoadSegmentCategory.Parse(envelope.Message.Category);

            //     await context.RoadSegments.AddAsync(new RoadSegmentDenormRecord
            //     {
            //         Id = envelope.Message.Id,
            //         BeginOperator = OrganizationId.Unknown,
            //         BeginOrganization = envelope.Message.Origin.OrganizationId,
            //         BeginTime = envelope.Message.Origin.Since,
            //         BeginApplication = "-8",
            //
            //         Maintainer = envelope.Message.MaintenanceAuthority.Code,
            //         MaintainerLabel = envelope.Message.MaintenanceAuthority.Name,
            //
            //         Method = roadSegmentGeometryDrawMethod.Translation.Identifier,
            //         MethodLabel = roadSegmentGeometryDrawMethod.Translation.Name,
            //
            //         Category = roadSegmentCategory.Translation.Identifier,
            //         CategoryLabel = roadSegmentCategory.Translation.Name,
            //
            //         Geometry = GeometryTranslator.Translate(envelope.Message.Geometry),
            //         Geometry2D = null,
            //         GeometryVersion = envelope.Message.GeometryVersion,
            //
            //         Morphology = roadSegmentMorphology.Translation.Identifier,
            //         MorphologyLabel = roadSegmentMorphology.Translation.Name,
            //
            //         Status = roadSegmentStatus.Translation.Identifier,
            //         StatusLabel = roadSegmentStatus.Translation.Name,
            //
            //         AccessRestriction = accessRestriction.Translation.Identifier,
            //         AccessRestrictionLabel = accessRestriction.Translation.Name,
            //
            //         SourceId = null,
            //         SourceIdSource = null,
            //
            //         OrganizationLabel = envelope.Message.Origin.Organization,
            //         RecordingDate = envelope.Message.RecordingDate,
            //         TransactionId = 0,
            //
            //         LeftSideMunicipality = 0,
            //         LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
            //         LeftSideStreetNameLabel = string.IsNullOrWhiteSpace(envelope.Message.LeftSide.StreetName)
            //             ? null
            //             : envelope.Message.LeftSide.StreetName,
            //
            //         RightSideMunicipality = 0,
            //         RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
            //         RightSideStreetNameLabel = string.IsNullOrWhiteSpace(envelope.Message.RightSide.StreetName)
            //             ? null
            //             : envelope.Message.RightSide.StreetName,
            //
            //         RoadSegmentVersion = envelope.Message.Version,
            //
            //         BeginRoadNodeId = envelope.Message.StartNodeId,
            //         EndRoadNodeId = envelope.Message.EndNodeId
            //     }, token);
            });
        }
    }
}
