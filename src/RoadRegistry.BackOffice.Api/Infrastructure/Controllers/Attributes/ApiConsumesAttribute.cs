namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Mvc;
    using RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

    public class ApiConsumesAttribute : ConsumesAttribute
    {
        public ApiConsumesAttribute()
            : this(EndpointType.BackOffice) { }

        public ApiConsumesAttribute(EndpointType endpointType) : base(AcceptTypes.Json)
        {
            ContentTypes = endpointType.Consumes();
        }
    }
}
