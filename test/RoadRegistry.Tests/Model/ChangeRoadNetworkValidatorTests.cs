namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
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
            Fixture.CustomizePointM();
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

            Fixture.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.RoadNumber =Fixture.Create<EuropeanRoadNumber>();
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
                            Geometry = GeometryTranslator.Translate(Fixture.Create<PointM>())
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
                            PartOfEuropeanRoads = Fixture.CreateMany<RoadSegmentEuropeanRoadAttributes>()
                                .ToArray(),
                            PartOfNationalRoads = Fixture.CreateMany<RoadSegmentNationalRoadAttributes>()
                                .ToArray(),
                            PartOfNumberedRoads = Fixture.CreateMany<RoadSegmentNumberedRoadAttributes>()
                                .ToArray(),
                            Lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttribute>().ToArray(),
                            Widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttribute>().ToArray(),
                            Surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttribute>().ToArray()
                        })
            );
            Fixture.Customize<RequestedChange>(
                composer =>
                    composer.FromFactory(random =>
                        {
                            var result = new RequestedChange();
                            switch(random.Next(0, 2))
                            {
                                case 0:
                                    result.AddRoadNode = Fixture.Create<Messages.AddRoadNode>();
                                    break;
                                case 1:
                                    result.AddRoadSegment = Fixture.Create<Messages.AddRoadSegment>();
                                    break;
                            }
                            return result;
                        }
                    )
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
            var addRoadNodes = Fixture.CreateMany<Messages.AddRoadNode>(10).ToArray();
            Array.ForEach(addRoadNodes, addRoadNode => addRoadNode.TemporaryId = 1);
            var data = Array.ConvertAll(addRoadNodes,
                addRoadNode => new RequestedChange {AddRoadNode = addRoadNode});

            Validator.ShouldHaveValidationErrorFor(c => c.Changes, data);
        }

        [Fact]
        public void AllTemporaryRoadSegmentIdentifiersMustBeUnique()
        {
            var addRoadSegments = Fixture.CreateMany<Messages.AddRoadSegment>(10).ToArray();
            Array.ForEach(addRoadSegments, addRoadSegment => addRoadSegment.TemporaryId = 1);
            var data = Array.ConvertAll(addRoadSegments,
                addRoadSegment => new RequestedChange {AddRoadSegment = addRoadSegment});

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
