namespace RoadRegistry.BackOffice.Api.Organizations;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentValidation;
using Framework;
using Infrastructure.Authentication;
using Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class OrganizationsController
{
    private const string ChangeRoute = "{id}";

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
    [HttpPatch(ChangeRoute, Name = nameof(Change))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    [SwaggerOperation(OperationId = nameof(Change), Description = "")]
    public async Task<IActionResult> Change(
        [FromBody] OrganizationChangeParameters parameters,
        [FromRoute] string id,
        [FromServices] IValidator<ChangeOrganization> validator,
        CancellationToken cancellationToken)
    {
        var command = new ChangeOrganization
        {
            Code = id,
            Name = parameters?.Name,
            OvoCode = parameters?.OvoCode,
            IsMaintainer = parameters?.IsMaintainer
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await OrganizationCommandQueue
            .WriteAsync(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed class OrganizationChangeParameters
{
    public string? Name { get; set; }
    public string? OvoCode { get; set; }
    public bool? IsMaintainer { get; set; }
};
