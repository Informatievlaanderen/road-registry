namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Threading;
using System.Threading.Tasks;
using FeatureToggles;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySystemController
{
    private const string CreateOrganizationRoute = "organization";

    /// <summary>
    ///     Creates the organization.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="featureToggle">The feature toggle.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CreateOrganizationRoute, Name = nameof(CreateOrganization))]
    [SwaggerOperation(OperationId = nameof(CreateOrganization), Description = "")]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationCreateParameters parameters,
        [FromServices] UseOrganizationCreateFeatureToggle featureToggle,
        [FromServices] IValidator<CreateOrganization> validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        var command = new CreateOrganization
        {
            Code = parameters?.Code,
            Name = parameters?.Name
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await RoadNetworkCommandQueue
            .Write(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed record OrganizationCreateParameters(string Code, string Name);