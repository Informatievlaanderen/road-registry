namespace RoadRegistry.Tests.BackOffice.Core;

using FluentValidation.TestHelper;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RequestedChangeValidatorTests : ValidatorTest<RequestedChange, RequestedChangeValidator>
{
    public RequestedChangeValidatorTests()
    {
        Model = new RequestedChange();
    }

    [Fact]
    public void AddGradeSeparatedJunctionHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.AddGradeSeparatedJunction, typeof(AddGradeSeparatedJunctionValidator));
    }

    [Fact]
    public void AddRoadNodeHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.AddRoadNode, typeof(AddRoadNodeValidator));
    }

    [Fact]
    public void AddRoadSegmentHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.AddRoadSegment, typeof(AddRoadSegmentValidator));
    }

    [Fact]
    public void AddRoadSegmentToEuropeanRoadHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.AddRoadSegmentToEuropeanRoad, typeof(AddRoadSegmentToEuropeanRoadValidator));
    }

    [Fact]
    public void AddRoadSegmentToNationalRoadHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.AddRoadSegmentToNationalRoad, typeof(AddRoadSegmentToNationalRoadValidator));
    }

    [Fact]
    public void AddRoadSegmentToNumberedRoadHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.AddRoadSegmentToNumberedRoad, typeof(AddRoadSegmentToNumberedRoadValidator));
    }

    [Fact]
    public void ModifyRoadNodeHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.ModifyRoadNode, typeof(ModifyRoadNodeValidator));
    }
}
