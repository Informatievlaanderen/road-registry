namespace RoadRegistry.BackOffice.Api.Organizations;

using FluentValidation;
using Framework;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

public partial class OrganizationsController
{
    private const string RenameRoute = "{id}/rename";

    /// <summary>
    ///     Renames the organization.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPatch(RenameRoute, Name = nameof(Rename))]
    [SwaggerOperation(OperationId = nameof(Rename), Description = "")]
    public async Task<IActionResult> Rename(
        [FromBody] OrganizationRenameParameters parameters,
        [FromRoute] string id,
        [FromServices] IValidator<RenameOrganization> validator,
        CancellationToken cancellationToken)
    {
        var command = new RenameOrganization
        {
            Code = id,
            Name = parameters?.Name
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await RoadNetworkCommandQueue
            .WriteAsync(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed record OrganizationRenameParameters(string Name);
