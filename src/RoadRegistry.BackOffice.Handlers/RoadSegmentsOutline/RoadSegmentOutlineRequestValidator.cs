namespace RoadRegistry.BackOffice.Handlers.RoadSegmentsOutline
{
    using Abstractions.RoadSegments;
    using FluentValidation;
    using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

    public class RoadSegmentOutlineRequestValidator : AbstractValidator<CreateRoadSegmentOutlineRequest>
    {
        public RoadSegmentOutlineRequestValidator()
        {

        }
    }
}
