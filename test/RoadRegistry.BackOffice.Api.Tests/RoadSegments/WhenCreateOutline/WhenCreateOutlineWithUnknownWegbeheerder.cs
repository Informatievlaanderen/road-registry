namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithUnknownWegbeheerder : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithUnknownWegbeheerderFixture>
{
    public WhenCreateOutlineWithUnknownWegbeheerder(WhenCreateOutlineWithUnknownWegbeheerderFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "WegbeheerderNietGekend";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven wegbeheerdercode";
}
