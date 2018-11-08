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
            Fixture.Customize<Messages.AddRoadNode>(
                composer =>
                    composer.FromFactory(random =>
                        new Messages.AddRoadNode
                        {
                            Id = random.Next(),
                            Type = Fixture.Create<Messages.RoadNodeType>(),
                            Geometry = Fixture.Create<PointM>().ToBytes()
                        }
                    )
            );
            Fixture.Customize<Messages.AddRoadSegment>(
                composer =>
                    composer.FromFactory(random =>
                        {
                            var geometry = Fixture.Create<MultiLineString>();
                            return new Messages.AddRoadSegment
                            {
                                Id = random.Next(),
                                StartNodeId = random.Next(),
                                EndNodeId = random.Next(),
                                Geometry = geometry.ToBytes(),
                                Maintainer = Fixture.Create<string>(),
                                GeometryDrawMethod = Fixture.Create<Messages.RoadSegmentGeometryDrawMethod>(),
                                Morphology = Fixture.Create<Messages.RoadSegmentMorphology>(),
                                Status = Fixture.Create<Messages.RoadSegmentStatus>(),
                                Category = Fixture.Create<Messages.RoadSegmentCategory>(),
                                AccessRestriction = Fixture.Create<Messages.RoadSegmentAccessRestriction>(),
                                LeftSideStreetNameId = Fixture.Create<Nullable<int>>(),
                                RightSideStreetNameId = Fixture.Create<Nullable<int>>(),
                                PartOfEuropeanRoads = Fixture.CreateMany<RequestedRoadSegmentEuropeanRoadAttributes>().ToArray(),
                                PartOfNationalRoads = Fixture.CreateMany<RequestedRoadSegmentNationalRoadAttributes>().ToArray(),
                                PartOfNumberedRoads = Fixture.CreateMany<RequestedRoadSegmentNumberedRoadAttributes>().ToArray(),
                                Lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttributes>().ToArray(),
                                Widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttributes>().ToArray(),
                                Surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttributes>().ToArray()
                            };
                        }
                    )
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
            Validator = new ChangeRoadNetworkValidator(new WellKnownBinaryReader());
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
            var data = Fixture.CreateMany<RequestedChange>().ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

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
