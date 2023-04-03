namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

public class WhenDeleteOutlineWithInvalidIdFixture : WhenDeleteOutlineWithValidRequestFixture
{
    public WhenDeleteOutlineWithInvalidIdFixture(IMediator mediator) : base(mediator)
    {
    }

    protected override PostDeleteOutlineParameters CreateRequest()
    {
        return base.CreateRequest() with { WegsegmentId = 0 };
    }
}
