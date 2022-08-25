namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class ChangeRoadNetworkValidatorTests
{
    public ChangeRoadNetworkValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizePoint();
        Fixture.CustomizePolylineM();

        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeRoadSegmentCategory();
        Fixture.CustomizeRoadSegmentMorphology();
        Fixture.CustomizeRoadSegmentStatus();
        Fixture.CustomizeRoadSegmentAccessRestriction();
        Fixture.CustomizeRoadSegmentLaneCount();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();
        Fixture.CustomizeRoadSegmentGeometryDrawMethod();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentSurfaceType();
        Fixture.CustomizeRoadSegmentWidth();
        Fixture.CustomizeEuropeanRoadNumber();
        Fixture.CustomizeNationalRoadNumber();
        Fixture.CustomizeNumberedRoadNumber();
        Fixture.CustomizeGradeSeparatedJunctionId();
        Fixture.CustomizeGradeSeparatedJunctionType();

        Fixture.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<EuropeanRoadNumber>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadSegmentNationalRoadAttributes>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NationalRoadNumber>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NumberedRoadNumber>();
                instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = Fixture.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
            }).OmitAutoProperties());

        Fixture.Customize<RoadRegistry.BackOffice.Messages.AddRoadNode>(
            composer =>
                composer.FromFactory(random =>
                    new RoadRegistry.BackOffice.Messages.AddRoadNode
                    {
                        TemporaryId = Fixture.Create<RoadNodeId>(),
                        Type = Fixture.Create<RoadNodeType>(),
                        Geometry = Fixture.Create<RoadNodeGeometry>()
                    }
                )
        );
        Fixture.Customize<RoadRegistry.BackOffice.Messages.ModifyRoadNode>(
            composer =>
                composer.FromFactory(random =>
                    new RoadRegistry.BackOffice.Messages.ModifyRoadNode
                    {
                        Id = Fixture.Create<RoadNodeId>(),
                        Type = Fixture.Create<RoadNodeType>(),
                        Geometry = Fixture.Create<RoadNodeGeometry>()
                    }
                )
        );
        Fixture.Customize<RoadRegistry.BackOffice.Messages.AddRoadSegment>(
            composer =>
                composer.FromFactory(random =>
                    new RoadRegistry.BackOffice.Messages.AddRoadSegment
                    {
                        TemporaryId = Fixture.Create<RoadSegmentId>(),
                        StartNodeId = Fixture.Create<RoadNodeId>(),
                        EndNodeId = Fixture.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>()),
                        MaintenanceAuthority = Fixture.Create<string>(),
                        GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        Morphology = Fixture.Create<RoadSegmentMorphology>(),
                        Status = Fixture.Create<RoadSegmentStatus>(),
                        Category = Fixture.Create<RoadSegmentCategory>(),
                        AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                        LeftSideStreetNameId = Fixture.Create<int?>(),
                        RightSideStreetNameId = Fixture.Create<int?>(),
                        Lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                        Widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                        Surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                    }).OmitAutoProperties()
        );
        Fixture.Customize<RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction>(
            composer =>
                composer.FromFactory(random =>
                    new RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction
                    {
                        TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                        UpperSegmentId = Fixture.Create<RoadSegmentId>(),
                        LowerSegmentId = Fixture.Create<RoadSegmentId>(),
                        Type = Fixture.Create<GradeSeparatedJunctionType>()
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<RequestedChange>(
            composer =>
                composer.FromFactory(random =>
                    {
                        var result = new RequestedChange();
                        switch (random.Next(0, 4))
                        {
                            case 0:
                                result.AddRoadNode = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadNode>();
                                break;
                            case 1:
                                result.AddRoadSegment = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadSegment>();
                                break;
                            case 2:
                                result.AddGradeSeparatedJunction = Fixture.Create<RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction>();
                                break;
                            case 3:
                                result.ModifyRoadNode = Fixture.Create<RoadRegistry.BackOffice.Messages.ModifyRoadNode>();
                                break;
                        }

                        return result;
                    }
                ).OmitAutoProperties()
        );
        Validator = new ChangeRoadNetworkValidator();
    }

    public Fixture Fixture { get; }
    public ChangeRoadNetworkValidator Validator { get; }

    [Fact]
    public void ChangesCanNotBeNull()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Changes, (RequestedChange[])null);
    }

    [Fact]
    public void ChangeCanNotBeNull()
    {
        var data = Fixture.CreateMany<RequestedChange>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllTemporaryRoadNodeIdentifiersMustBeUnique()
    {
        var changes = Fixture.CreateMany<RoadRegistry.BackOffice.Messages.AddRoadNode>(10).ToArray();
        Array.ForEach(changes, addRoadNode => addRoadNode.TemporaryId = 1);
        var data = Array.ConvertAll(changes,
            change => new RequestedChange { AddRoadNode = change });

        Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllTemporaryRoadSegmentIdentifiersMustBeUnique()
    {
        var changes = Fixture.CreateMany<RoadRegistry.BackOffice.Messages.AddRoadSegment>(10).ToArray();
        Array.ForEach(changes, addRoadSegment => addRoadSegment.TemporaryId = 1);
        var data = Array.ConvertAll(changes,
            change => new RequestedChange { AddRoadSegment = change });

        Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllTemporaryGradeSeparatedJunctionIdentifiersMustBeUnique()
    {
        var changes = Fixture.CreateMany<RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction>(10).ToArray();
        Array.ForEach(changes, addRoadSegment => addRoadSegment.TemporaryId = 1);
        var data = Array.ConvertAll(changes,
            change => new RequestedChange { AddGradeSeparatedJunction = change });

        Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllChangesAreNull()
    {
        var data = Fixture.CreateMany<RequestedChange>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = new RequestedChange
        {
            AddGradeSeparatedJunction = null,
            AddRoadNode = null,
            ModifyRoadNode = null,
            AddRoadSegment = null,
            AddRoadSegmentToEuropeanRoad = null,
            AddRoadSegmentToNationalRoad = null,
            AddRoadSegmentToNumberedRoad = null
        };

        Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void MoreThanOneChangeIsNotNull()
    {
        var data = Fixture.CreateMany<RequestedChange>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = new RequestedChange
        {
            AddGradeSeparatedJunction = Fixture.Create<RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction>(),
            AddRoadNode = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadNode>(),
            ModifyRoadNode = Fixture.Create<RoadRegistry.BackOffice.Messages.ModifyRoadNode>(),
            AddRoadSegment = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadSegment>(),
            AddRoadSegmentToEuropeanRoad = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadSegmentToEuropeanRoad>(),
            AddRoadSegmentToNationalRoad = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadSegmentToNationalRoad>(),
            AddRoadSegmentToNumberedRoad = Fixture.Create<RoadRegistry.BackOffice.Messages.AddRoadSegmentToNumberedRoad>()
        };

        Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void ChangesHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Changes, typeof(RequestedChangeValidator));
    }
}
