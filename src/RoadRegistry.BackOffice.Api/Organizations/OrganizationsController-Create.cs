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
    private const string CreateRoute = "";

    /// <summary>
    ///     Creates the organization.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CreateRoute, Name = nameof(Create))]
    [SwaggerOperation(OperationId = nameof(Create), Description = "")]
    public async Task<IActionResult> Create(
        [FromBody] OrganizationCreateParameters parameters,
        [FromServices] IValidator<CreateOrganization> validator,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganization
        {
            Code = parameters?.Code,
            Name = parameters?.Name,
            OvoCode = parameters?.OvoCode
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await OrganizationCommandQueue
            .WriteAsync(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed record OrganizationCreateParameters(string Code, string Name, string OvoCode);
