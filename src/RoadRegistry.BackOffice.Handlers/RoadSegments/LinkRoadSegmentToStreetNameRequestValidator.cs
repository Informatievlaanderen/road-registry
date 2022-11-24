using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Handlers.RoadSegments
{
    using FluentValidation;
    using MediatR;
    using RoadRegistry.BackOffice.Abstractions.RoadSegments;

    public class LinkRoadSegmentToStreetNameRequestValidator : AbstractValidator<LinkRoadSegmentToStreetNameRequest>, IPipelineBehavior<LinkRoadSegmentToStreetNameRequest, LinkRoadSegmentToStreetNameResponse>
    {
        public LinkRoadSegmentToStreetNameRequestValidator()
        {
            RuleFor(x => x.RoadSegmentId)
                .GreaterThan(0);
            RuleFor(x => x.LeftStreetNameId)
                .Must((request, value) => request.RightStreetNameId <= 0 ? value > 0 : value == 0);
            RuleFor(x => x.RightStreetNameId)
                .Must((request, value) => request.LeftStreetNameId <= 0 ? value > 0 : value == 0);

            //TODO-rik validate if roadsegmentid/leftstreetnameid/rightstreetnameid exists

        }

        public async Task<LinkRoadSegmentToStreetNameResponse> Handle(LinkRoadSegmentToStreetNameRequest request, RequestHandlerDelegate<LinkRoadSegmentToStreetNameResponse> next, CancellationToken cancellationToken)
        {
            await this.ValidateAndThrowAsync(request, cancellationToken);
            var response = await next();
            return response;
        }
    }
}
