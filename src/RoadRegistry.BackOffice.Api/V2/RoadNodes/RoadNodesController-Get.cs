namespace RoadRegistry.BackOffice.Api.V2.RoadNodes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.BackOffice.Api.Infrastructure.Options;
using RoadRegistry.Read.Projections;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadNodesController
{
    private const string GetRoadNodeRoute = "{id}";

    /// <summary>
    ///     Vraag een wegknoop op.
    /// </summary>
    /// <param name="id">De identificator van het wegknoop.</param>
    /// <param name="apiOptions"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het wegknoop gevonden is.</response>
    /// <response code="404">Als het wegknoop niet gevonden kan worden.</response>
    /// <response code="410">Als het wegknoop is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpGet(GetRoadNodeRoute, Name = nameof(GetRoadNodeV2))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WegknoopV2Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(WegknoopV2DetailResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadNodeNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(RoadNodeGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(GetRoadNodeV2), Description = "Vraag een wegknoop op.")]
    public async Task<IActionResult> GetRoadNodeV2(
        [FromRoute] int id,
        [FromServices] ApiOptions apiOptions,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();

        var roadNode = await session.LoadAsync<RoadNodeReadItem>(id, cancellationToken);
        if (roadNode is null || !roadNode.IsV2)
        {
            return NotFound();
        }

        if (roadNode.IsRemoved)
        {
            return new StatusCodeResult(StatusCodes.Status410Gone);
        }

        var result = new WegknoopV2Detail
        {
            Identificator = new WegknoopIdentificator(OsloNamespaces.Wegknoop, roadNode.RoadNodeId.ToString(), roadNode.Origin.Timestamp.ToDateTimeOffset()),
            WegknoopGeometrie = new WegknoopGeometrie
            {
                Geometrie =
                [
                    new WegknoopGeometrieProjectie
                    {
                        Gml = roadNode.Geometry.Lambert08.Value.ConvertToGml()
                    },
                    new WegknoopGeometrieProjectie
                    {
                        Gml = roadNode.Geometry.Lambert72.Value.ConvertToGml()
                    }
                ]
            },
            WegknoopType = RoadNodeTypeV2.Parse(roadNode.Type!).ToDutchString(),
            Grensknoop = roadNode.Grensknoop,
            AansluitendeWegsegmenten = roadNode.RoadSegmentIds
                .Select(x => new WegsegmentLink(x, apiOptions.GetWegsegmentDetailUrlFormat()))
                .ToArray()
        };

        return Ok(result);
    }
}

[DataContract(Name = "WegknoopV2Detail", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopV2Detail")]
public class WegknoopV2Detail
{
    /// <summary>
    ///     Identificerende gegevens van de wegknoop.
    /// </summary>
    [DataMember(Name = "Identificator", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegknoopIdentificator Identificator { get; set; }

    /// <summary>
    ///     De middellijngeometrie van het wegknoop.
    /// </summary>
    [DataMember(Name = "wegknoopGeometrie", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegknoopGeometrie WegknoopGeometrie { get; set; }

    /// <summary>
    ///     De geometriemethode van het wegknoop.
    /// </summary>
    [DataMember(Name = "WegknoopType", Order = 3)]
    [JsonProperty(Required = Required.DisallowNull)]
    [RoadRegistryEnumDataType(typeof(RoadNodeTypeV2))]
    public required string WegknoopType { get; set; }

    /// <summary>
    ///     Een wegknoop is een grensknoop indien de wegknoop aansluit op wegen die buiten het Vlaamse gewest liggen.
    /// </summary>
    [DataMember(Name = "Grensknoop", Order = 4)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required bool Grensknoop { get; set; }

    /// <summary>
    ///     Geeft aan met welke gerealiseerde wegsegmenten de wegknoop verbonden is.
    /// </summary>
    [DataMember(Name = "AansluitendeWegsegmenten", Order = 5)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegsegmentLink[] AansluitendeWegsegmenten { get; set; } = [];
}

[DataContract(Name = "WegknoopV2Identificator", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopV2Identificator")]
public class WegknoopIdentificator : Identificator
{
    public WegknoopIdentificator(string naamruimte, string objectId, DateTimeOffset? versie)
        : base(naamruimte, objectId, versie)
    {
    }

    public WegknoopIdentificator()
        : base(string.Empty, string.Empty, string.Empty)
    {
    }
}

[DataContract(Name = "WegknoopGeometrie", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopGeometrie")]
public class WegknoopGeometrie
{
    /// <summary>
    /// Geometrie van de wegknoop.
    /// </summary>
    [DataMember(Name = "Geometrie", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required WegknoopGeometrieProjectie[] Geometrie { get; set; }

    /// <summary>
    /// GeoJSON-geometrietype.
    /// </summary>
    [DataMember(Name = "Type", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Type { get; set; } = "Point";
}

[DataContract(Name = "WegknoopGeometrieProjectie", Namespace = "")]
[CustomSwaggerSchemaId("WegknoopGeometrieProjectie")]
public class WegknoopGeometrieProjectie
{
    [DataMember(Name = "gml", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public required string Gml { get; set; }
}

public class WegknoopV2DetailResponseExamples : IExamplesProvider<WegknoopV2Detail>
{
    private readonly ApiOptions _apiOptions;

    public WegknoopV2DetailResponseExamples(ApiOptions apiOptions)
    {
        _apiOptions = apiOptions;
    }

    public WegknoopV2Detail GetExamples()
    {
        var geometry = new Point(new(243234.8929999992, 160239.3830000013)).WithSrid(WellknownSrids.Lambert72);

        return new WegknoopV2Detail
        {
            Identificator = new WegknoopIdentificator(OsloNamespaces.Wegknoop, "643556", new DateTimeOffset(2026, 09, 27, 13, 46, 14, TimeSpan.FromHours(2))),
            WegknoopGeometrie = new WegknoopGeometrie
            {
                Geometrie =
                [
                    new WegknoopGeometrieProjectie
                    {
                        Gml = geometry.EnsureLambert08().ConvertToGml()
                    },
                    new WegknoopGeometrieProjectie
                    {
                        Gml = geometry.EnsureLambert72().ConvertToGml()
                    }
                ]
            },
            WegknoopType = RoadNodeTypeV2.Validatieknoop.ToDutchString(),
            Grensknoop = true,
            AansluitendeWegsegmenten = [
                new WegsegmentLink(new RoadSegmentId(51613), _apiOptions.GetWegsegmentDetailUrlFormat()),
                new WegsegmentLink(new RoadSegmentId(89134), _apiOptions.GetWegsegmentDetailUrlFormat())
            ]
        };
    }
}
