namespace RoadRegistry.BackOffice.Api.Organizations;

using System.Threading;
using System.Threading.Tasks;
using FeatureToggles;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class OrganizationsController
{
    private const string CreateRoute = "";

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
    [HttpPost(CreateRoute, Name = nameof(Create))]
    [SwaggerOperation(OperationId = nameof(Create), Description = "")]
    public async Task<IActionResult> Create([FromBody] OrganizationCreateParameters parameters,
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
            Name = parameters?.Name,
            OvoCode = parameters?.OvoCode
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await RoadNetworkCommandQueue
            .WriteAsync(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed record OrganizationCreateParameters(string Code, string Name, string OvoCode);
