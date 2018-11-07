namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class ChangeRoadNetworkValidatorTests
    {
        public ChangeRoadNetworkValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizePointM();
            Fixture.CustomizePolylineM();
            Fixture.Customize<Shared.AddRoadNode>(
                composer =>
                    composer.FromFactory(random =>
                        new Shared.AddRoadNode 
                        {
                            Id = random.Next(),
                            Type = Fixture.Create<Shared.RoadNodeType>(),
                            Geometry = Fixture.Create<PointM>().ToBytes()
                        }
                    )
            );
            Fixture.Customize<Shared.AddRoadSegment>(
                composer =>
                    composer.FromFactory(random =>
                        {
                            var geometry = Fixture.Create<MultiLineString>();
                            return new Shared.AddRoadSegment 
                            {
                                Id = random.Next(),
                                StartNodeId = random.Next(),
                                EndNodeId = random.Next(),
                                Geometry = geometry.ToBytes(),
                                Maintainer = Fixture.Create<string>(),
                                GeometryDrawMethod = Fixture.Create<Shared.RoadSegmentGeometryDrawMethod>(),
                                Morphology = Fixture.Create<Shared.RoadSegmentMorphology>(),
                                Status = Fixture.Create<Shared.RoadSegmentStatus>(),
                                Category = Fixture.Create<Shared.RoadSegmentCategory>(),
                                AccessRestriction = Fixture.Create<Shared.RoadSegmentAccessRestriction>(),
                                LeftSideStreetNameId = Fixture.Create<Nullable<int>>(),
                                RightSideStreetNameId = Fixture.Create<Nullable<int>>(),
                                PartOfEuropeanRoads = Fixture.CreateMany<Shared.RoadSegmentEuropeanRoadProperties>().ToArray(),
                                PartOfNationalRoads = Fixture.CreateMany<Shared.RoadSegmentNationalRoadProperties>().ToArray(),
                                PartOfNumberedRoads = Fixture.CreateMany<Shared.RoadSegmentNumberedRoadProperties>().ToArray(),
                                Lanes = Fixture.CreateMany<Shared.RoadSegmentLaneProperties>().ToArray(),
                                Widths = Fixture.CreateMany<Shared.RoadSegmentWidthProperties>().ToArray(),
                                Hardenings = Fixture.CreateMany<Shared.RoadSegmentHardeningProperties>().ToArray()
                            };
                        }
                    )
            );
            Fixture.Customize<Commands.RequestedChange>(
                composer =>
                    composer.FromFactory(random =>
                        {
                            var result = new Commands.RequestedChange();
                            switch(random.Next(0, 2))
                            {
                                case 0:
                                    result.AddRoadNode = Fixture.Create<Shared.AddRoadNode>();
                                    break;
                                case 1:
                                    result.AddRoadSegment = Fixture.Create<Shared.AddRoadSegment>();
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
            Validator.ShouldHaveValidationErrorFor(c => c.Changes, (Commands.RequestedChange[]) null);
        }

        [Fact]
        public void ChangeCanNotBeNull()
        {
            var data = Fixture.CreateMany<Commands.RequestedChange>().ToArray();
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