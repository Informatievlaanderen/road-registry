namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Authentication;
using Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

public partial class RoadRegistrySecurityController
{
    private const string UserRoute = "user";

    [HttpGet(UserRoute, Name = nameof(GetUser))]
    [SwaggerOperation(OperationId = nameof(GetUser), Description = "")]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllSchemes, Policy = AcmIdmConstants.PolicyNames.VoInfo)]
    public IActionResult GetUser()
    {
        return Ok(new UserData(HttpContext.User));
    }
}

public class UserData
{
    public UserData(ClaimsPrincipal user)
    {
        Claims = user.Claims
            .Select(claim => new UserClaimData(claim.Type, claim.Value))
            .ToArray();
    }

    public ICollection<UserClaimData> Claims { get; }
}

public sealed record UserClaimData(string Type, string Value);
