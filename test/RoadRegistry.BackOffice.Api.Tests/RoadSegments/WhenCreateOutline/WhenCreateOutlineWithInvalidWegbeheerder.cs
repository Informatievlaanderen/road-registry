namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidWegbeheerder : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidWegbeheerderFixture>
{
    public WhenCreateOutlineWithInvalidWegbeheerder(WhenCreateOutlineWithInvalidWegbeheerderFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }
    
    protected override string ExpectedErrorCode => "WegbeheerderNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Wegbeheerder is foutief.";
}
