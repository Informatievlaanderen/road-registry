namespace RoadRegistry.BackOffice.Extracts
{
    using FluentValidation;

    public class RequestRoadNetworkExtractValidator : AbstractValidator<Messages.RequestRoadNetworkExtract>
    {
        public RequestRoadNetworkExtractValidator()
        {
            RuleFor(c => c.ExternalRequestId).NotEmpty();
            RuleFor(c => c.DownloadId).NotEmpty();
            RuleFor(c => c.Contour).NotNull().SetValidator(new RoadNetworkExtractGeometryValidator());
        }
    }
}
