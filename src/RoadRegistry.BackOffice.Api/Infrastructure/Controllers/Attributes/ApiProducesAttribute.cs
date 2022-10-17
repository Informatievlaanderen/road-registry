namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Mvc;
    using RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

    public class ApiProducesAttribute : ProducesAttribute
    {
        public ApiProducesAttribute()
            : this(EndpointType.Legacy) { }

        public ApiProducesAttribute(EndpointType endpointType) : base(AcceptTypes.Json)
        {
            ContentTypes = endpointType.Produces();
        }
    }
}
