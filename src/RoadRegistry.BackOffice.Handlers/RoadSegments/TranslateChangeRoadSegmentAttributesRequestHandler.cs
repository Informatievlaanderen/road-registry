using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Handlers.RoadSegments
{
    using Abstractions;
    using Abstractions.RoadSegments;
    using MediatR;
    using System.ComponentModel.DataAnnotations;

    public class TranslateChangeRoadSegmentAttributesRequestHandler : IRequestHandler<TranslateChangeRoadSegmentAttributesRequest, ChangeRoadSegmentAttributesRequest>
    {


        public TranslateChangeRoadSegmentAttributesRequestHandler()
        {
            
        }

        public Task<ChangeRoadSegmentAttributesRequest> Handle(TranslateChangeRoadSegmentAttributesRequest request, CancellationToken cancellationToken)
        {

            var roadSegmentIdentifiers = parameters.SelectMany(s => s.Wegsegmenten).Distinct().ToList();

            var request = new ChangeRoadSegmentAttributesRequest();

            foreach (var roadSegmentId in roadSegmentIdentifiers)
            {
                //request.ChangeRequests.Append(new ChangeRoadSegmentStatusAttributeRequest(new RoadSegmentId(roadSegmentId)))
            }
        }
    }
}
