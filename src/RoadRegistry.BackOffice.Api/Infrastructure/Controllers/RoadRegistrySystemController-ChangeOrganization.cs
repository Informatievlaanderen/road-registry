namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySystemController
{
    private const string ChangeOrganizationRoute = "organization/{id}";

    /// <summary>
    ///     Changes the organization.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPatch(ChangeOrganizationRoute, Name = nameof(ChangeOrganization))]
    [SwaggerOperation(OperationId = nameof(ChangeOrganization), Description = "")]
    public async Task<IActionResult> ChangeOrganization([FromBody] OrganizationChangeParameters parameters,
        [FromRoute] string id,
        [FromServices] IValidator<ChangeOrganization> validator,
        CancellationToken cancellationToken)
    {
        var command = new ChangeOrganization
        {
            Code = id,
            Name = parameters?.Name,
            OvoCode = parameters?.OvoCode
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await RoadNetworkCommandQueue
            .Write(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed record OrganizationChangeParameters(string Name, string OvoCode);
