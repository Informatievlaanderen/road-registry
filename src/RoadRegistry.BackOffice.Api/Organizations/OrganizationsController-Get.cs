namespace RoadRegistry.BackOffice.Api.Organizations;

using Abstractions.Organizations;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

public partial class OrganizationsController
{
    private const string GetRoute = "";

    /// <summary>
    ///     Vraag de lijst met organisaties op.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetRoute, Name = nameof(Get))]
    [ProducesResponseType(typeof(GetOrganizationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetOrganizationsResponseResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(Get))]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var organizationsResponse = await Mediator.Send(new GetOrganizationsRequest(), cancellationToken);

        var organizations = new GetOrganizationsResponse();
        organizations.AddRange(organizationsResponse.Organizations
            .Select(organization => new OrganisatieObject
            {
                Code = organization.Code,
                Label = organization.Name,
                OvoCode = organization.OvoCode
            }));

        return Ok(organizations);
    }
}

[DataContract(Name = "Organisaties", Namespace = "")]
[CustomSwaggerSchemaId("Organisaties")]
public class GetOrganizationsResponse: List<OrganisatieObject>
{
}

[DataContract(Name = "Organisatie", Namespace = "")]
[CustomSwaggerSchemaId("Organisatie")]
public class OrganisatieObject
{
    [DataMember(Name = "Code", Order = 1)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Code { get; set; }

    [DataMember(Name = "Label", Order = 2)]
    [JsonProperty(Required = Required.DisallowNull)]
    public string Label { get; set; }

    [DataMember(Name = "OvoCode", Order = 3)]
    [JsonProperty]
    public string OvoCode { get; set; }
}

public class GetOrganizationsResponseResponseExamples : IExamplesProvider<GetOrganizationsResponse>
{
    public GetOrganizationsResponse GetExamples()
    {
        return new GetOrganizationsResponse
        {
            new ()
            {
                Code = "44021",
                Label = "Stad Gent",
                OvoCode = "OVO002067",
            },
            new ()
            {
                Code = "V0201",
                Label = "Agentschap Wegen en Verkeer",
                OvoCode = null
            }
        };
    }
}
