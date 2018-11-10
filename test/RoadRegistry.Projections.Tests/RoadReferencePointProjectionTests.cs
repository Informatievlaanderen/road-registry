namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using Xunit;

    public class RoadReferencePointProjectionTests
    {
        private readonly Fixture _fixture;
        private readonly ReferencePointTypeTranslator _referencePointTypeTranslator;

        public RoadReferencePointProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadNodeType();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
            _fixture.CustomizePointM();
            _fixture.CustomizeOriginProperties();
            _fixture.Customize<ImportedReferencePoint>(
                customization =>
                    customization.FromFactory(generator =>
                        new ImportedReferencePoint
                        {
                            Id = generator.Next(),
                            Caption = Math.Round(generator.NextDouble() * 3.0, 2),
                            Version = _fixture.Create<int>(),
                            Geometry = new WellKnownBinaryWriter().Write(_fixture.Create<PointM>()),
                            Type = _fixture.Create<ReferencePointType>(),
                            Ident8 = new string('a', generator.Next(1, 9)),
                            Origin = _fixture.Create<OriginProperties>()
                        }).OmitAutoProperties()
                );

            _referencePointTypeTranslator = new ReferencePointTypeTranslator();
        }

        [Fact]
        public Task When_a_road_reference_points_were_imported()
        {
            var data = _fixture
                .CreateMany<ImportedReferencePoint>(new Random().Next(1,100))
                .Select(@event =>
                {
                    var pointShapeContent = new PointShapeContent(
                        new WellKnownBinaryReader().ReadAs<PointM>(@event.Geometry)
                    );

                    var expected = new RoadReferencePointRecord
                    {
                        Id = @event.Id,
                        ShapeRecordContent = pointShapeContent.ToBytes(),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                        Envelope = BoundingBox2D.From(pointShapeContent.Shape.EnvelopeInternal),
                        DbaseRecord = new RoadReferencePointDbaseRecord
                        {
                            RP_OIDN = { Value = @event.Id },
                            RP_UIDN = { Value = @event.Id + "_" + @event.Version },
                            IDENT8 = { Value = @event.Ident8 },
                            OPSCHRIFT = { Value = @event.Caption },
                            TYPE = { Value = _referencePointTypeTranslator.TranslateToIdentifier(@event.Type) },
                            LBLTYPE = { Value = _referencePointTypeTranslator.TranslateToDutchName(@event.Type) },
                            BEGINTIJD = { Value = @event.Origin.Since },
                            BEGINORG = { Value = @event.Origin.OrganizationId },
                            LBLBGNORG = { Value = @event.Origin.Organization }
                        }.ToBytes(Encoding.UTF8)
                    };

                    return new {
                        ImportedReferencePoint = @event,
                        Expected = expected
                    };
                }).ToList();

            return new RoadReferencePointRecordProjection(
                    new WellKnownBinaryReader(),
                    _referencePointTypeTranslator,
                    Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedReferencePoint))
                .Expect(data.Select(d => d.Expected));
        }
    }
}
