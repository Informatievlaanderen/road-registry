namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;

public class WhenCreateOutlineWithUnknownWegbeheerderFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithUnknownWegbeheerderFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    private readonly OrganizationId _wegbeheerder = new("ABC");

    protected override Task SetupAsync()
    {
        CustomizeOrganizationRepository(new FakeOrganizationRepository()
            .Seed(_wegbeheerder, null));

        return base.SetupAsync();
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        return base.CreateRequest() with { Wegbeheerder = _wegbeheerder };
    }
}
