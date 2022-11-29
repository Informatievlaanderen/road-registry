namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using Extensions;
using FluentValidation;
using MediatR;

public class LinkRoadSegmentToStreetNameRequestValidator : AbstractValidator<LinkRoadSegmentToStreetNameRequest>, IPipelineBehavior<LinkRoadSegmentToStreetNameRequest, LinkRoadSegmentToStreetNameResponse>
{
    public LinkRoadSegmentToStreetNameRequestValidator()
    {
        RuleFor(x => x.RoadSegmentId)
            .GreaterThan(0)
            .WithErrorCode("IncorrectObjectId")
            .WithMessage(request => $"De waarde '{request.RoadSegmentId}' is ongeldig.");
        RuleFor(x => x.LeftStreetNameId)
            .MustBeValidStreetNamePuri()
            .WithErrorCode("IncorrectObjectId")
            .WithMessage(request => $"De waarde '{request.LeftStreetNameId}' is ongeldig.")
            .Must((request, _) =>
            {
                var leftIdentifier = request.LeftStreetNameId.GetIdentifierFromPuri();
                var rightIdentifier = request.RightStreetNameId.GetIdentifierFromPuri();
                return rightIdentifier <= 0 ? leftIdentifier > 0 : leftIdentifier == 0;
            })
            .WithErrorCode("IncorrectObjectId")
            .WithMessage(request => $"De waarde '{request.LeftStreetNameId}' is ongeldig.");
        RuleFor(x => x.RightStreetNameId)
            .MustBeValidStreetNamePuri()
            .WithErrorCode("IncorrectObjectId")
            .WithMessage(request => $"De waarde '{request.RightStreetNameId}' is ongeldig.")
            .Must((request, _) =>
            {
                var leftIdentifier = request.LeftStreetNameId.GetIdentifierFromPuri();
                var rightIdentifier = request.RightStreetNameId.GetIdentifierFromPuri();

                return leftIdentifier <= 0 ? rightIdentifier > 0 : rightIdentifier == 0;
            })
            .WithErrorCode("IncorrectObjectId")
            .WithMessage(request => $"De waarde '{request.RightStreetNameId}' is ongeldig.");
    }

    public async Task<LinkRoadSegmentToStreetNameResponse> Handle(LinkRoadSegmentToStreetNameRequest request, RequestHandlerDelegate<LinkRoadSegmentToStreetNameResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
