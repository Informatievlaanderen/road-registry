namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using MediatR;

public sealed record CorrectRoadSegmentStatusDutchTranslationsRequest : IRequest<CorrectRoadSegmentStatusDutchTranslationsResponse>
{
    public int Identifier { get; set; }
    public string Name { get; set; }
}
