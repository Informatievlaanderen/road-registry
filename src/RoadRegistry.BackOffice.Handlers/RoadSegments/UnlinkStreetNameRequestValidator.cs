namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using Abstractions.Validation;
using Extensions;
using FluentValidation;
using MediatR;

public class UnlinkStreetNameRequestValidator : AbstractValidator<UnlinkStreetNameRequest>, IPipelineBehavior<UnlinkStreetNameRequest, UnlinkStreetNameResponse>
{
    public UnlinkStreetNameRequestValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.WegsegmentId));

        RuleFor(x => x.LinkerstraatnaamId)
            .MustBeValidStreetNamePuri()
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.LinkerstraatnaamId));

        RuleFor(x => x.LinkerstraatnaamId)
            .Must((request, _) =>
            {
                var leftIdentifier = request.LinkerstraatnaamId.GetIdentifierFromPuri();
                var rightIdentifier = request.RechterstraatnaamId.GetIdentifierFromPuri();
                return rightIdentifier <= 0 ? leftIdentifier > 0 : leftIdentifier == 0;
            })
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.LinkerstraatnaamId));

        RuleFor(x => x.RechterstraatnaamId)
            .MustBeValidStreetNamePuri()
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.RechterstraatnaamId));

        RuleFor(x => x.RechterstraatnaamId)
            .Must((request, _) =>
            {
                var leftIdentifier = request.LinkerstraatnaamId.GetIdentifierFromPuri();
                var rightIdentifier = request.RechterstraatnaamId.GetIdentifierFromPuri();
                return leftIdentifier <= 0 ? rightIdentifier > 0 : rightIdentifier == 0;
            })
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.RechterstraatnaamId));
    }

    public async Task<UnlinkStreetNameResponse> Handle(UnlinkStreetNameRequest request, RequestHandlerDelegate<UnlinkStreetNameResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
