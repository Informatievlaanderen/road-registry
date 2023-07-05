namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Attributes;
using Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySecurityController
{
    private const string UserRoute = "user";

    [ApiKey]
    [HttpGet(UserRoute, Name = nameof(GetUser))]
    [SwaggerOperation(OperationId = nameof(GetUser), Description = "")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = AcmIdmConstants.PolicyNames.VoInfo)]
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