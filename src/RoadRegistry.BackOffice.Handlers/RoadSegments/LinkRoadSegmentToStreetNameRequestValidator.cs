namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using FluentValidation;
using MediatR;

public class LinkRoadSegmentToStreetNameRequestValidator : AbstractValidator<LinkRoadSegmentToStreetNameRequest>, IPipelineBehavior<LinkRoadSegmentToStreetNameRequest, LinkRoadSegmentToStreetNameResponse>
{
    public LinkRoadSegmentToStreetNameRequestValidator()
    {
        RuleFor(x => x.RoadSegmentId)
            .GreaterThan(0)
            .WithMessage($"'{nameof(LinkRoadSegmentToStreetNameRequest.RoadSegmentId)}' must be greater than 0");
        RuleFor(x => x.LeftStreetNameId)
            .Must((request, value) => request.RightStreetNameId <= 0 ? value > 0 : value == 0)
            .WithMessage($"'{nameof(LinkRoadSegmentToStreetNameRequest.LeftStreetNameId)}' must be greater than 0 while '{nameof(LinkRoadSegmentToStreetNameRequest.RightStreetNameId)}' is empty");
        RuleFor(x => x.RightStreetNameId)
            .Must((request, value) => request.LeftStreetNameId <= 0 ? value > 0 : value == 0)
            .WithMessage($"'{nameof(LinkRoadSegmentToStreetNameRequest.RightStreetNameId)}' must be greater than 0 while '{nameof(LinkRoadSegmentToStreetNameRequest.LeftStreetNameId)}' is empty");
    }

    public async Task<LinkRoadSegmentToStreetNameResponse> Handle(LinkRoadSegmentToStreetNameRequest request, RequestHandlerDelegate<LinkRoadSegmentToStreetNameResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
