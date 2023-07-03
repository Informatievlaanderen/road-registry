namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using Authentication;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class ApiKeyAttribute : AuthorizeAttribute
{
    public ApiKeyAttribute()
    {
        AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme;
        Policy = PolicyNames.WegenUitzonderingen.Beheerder;
    }
}
