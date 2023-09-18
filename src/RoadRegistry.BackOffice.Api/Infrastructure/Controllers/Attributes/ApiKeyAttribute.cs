namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using Authorization;
using Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class ApiKeyAttribute : AuthorizeAttribute
{
    public ApiKeyAttribute()
    {
        AuthenticationSchemes = Authentication.AuthenticationSchemes.ApiKey;
        Policy = AcmIdmConstants.PolicyNames.VoInfo;
    }
}
