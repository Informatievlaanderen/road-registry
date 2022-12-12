namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("system")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class RoadRegistrySystemController : ControllerBase
{
    protected IStreamStore Store { get; }

    public RoadRegistrySystemController(
        IStreamStore store)
    {
        Store = store;
    }
}
