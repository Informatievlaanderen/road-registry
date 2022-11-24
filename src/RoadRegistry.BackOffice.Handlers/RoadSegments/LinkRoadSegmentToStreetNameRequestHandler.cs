using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Handlers.RoadSegments
{
    using Framework;
    using Microsoft.Extensions.Logging;

    public class LinkRoadSegmentToStreetNameRequestHandler : EndpointRequestHandler<LinkRoadSegmentToStreetNameRequest, LinkRoadSegmentToStreetNameResponse>
    {
        public LinkRoadSegmentToStreetNameRequestHandler(CommandHandlerDispatcher dispatcher, ILogger logger) : base(dispatcher, logger)
        {
        }

        public override Task<LinkRoadSegmentToStreetNameResponse> HandleAsync(LinkRoadSegmentToStreetNameRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
