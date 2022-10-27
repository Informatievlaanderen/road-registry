namespace RoadRegistry.Legacy.Extract.Readers;

using System;
using System.Collections.Generic;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using Point = NetTopologySuite.Geometries.Point;

public class ImportedRoadNodesReader : IEventReader
{
    private readonly IClock _clock;
    private readonly ILogger<ImportedRoadNodesReader> _logger;
    private readonly WellKnownBinaryReader _wkbReader;

    public ImportedRoadNodesReader(IClock clock, WellKnownBinaryReader wkbReader, ILogger<ImportedRoadNodesReader> logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        return new SqlCommand(
            @"SELECT wk.[wegknoopID]
                            ,wk.[wegknoopversie]
                            ,wk.[type]
                            ,wk.[geometrie].AsBinaryZM()
                            ,wk.[beginorganisatie]
                            ,lo.[label]
                            ,wk.[beginoperator]
                            ,wk.[beginapplicatie]
                            ,wk.[begintijd]
                            ,wk.[transactieid]
                        FROM [dbo].[wegknoop] wk
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wk.[beginorganisatie] = lo.[code]", connection
        ).YieldEachDataRecord(reader =>
        {
            var id = reader.GetInt32(0);
            _logger.LogDebug("Reading road node with id {0}", id);
            var geometry = _wkbReader.ReadAs<Point>(reader.GetAllBytes(3));
            return new StreamEvent(RoadNetworks.Stream, new ImportedRoadNode
            {
                Id = id,
                Version = reader.GetInt32(1),
                Type = RoadNodeType.ByIdentifier[reader.GetInt32(2)],
                Geometry = new RoadNodeGeometry
                {
                    SpatialReferenceSystemIdentifier =
                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                    Point = new BackOffice.Messages.Point
                    {
                        X = geometry.X,
                        Y = geometry.Y
                    }
                },
                Origin = new ImportedOriginProperties
                {
                    OrganizationId = reader.GetNullableString(4),
                    Organization = reader.GetNullableString(5),
                    Operator = reader.GetNullableString(6),
                    Application = reader.GetNullableString(7),
                    Since = reader.GetDateTime(8),
                    TransactionId = reader.GetNullableInt32(9) ?? TransactionId.Unknown.ToInt32()
                },
                When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
            });
        });
    }
}