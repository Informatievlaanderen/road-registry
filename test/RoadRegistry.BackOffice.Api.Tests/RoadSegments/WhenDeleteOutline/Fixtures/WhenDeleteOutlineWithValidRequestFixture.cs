using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using AutoFixture;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

public class WhenDeleteOutlineWithValidRequestFixture : WhenDeleteOutlineFixture
{
    public WhenDeleteOutlineWithValidRequestFixture(IMediator mediator) : base(mediator)
    {
    }

    protected override PostDeleteOutlineParameters CreateRequest()
    {
        return new PostDeleteOutlineParameters
        {
            WegsegmentId = ObjectProvider.Create<RoadSegmentId>()
        };
    }
}
