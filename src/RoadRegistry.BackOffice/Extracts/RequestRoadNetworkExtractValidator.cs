namespace RoadRegistry.BackOffice.Extracts;

using FluentValidation;
using Messages;

public class RequestRoadNetworkExtractValidator : AbstractValidator<RequestRoadNetworkExtract>
{
    public RequestRoadNetworkExtractValidator()
    {
        RuleFor(c => c.ExternalRequestId).NotEmpty();
        RuleFor(c => c.DownloadId).NotEmpty();
        RuleFor(c => c.Contour).NotNull().SetValidator(new RoadNetworkExtractGeometryValidator());
    }
}