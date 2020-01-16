namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation.TestHelper;
    using Messages;
    using NetTopologySuite.Geometries;
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
                    instance.Number =Fixture.Create<EuropeanRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Ident2 = Fixture.Create<NationalRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Ident8 = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentLaneAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentWidthAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentSurfaceAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
                }).OmitAutoProperties());

            Fixture.Customize<Messages.AddRoadNode>(
                composer =>
                    composer.FromFactory(random =>
                        new Messages.AddRoadNode
                        {
                            TemporaryId = Fixture.Create<RoadNodeId>(),
                            Type = Fixture.Create<RoadNodeType>(),
                            Geometry = Fixture.Create<RoadNodeGeometry>()
                        }
                    )
            );
            Fixture.Customize<Messages.AddRoadSegment>(
                composer =>
                    composer.FromFactory(random =>
                        new Messages.AddRoadSegment
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
                            LeftSideStreetNameId = Fixture.Create<Nullable<int>>(),
                            RightSideStreetNameId = Fixture.Create<Nullable<int>>(),
                            Lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                            Widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                            Surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                        }).OmitAutoProperties()
            );
            Fixture.Customize<Messages.AddGradeSeparatedJunction>(
                composer =>
                    composer.FromFactory(random =>
                        new Messages.AddGradeSeparatedJunction
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
                            switch(random.Next(0, 3))
                            {
                                case 0:
                                    result.AddRoadNode = Fixture.Create<Messages.AddRoadNode>();
                                    break;
                                case 1:
                                    result.AddRoadSegment = Fixture.Create<Messages.AddRoadSegment>();
                                    break;
                                case 2:
                                    result.AddGradeSeparatedJunction = Fixture.Create<Messages.AddGradeSeparatedJunction>();
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
            Validator.ShouldHaveValidationErrorFor(c => c.Changes, (RequestedChange[]) null);
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
            var changes = Fixture.CreateMany<Messages.AddRoadNode>(10).ToArray();
            Array.ForEach(changes, addRoadNode => addRoadNode.TemporaryId = 1);
            var data = Array.ConvertAll(changes,
                change => new RequestedChange {AddRoadNode = change});

            Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
        }

        [Fact]
        public void AllTemporaryRoadSegmentIdentifiersMustBeUnique()
        {
            var changes = Fixture.CreateMany<Messages.AddRoadSegment>(10).ToArray();
            Array.ForEach(changes, addRoadSegment => addRoadSegment.TemporaryId = 1);
            var data = Array.ConvertAll(changes,
                change => new RequestedChange {AddRoadSegment = change});

            Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
        }

        [Fact]
        public void AllTemporaryGradeSeparatedJunctionIdentifiersMustBeUnique()
        {
            var changes = Fixture.CreateMany<Messages.AddGradeSeparatedJunction>(10).ToArray();
            Array.ForEach(changes, addRoadSegment => addRoadSegment.TemporaryId = 1);
            var data = Array.ConvertAll(changes,
                change => new RequestedChange {AddGradeSeparatedJunction = change});

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
                AddGradeSeparatedJunction = Fixture.Create<Messages.AddGradeSeparatedJunction>(),
                AddRoadNode = Fixture.Create<Messages.AddRoadNode>(),
                AddRoadSegment = Fixture.Create<Messages.AddRoadSegment>(),
                AddRoadSegmentToEuropeanRoad = Fixture.Create<Messages.AddRoadSegmentToEuropeanRoad>(),
                AddRoadSegmentToNationalRoad = Fixture.Create<Messages.AddRoadSegmentToNationalRoad>(),
                AddRoadSegmentToNumberedRoad = Fixture.Create<Messages.AddRoadSegmentToNumberedRoad>(),
            };

            Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
        }

        [Fact]
        public void ChangesHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Changes, typeof(RequestedChangeValidator));
        }

        // [Fact]
        // public void VerifyValid()
        // {


        //     Validator.ValidateAndThrow(data);
        // }
    }
}
