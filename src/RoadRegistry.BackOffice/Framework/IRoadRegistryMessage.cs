namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public interface IRoadRegistryMessage
{
    Guid MessageId { get; }
    ClaimsPrincipal Principal { get; }
}
