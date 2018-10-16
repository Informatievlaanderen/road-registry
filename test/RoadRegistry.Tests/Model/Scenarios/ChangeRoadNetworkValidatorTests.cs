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
            Fixture.Customize<Commands.AddRoadNode>(
                composer =>
                    composer.FromFactory(random =>
                        new Commands.AddRoadNode 
                        {
                            Id = random.Next(),
                            Type = Fixture.Create<Shared.RoadNodeType>(),
                            Geometry = Fixture.Create<PointM>().ToBytes()
                        }
                    )
            );
            Fixture.Customize<Commands.AddRoadSegment>(
                composer =>
                    composer.FromFactory(random =>
                        {
                            var geometry = Fixture.Create<MultiLineString>();
                            return new Commands.AddRoadSegment 
                            {
                                Id = random.Next(),
                                StartNodeId = random.Next(),
                                EndNodeId = random.Next(),
                                Geometry = geometry.ToBytes(),
                                Maintainer = Fixture.Create<String>(),
                                GeometryDrawMethod = Fixture.Create<Shared.RoadSegmentGeometryDrawMethod>(),
                                Morphology = Fixture.Create<Shared.RoadSegmentMorphology>(),
                                Status = Fixture.Create<Shared.RoadSegmentStatus>(),
                                Category = Fixture.Create<Shared.RoadSegmentCategory>(),
                                AccessRestriction = Fixture.Create<Shared.RoadSegmentAccessRestriction>(),
                                LeftSideStreetNameId = Fixture.Create<Nullable<Int32>>(),
                                RightSideStreetNameId = Fixture.Create<Nullable<Int32>>(),
                                PartOfEuropeanRoads = Fixture.CreateMany<Commands.RoadSegmentEuropeanRoadProperties>().ToArray(),
                                PartOfNationalRoads = Fixture.CreateMany<Commands.RoadSegmentNationalRoadProperties>().ToArray(),
                                PartOfNumberedRoads = Fixture.CreateMany<Commands.RoadSegmentNumberedRoadProperties>().ToArray(),
                                Lanes = Fixture.CreateMany<Commands.RoadSegmentLaneProperties>().ToArray(),
                                Widths = Fixture.CreateMany<Commands.RoadSegmentWidthProperties>().ToArray(),
                                Hardenings = Fixture.CreateMany<Commands.RoadSegmentHardeningProperties>().ToArray()
                            };
                        }
                    )
            );
            Fixture.Customize<Commands.RoadNetworkChange>(
                composer =>
                    composer.FromFactory(random =>
                        {
                            var result = new Commands.RoadNetworkChange();
                            switch(random.Next(0, 2))
                            {
                                case 0:
                                    result.AddRoadNode = Fixture.Create<Commands.AddRoadNode>();
                                    break;
                                case 1:
                                    result.AddRoadSegment = Fixture.Create<Commands.AddRoadSegment>();
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
            Validator.ShouldHaveValidationErrorFor(c => c.Changes, (Commands.RoadNetworkChange[]) null);
        }

        [Fact]
        public void ChangeCanNotBeNull()
        {
            var changes = Fixture.CreateMany<Commands.RoadNetworkChange>().ToArray();
            var index = new Random().Next(0, changes.Length + 1);
            changes[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.Changes, changes);
        }

        // [Fact]
        // public void VerifyValid()
        // {
            

        //     Validator.ValidateAndThrow(data);
        // }
    }
}