namespace RoadRegistry.Api.Oslo.Configuration
{
    using Aiv.Vbr.Api;
    using Aiv.Vbr.Configuration.Api;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("configuratie")]
    [ApiExplorerSettings(GroupName = "Configuratie")]
    public class RoadRegistryConfigurationController : ConfigurationController { }
}
