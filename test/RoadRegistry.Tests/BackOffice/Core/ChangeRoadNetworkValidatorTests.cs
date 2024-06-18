namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using AddGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction;
using AddRoadNode = RoadRegistry.BackOffice.Messages.AddRoadNode;
using AddRoadSegment = RoadRegistry.BackOffice.Messages.AddRoadSegment;
using AddRoadSegmentToEuropeanRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToEuropeanRoad;
using AddRoadSegmentToNationalRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNationalRoad;
using AddRoadSegmentToNumberedRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNumberedRoad;
using ModifyRoadNode = RoadRegistry.BackOffice.Messages.ModifyRoadNode;
using RequestedRoadSegmentEuropeanRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentEuropeanRoadAttribute;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RequestedRoadSegmentNationalRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentNationalRoadAttribute;
using RequestedRoadSegmentNumberedRoadAttribute = RoadRegistry.BackOffice.Messages.RequestedRoadSegmentNumberedRoadAttribute;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class ChangeRoadNetworkValidatorTests : ValidatorTest<ChangeRoadNetwork, ChangeRoadNetworkValidator>
{
    public ChangeRoadNetworkValidatorTests()
    {
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

        Fixture.Customize<RequestedRoadSegmentEuropeanRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<EuropeanRoadNumber>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentNationalRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NationalRoadNumber>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentNumberedRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NumberedRoadNumber>();
                instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadSegmentLaneAttributes>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadSegmentWidthAttributes>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = Fixture.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
        Fixture.Customize<RoadSegmentSurfaceAttributes>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
            }).OmitAutoProperties());
        Fixture.Customize<AddRoadNode>(composer =>
            composer.FromFactory(random =>
                new AddRoadNode
                {
                    TemporaryId = Fixture.Create<RoadNodeId>(),
                    Type = Fixture.Create<RoadNodeType>(),
                    Geometry = Fixture.Create<RoadNodeGeometry>()
                }
            ));
        Fixture.Customize<ModifyRoadNode>(
            composer =>
                composer.FromFactory(random =>
                    new ModifyRoadNode
                    {
                        Id = Fixture.Create<RoadNodeId>(),
                        Type = Fixture.Create<RoadNodeType>(),
                        Geometry = Fixture.Create<RoadNodeGeometry>()
                    }
                )
        );
        Fixture.Customize<AddRoadSegment>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegment
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
        Fixture.Customize<AddGradeSeparatedJunction>(
            composer =>
                composer.FromFactory(random =>
                    new AddGradeSeparatedJunction
                    {
                        TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                        UpperSegmentId = Fixture.Create<RoadSegmentId>(),
                        LowerSegmentId = Fixture.Create<RoadSegmentId>(),
                        Type = Fixture.Create<GradeSeparatedJunctionType>()
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<RequestedRoadSegmentLaneAttribute>(
            composer =>
                composer.FromFactory(random =>
                    new RequestedRoadSegmentLaneAttribute
                    {
                        AttributeId = Fixture.Create<int>(),
                        FromPosition = 0.00m,
                        ToPosition = 0.01m,
                        Count = 1,
                        Direction = RoadSegmentLaneDirection.Independent
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<RequestedRoadSegmentWidthAttribute>(
            composer =>
                composer.FromFactory(random =>
                    new RequestedRoadSegmentWidthAttribute
                    {
                        AttributeId = Fixture.Create<int>(),
                        FromPosition = 0.00m,
                        ToPosition = 0.01m,
                        Width = 1
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<RequestedRoadSegmentSurfaceAttribute>(
            composer =>
                composer.FromFactory(random =>
                    new RequestedRoadSegmentSurfaceAttribute
                    {
                        AttributeId = Fixture.Create<int>(),
                        FromPosition = 0.00m,
                        ToPosition = 0.01m,
                        Type = RoadSegmentSurfaceType.NotApplicable
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
                                result.AddRoadNode = Fixture.Create<AddRoadNode>();
                                break;
                            case 1:
                                result.AddRoadSegment = Fixture.Create<AddRoadSegment>();
                                break;
                            case 2:
                                result.AddGradeSeparatedJunction = Fixture.Create<AddGradeSeparatedJunction>();
                                break;
                            case 3:
                                result.ModifyRoadNode = Fixture.Create<ModifyRoadNode>();
                                break;
                        }

                        return result;
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<AddRoadSegmentToEuropeanRoad>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegmentToEuropeanRoad
                    {
                        TemporaryAttributeId = Fixture.Create<int>(),
                        SegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = Fixture.Create<RoadSegmentId>(),
                        Number = Fixture.Create<EuropeanRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<AddRoadSegmentToNationalRoad>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegmentToNationalRoad
                    {
                        TemporaryAttributeId = Fixture.Create<int>(),
                        SegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = Fixture.Create<RoadSegmentId>(),
                        Number = Fixture.Create<NationalRoadNumber>()
                    }
                ).OmitAutoProperties()
        );
        Fixture.Customize<AddRoadSegmentToNumberedRoad>(
            composer =>
                composer.FromFactory(random =>
                    new AddRoadSegmentToNumberedRoad
                    {
                        TemporaryAttributeId = Fixture.Create<int>(),
                        SegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        SegmentId = Fixture.Create<RoadSegmentId>(),
                        Number = Fixture.Create<NumberedRoadNumber>(),
                        Direction = RoadSegmentNumberedRoadDirection.Unknown
                    }
                ).OmitAutoProperties()
        );

        Model = new ChangeRoadNetwork
        {
            RequestId = "0000-00000000-00000000-00000000-00000000-00000000-00000000-00000",
            OrganizationId = "000000000000000000",
            Reason = Fixture.Create<string>(),
            Operator = Fixture.Create<string>(),
            Changes = new[]
            {
                new RequestedChange
                {
                    AddGradeSeparatedJunction = Fixture.Create<AddGradeSeparatedJunction>(),
                    AddRoadNode = new AddRoadNode
                    {
                        TemporaryId = Fixture.Create<RoadNodeId>(),
                        Type = RoadNodeType.FakeNode,
                        Geometry = Fixture.Create<RoadNodeGeometry>()
                    },
                    ModifyRoadNode = new ModifyRoadNode
                    {
                        Id = Fixture.Create<RoadNodeId>(),
                        Type = RoadNodeType.FakeNode,
                        Geometry = Fixture.Create<RoadNodeGeometry>()
                    },
                    AddRoadSegment = new AddRoadSegment
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
                    },
                    AddRoadSegmentToEuropeanRoad = Fixture.Create<AddRoadSegmentToEuropeanRoad>(),
                    AddRoadSegmentToNationalRoad = Fixture.Create<AddRoadSegmentToNationalRoad>(),
                    AddRoadSegmentToNumberedRoad = Fixture.Create<AddRoadSegmentToNumberedRoad>()
                }
            }
        };
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

        ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllTemporaryGradeSeparatedJunctionIdentifiersMustBeUnique()
    {
        var changes = Fixture.CreateMany<AddGradeSeparatedJunction>(10).ToArray();
        Array.ForEach(changes, addRoadSegment => addRoadSegment.TemporaryId = 1);
        var data = Array.ConvertAll(changes,
            change => new RequestedChange { AddGradeSeparatedJunction = change });

        ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllTemporaryRoadNodeIdentifiersMustBeUnique()
    {
        var changes = Fixture.CreateMany<AddRoadNode>(10).ToArray();
        Array.ForEach(changes, addRoadNode => addRoadNode.TemporaryId = 1);
        var data = Array.ConvertAll(changes,
            change => new RequestedChange { AddRoadNode = change });

        ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void AllTemporaryRoadSegmentIdentifiersMustBeUnique()
    {
        var changes = Fixture.CreateMany<AddRoadSegment>(10).ToArray();
        Array.ForEach(changes, addRoadSegment => addRoadSegment.TemporaryId = 1);
        var data = Array.ConvertAll(changes,
            change => new RequestedChange { AddRoadSegment = change });

        ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void ChangeCanNotBeNull()
    {
        var data = Fixture.CreateMany<RequestedChange>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = null;

        ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public void ChangesCanNotBeNull()
    {
        ShouldHaveValidationErrorFor(c => c.Changes, null);
    }

    [Fact]
    public void ChangesHasExpectedValidator()
    {
        Validator.ShouldHaveChildValidator(c => c.Changes, typeof(RequestedChangeValidator));
    }

    [Fact]
    public void MoreThanOneChangeIsNotNull()
    {
        var data = Fixture.CreateMany<RequestedChange>(10).ToArray();
        var index = new Random().Next(0, data.Length);
        data[index] = new RequestedChange
        {
            AddGradeSeparatedJunction = Fixture.Create<AddGradeSeparatedJunction>(),
            AddRoadNode = Fixture.Create<AddRoadNode>(),
            ModifyRoadNode = Fixture.Create<ModifyRoadNode>(),
            AddRoadSegment = Fixture.Create<AddRoadSegment>(),
            AddRoadSegmentToEuropeanRoad = Fixture.Create<AddRoadSegmentToEuropeanRoad>(),
            AddRoadSegmentToNationalRoad = Fixture.Create<AddRoadSegmentToNationalRoad>(),
            AddRoadSegmentToNumberedRoad = Fixture.Create<AddRoadSegmentToNumberedRoad>()
        };

        ShouldHaveValidationErrorFor(c => c.Changes, data);
    }

    [Fact]
    public override void VerifyValid()
    {
        Model.Changes[0].AddGradeSeparatedJunction = null;
        Model.Changes[0].AddRoadNode = null;
        Model.Changes[0].ModifyRoadNode = null;
        //Model.Changes[0].AddRoadSegment = null;
        Model.Changes[0].AddRoadSegmentToEuropeanRoad = null;
        Model.Changes[0].AddRoadSegmentToNationalRoad = null;
        Model.Changes[0].AddRoadSegmentToNumberedRoad = null;

        base.VerifyValid();
    }
}
