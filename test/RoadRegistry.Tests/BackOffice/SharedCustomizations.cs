namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Dsl;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using System.Linq;
using NodaTime.Text;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = RoadRegistry.BackOffice.Messages.Point;
using Polygon = RoadRegistry.BackOffice.Messages.Polygon;
using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;
using RoadSegmentLaneAttribute = RoadRegistry.BackOffice.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = RoadRegistry.BackOffice.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = RoadRegistry.BackOffice.RoadSegmentWidthAttribute;

public static class SharedCustomizations
{
    public static void CustomizeArchiveId(this IFixture fixture)
    {
        fixture.Customize<ArchiveId>(composer =>
            composer.FromFactory(generator =>
                new ArchiveId(Guid.NewGuid().ToString("N"))
            )
        );
    }

    public static void CustomizeAttributeHash(this IFixture fixture)
    {
        fixture.Customize<AttributeHash>(
            customization => customization.FromFactory(
                generator =>
                {
                    var result = new AttributeHash(
                        fixture.Create<RoadSegmentAccessRestriction>(),
                        fixture.Create<RoadSegmentCategory>(),
                        fixture.Create<RoadSegmentMorphology>(),
                        fixture.Create<RoadSegmentStatus>(),
                        fixture.Create<StreetNameLocalId?>(),
                        fixture.Create<StreetNameLocalId?>(),
                        fixture.Create<OrganizationId>(),
                        fixture.Create<RoadSegmentGeometryDrawMethod>()
                    );
                    var times = generator.Next(0, 10);
                    for (var index = 0; index < times; index++)
                    {
                        switch (generator.Next(0, 8))
                        {
                            case 0:
                                result = result.With(fixture.Create<RoadSegmentCategory>());
                                break;
                            case 1:
                                result = result.With(fixture.Create<RoadSegmentMorphology>());
                                break;
                            case 2:
                                result = result.With(fixture.Create<RoadSegmentStatus>());
                                break;
                            case 3:
                                result = result.With(fixture.Create<RoadSegmentAccessRestriction>());
                                break;
                            case 4:
                                result = result.WithLeftSide(fixture.Create<StreetNameLocalId?>());
                                break;
                            case 5:
                                result = result.WithRightSide(fixture.Create<StreetNameLocalId?>());
                                break;
                            case 6:
                                result = result.With(fixture.Create<OrganizationId>());
                                break;
                            case 7:
                                result = result.With(fixture.Create<RoadSegmentGeometryDrawMethod>());
                                break;
                        }
                    }

                    return result;
                }));
    }

    public static void CustomizeAttributeId(this IFixture fixture)
    {
        fixture.Customize<AttributeId>(composer =>
            composer.FromFactory<int>(value => new AttributeId(Math.Abs(value)))
        );
    }

    public static void CustomizeChangeRequestId(this IFixture fixture)
    {
        fixture.Customize<ChangeRequestId>(composer =>
            composer.FromFactory(generator =>
                new ChangeRequestId(Enumerable.Range(0, ChangeRequestId.ExactLength).Select(index => (byte)generator.Next(0, 256)).ToArray())
            )
        );
    }

    public static void CustomizeCrabStreetnameId(this IFixture fixture)
    {
        fixture.Customize<StreetNameLocalId>(composer =>
            composer.FromFactory(generator => (generator.Next() % 3) switch
            {
                0 => new StreetNameLocalId(StreetNameLocalId.Unknown),
                1 => new StreetNameLocalId(StreetNameLocalId.NotApplicable),
                _ => new StreetNameLocalId(generator.Next(0, int.MaxValue))
            })
        );
    }

    public static void CustomizeDownloadId(this IFixture fixture)
    {
        fixture.Customize<DownloadId>(composer =>
            composer.FromFactory(generator =>
                new DownloadId(Guid.NewGuid()))
        );
    }

    public static void CustomizeEuropeanRoadNumber(this IFixture fixture)
    {
        fixture.Customize<EuropeanRoadNumber>(composer =>
            composer.FromFactory<int>(value => EuropeanRoadNumber.All[Math.Abs(value) % EuropeanRoadNumber.All.Length]));
    }

    public static void CustomizeExternalExtractRequestId(this IFixture fixture)
    {
        fixture.Customize<ExternalExtractRequestId>(composer =>
            composer.FromFactory(generator =>
                new ExternalExtractRequestId(new string(
                    (char)generator.Next(97, 123), // a-z
                    generator.Next(1, ExternalExtractRequestId.MaxLength))))
        );
    }

    public static void CustomizeExtractDescription(this IFixture fixture)
    {
        fixture.Customize<ExtractDescription>(composer =>
            composer.FromFactory(generator =>
                new ExtractDescription(new string(
                    (char)generator.Next(97, 123), // a-z
                    generator.Next(1, ExtractDescription.MaxLength + 1))))
        );
    }

    public static void CustomizeExtractRequestId(this IFixture fixture)
    {
        fixture.Customize<ExtractRequestId>(composer =>
            composer.FromFactory(generator =>
                new ExtractRequestId(Enumerable.Range(0, ExtractRequestId.ExactLength).Select(index => (byte)generator.Next(0, 256)).ToArray())
            )
        );
    }

    public static void CustomizeGradeSeparatedJunctionId(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionId>(composer =>
            composer.FromFactory<int>(value => new GradeSeparatedJunctionId(Math.Abs(value)))
        );
    }

    public static void CustomizeGradeSeparatedJunctionType(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionType>(composer =>
            composer.FromFactory<int>(value => GradeSeparatedJunctionType.All[value % GradeSeparatedJunctionType.All.Length]));
    }

    public static void CustomizeMultiPolygon(this IFixture fixture)
    {
        fixture.Customize<Ring>(customization =>
            customization.FromFactory(generator =>
            {
                var ring = new Ring
                {
                    Points = fixture.CreateMany<Point>().ToArray()
                };

                ring.Points = ring.Points.Append(ring.Points[0]).ToArray();

                return ring;
            }).OmitAutoProperties());

        fixture.Customize<Polygon>(customization =>
            customization.FromFactory(generator => new Polygon
            {
                Shell = fixture.Create<Ring>(),
                Holes = Array.Empty<Ring>()
            }).OmitAutoProperties());

        fixture.Customize<RoadNetworkExtractGeometry>(customization =>
            customization.FromFactory(generator =>
            {
                var geometry = new RoadNetworkExtractGeometry
                {
                    SpatialReferenceSystemIdentifier =
                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };

                if (generator.Next() % 2 == 0)
                {
                    geometry.MultiPolygon = new[] { fixture.Create<Polygon>() };
                    geometry.Polygon = null;
                }
                else
                {
                    geometry.Polygon = fixture.Create<Polygon>();
                    geometry.MultiPolygon = null;
                }

                return geometry;
            }).OmitAutoProperties()
        );
    }

    public static void CustomizeMunicipalityGeometry(this IFixture fixture)
    {
        fixture.Customize<Ring>(customization =>
            customization.FromFactory(generator =>
            {
                var ring = new Ring
                {
                    Points = fixture.CreateMany<Point>().ToArray()
                };

                ring.Points = ring.Points.Append(ring.Points[0]).ToArray();

                return ring;
            }).OmitAutoProperties());

        fixture.Customize<Polygon>(customization =>
            customization.FromFactory(generator => new Polygon
            {
                Shell = fixture.Create<Ring>(),
                Holes = Array.Empty<Ring>()
            }).OmitAutoProperties());

        fixture.Customize<MunicipalityGeometry>(customization =>
            customization.FromFactory(generator =>
            {
                var geometry = new MunicipalityGeometry
                {
                    MultiPolygon = new[] { fixture.Create<Polygon>() },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                return geometry;
            }).OmitAutoProperties()
        );
    }

    public static void CustomizeNationalRoadNumber(this IFixture fixture)
    {
        fixture.Customize<NationalRoadNumber>(composer =>
            composer.FromFactory<int>(value => NationalRoadNumbers.All[Math.Abs(value) % NationalRoadNumbers.All.Length]));
    }

    public static void CustomizeNumberedRoadNumber(this IFixture fixture)
    {
        fixture.Customize<NumberedRoadNumber>(composer =>
            composer.FromFactory<int>(value => NumberedRoadNumbers.All[Math.Abs(value) % NumberedRoadNumbers.All.Length]));
    }

    public static void CustomizeOperatorName(this IFixture fixture)
    {
        fixture.Customize<OperatorName>(composer =>
            composer.FromFactory(generator =>
                new OperatorName(new string(
                    (char)generator.Next(97, 123), // a-z
                    generator.Next(1, OperatorName.MaxLength + 1))))
        );
    }

    public static void CustomizeOrganisation(this IFixture fixture)
    {
        fixture.Customize<Organisation>(customization =>
            customization.FromSeed(_ => Organisation.DigitaalVlaanderen)
        );
    }

    public static void CustomizeOrganizationId(this IFixture fixture)
    {
        fixture.Customize<OrganizationId>(composer =>
            composer.FromFactory(generator =>
                new OrganizationId(new string(
                    (char)generator.Next(97, 123), // a-z
                    generator.Next(1, OrganizationId.MaxLength + 1))))
        );
    }

    public static void CustomizeOrganizationName(this IFixture fixture)
    {
        fixture.Customize<OrganizationName>(composer =>
            composer.FromFactory(generator =>
                new OrganizationName(new string(
                    (char)generator.Next(97, 123), // a-z
                    generator.Next(1, OrganizationName.MaxLength + 1))))
        );
    }

    public static void CustomizeOrganizationOvoCode(this IFixture fixture)
    {
        fixture.Customize<OrganizationOvoCode>(composer =>
            composer.FromFactory(generator =>
                new OrganizationOvoCode(generator.Next(1, OrganizationOvoCode.MaxDigitsValue))
            ));
    }

    public static void CustomizeOrganizationKboNumber(this IFixture fixture)
    {
        fixture.Customize<OrganizationKboNumber>(composer =>
            composer.FromFactory(generator =>
                new OrganizationKboNumber($"{generator.Next(1, 99999):00000}{generator.Next(1, 99999):00000}")
            ));
    }

    public static void CustomizeOriginProperties(this IFixture fixture)
    {
        fixture.Customize<ImportedOriginProperties>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedOriginProperties
                    {
                        Organization = fixture.Create<OrganizationName>(),
                        OrganizationId = fixture.Create<OrganizationId>(),
                        Operator = fixture.Create<OperatorName>(),
                        Application = fixture.Create<string>(),
                        Since = fixture.Create<DateTime>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizePoint(this IFixture fixture)
    {
        fixture.Customize<NetTopologySuite.Geometries.Point>(customization =>
            customization.FromFactory(generator =>
                new NetTopologySuite.Geometries.Point(
                    fixture.Create<double>(),
                    fixture.Create<double>()
                )
            ).OmitAutoProperties()
        );
    }

    public static void CustomizePolylineM(this IFixture fixture)
    {
        fixture.Customize<CoordinateM>(customization =>
            customization.FromFactory(generator =>
                new CoordinateM(
                    fixture.Create<double>(),
                    fixture.Create<double>(),
                    fixture.Create<double>()
                )
            ).OmitAutoProperties()
        );

        fixture.Customize<LineString>(customization =>
            customization.FromFactory(generator =>
                new LineString(
                    new CoordinateArraySequence(fixture.CreateMany<CoordinateM>(2).Cast<Coordinate>().ToArray()),
                    GeometryConfiguration.GeometryFactory)
            ).OmitAutoProperties()
        );

        fixture.Customize<MultiLineString>(customization =>
            customization.FromFactory(generator =>
                new MultiLineString(
                    fixture.CreateMany<LineString>(1).ToArray(),
                    GeometryConfiguration.GeometryFactory)
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeProvenanceData(this IFixture fixture)
    {
        fixture.Customize<ProvenanceData>(customization =>
            customization.FromSeed(_ => new ProvenanceData(new Provenance(new FakeClock(NodaConstants.UnixEpoch).GetCurrentInstant(),
                Application.RoadRegistry,
                new Reason("TEST"),
                new Operator("TEST"),
                Modification.Unknown,
                fixture.Create<Organisation>())))
        );
    }

    public static void CustomizeReason(this IFixture fixture)
    {
        fixture.Customize<RoadRegistry.BackOffice.Reason>(composer =>
            composer.FromFactory(generator =>
                new RoadRegistry.BackOffice.Reason(new string(
                    (char)generator.Next(97, 123), // a-z
                    generator.Next(1, RoadRegistry.BackOffice.Reason.MaxLength + 1))))
        );
    }

    public static void CustomizeRecordType(this IFixture fixture)
    {
        fixture.Customize<RecordType>(composer =>
            composer.FromFactory<int>(value => RecordType.All[Math.Abs(value) % RecordType.All.Length]));
    }

    public static void CustomizeRoadNetworkExtractGeometry(this IFixture fixture)
    {
        fixture.Customize<Ring>(customization =>
            customization.FromFactory(generator =>
            {
                var ring = new Ring
                {
                    Points = fixture.CreateMany<Point>().ToArray()
                };

                ring.Points = ring.Points.Append(ring.Points[0]).ToArray();

                return ring;
            }).OmitAutoProperties());

        fixture.Customize<Polygon>(customization =>
            customization.FromFactory(generator => new Polygon
            {
                Shell = fixture.Create<Ring>(),
                Holes = Array.Empty<Ring>()
            }).OmitAutoProperties());

        fixture.Customize<RoadNetworkExtractGeometry>(customization =>
            customization.FromFactory(generator =>
            {
                var geometry = new RoadNetworkExtractGeometry
                {
                    SpatialReferenceSystemIdentifier =
                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };

                if (generator.Next() % 2 == 0)
                {
                    geometry.MultiPolygon = new[] { fixture.Create<Polygon>() };
                    geometry.Polygon = null;
                }
                else
                {
                    geometry.Polygon = fixture.Create<Polygon>();
                    geometry.MultiPolygon = null;
                }

                return geometry;
            }).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeId(this IFixture fixture)
    {
        fixture.Customize<RoadNodeId>(composer =>
            composer.FromFactory(generator => new RoadNodeId(generator.Next(1, 1000000)))
        );
    }

    public static void CustomizeRoadNodeType(this IFixture fixture)
    {
        fixture.Customize<RoadNodeType>(composer =>
            composer.FromFactory<int>(value => RoadNodeType.All[Math.Abs(value) % RoadNodeType.All.Length]));
    }

    public static void CustomizeRoadNodeVersion(this IFixture fixture)
    {
        fixture.Customize<RoadNodeVersion>(composer =>
            composer.FromFactory<int>(value => new RoadNodeVersion(Math.Abs(value)))
        );
    }

    public static void CustomizeRoadSegmentAccessRestriction(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAccessRestriction>(customization =>
            customization.FromFactory(generator =>
                RoadSegmentAccessRestriction.All[generator.Next() % RoadSegmentAccessRestriction.All.Length]
            )
        );
    }

    public static void CustomizeRoadSegmentCategory(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentCategory>(customization =>
            customization.FromFactory(generator =>
                {
                    var allowedValues = RoadSegmentCategory.All
                        .Where(x => !RoadSegmentCategory.IsUpgraded(x))
                        .ToArray();
                    return allowedValues[generator.Next() % allowedValues.Length];
                }
            )
        );
    }

    public static void CustomizeRoadSegmentGeometry(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentGeometry>(customizer =>
            customizer.FromFactory(_ =>
            {
                var lineString = fixture.Create<RoadRegistry.BackOffice.Messages.LineString>();
                var previousPoint = new Coordinate(lineString.Points[0].X, lineString.Points[0].Y);
                var measure = 0.0;
                lineString.Measures = lineString.Points.Select(point =>
                {
                    var currentPoint = new Coordinate(point.X, point.Y);
                    measure += previousPoint.Distance(currentPoint);
                    previousPoint = currentPoint;
                    return measure;
                }).ToArray();

                return new RoadSegmentGeometry
                {
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                    MultiLineString = new[]
                    {
                        lineString
                    }
                };
            }).OmitAutoProperties());
    }

    public static void CustomizeRoadSegmentGeometryDrawMethod(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentGeometryDrawMethod>(customization =>
            customization.FromFactory(generator => RoadSegmentGeometryDrawMethod.Measured)
        );
    }

    public static void CustomizeRoadSegmentGeometryVersion(this IFixture fixture)
    {
        fixture.Customize<GeometryVersion>(composer =>
            composer.FromFactory<int>(value => new GeometryVersion(Math.Abs(value)))
        );
    }

    public static void CustomizeRoadSegmentId(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentId>(composer =>
            composer.FromFactory<int>(value => new RoadSegmentId(Math.Abs(value)))
        );
    }

    public static void CustomizeRoadSegmentLaneAttribute(this IFixture fixture)
    {
        fixture.Customize<RoadRegistry.BackOffice.Core.RoadSegmentLaneAttribute>(customization =>
            customization.FromFactory<int>(
                _ =>
                {
                    var generator = new Generator<RoadSegmentPosition>(fixture);
                    var from = generator.First();
                    var to = generator.First(candidate => candidate > from);
                    return new RoadRegistry.BackOffice.Core.RoadSegmentLaneAttribute(
                        fixture.Create<AttributeId>(),
                        fixture.Create<AttributeId>(),
                        fixture.Create<RoadSegmentLaneCount>(),
                        fixture.Create<RoadSegmentLaneDirection>(),
                        from,
                        to,
                        fixture.Create<GeometryVersion>()
                    );
                }
            )
        );

        fixture.Customize<RoadSegmentLaneAttribute>(customization =>
            customization.FromFactory<int>(
                _ =>
                {
                    var laneAttribute = fixture.Create<RoadRegistry.BackOffice.Core.RoadSegmentLaneAttribute>();

                    return new RoadSegmentLaneAttribute(
                        laneAttribute.From,
                        laneAttribute.To,
                        laneAttribute.Count,
                        laneAttribute.Direction,
                        laneAttribute.AsOfGeometryVersion
                    );
                }
            )
        );
    }

    public static void CustomizeRoadSegmentPositionAttributesBuilder(this IFixture fixture)
    {
        fixture.Customize<Func<double, RoadSegmentPositionAttribute[]>>(customization =>
            customization.FromFactory(
                generator =>
                {
                    return geometryLength =>
                    {
                        var recordsCount = generator.Next(1, 10);
                        var currentPosition = RoadSegmentPosition.Zero;
                        var maxPosition = RoadSegmentPosition.FromDouble(geometryLength);
                        var attributeLength = maxPosition / (decimal)recordsCount;

                        return Enumerable.Range(1, recordsCount)
                            .Select(_ =>
                            {
                                var nextPosition = new RoadSegmentPosition(currentPosition + attributeLength);
                                var attribute = new RoadSegmentPositionAttribute
                                {
                                    From = currentPosition,
                                    To = nextPosition
                                };
                                currentPosition = nextPosition;
                                return attribute;
                            })
                            .ToArray();
                    };
                }
            )
        );
    }

    public static void CustomizeRoadSegmentLaneCount(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentLaneCount>(customization =>
            customization.FromFactory<int>(
                value => value == RoadSegmentLaneCount.Unknown || value == RoadSegmentLaneCount.NotApplicable
                    ? new RoadSegmentLaneCount(value)
                    : new RoadSegmentLaneCount(Math.Abs(value) % RoadSegmentLaneCount.Maximum.ToInt32())
            )
        );
    }

    public static void CustomizeRoadSegmentOutlineLaneCount(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentLaneCount>(customization =>
            customization.FromFactory(generator =>
            {
                var value = generator.Next(-1, RoadSegmentLaneCount.Maximum + 1);
                return value == -1 ? RoadSegmentLaneCount.NotApplicable
                    : value == 0 ? RoadSegmentLaneCount.Unknown
                    : new RoadSegmentLaneCount(value);
            })
        );
    }

    public static void CustomizeRoadSegmentLaneDirection(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentLaneDirection>(customization =>
            customization.FromFactory(generator =>
                RoadSegmentLaneDirection.All[generator.Next() % RoadSegmentLaneDirection.All.Length]
            )
        );
    }

    public static void CustomizeRoadSegmentMorphology(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentMorphology>(customization =>
            customization.FromFactory(generator =>
                RoadSegmentMorphology.All[generator.Next() % RoadSegmentMorphology.All.Length]
            )
        );
    }

    public static void CustomizeRoadSegmentNumberedRoadDirection(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentNumberedRoadDirection>(customization =>
            customization.FromFactory(generator =>
                RoadSegmentNumberedRoadDirection.All[generator.Next() % RoadSegmentNumberedRoadDirection.All.Length]
            )
        );
    }

    public static void CustomizeRoadSegmentNumberedRoadOrdinal(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentNumberedRoadOrdinal>(customization =>
            customization.FromFactory<int>(
                value => new RoadSegmentNumberedRoadOrdinal(Math.Abs(value))
            )
        );
    }

    public static void CustomizeRoadSegmentOutlineGeometryDrawMethod(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentGeometryDrawMethod>(customization =>
            customization.FromFactory(_ => RoadSegmentGeometryDrawMethod.Outlined)
        );
    }

    public static void CustomizeRoadSegmentOutlineMorphology(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentMorphology>(customization =>
            customization.FromFactory(generator =>
                {
                    var valid = RoadSegmentMorphology.Edit.Editable.ToArray();
                    return valid[generator.Next() % valid.Length];
                }
            )
        );
    }

    public static void CustomizeRoadSegmentOutlineStatus(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentStatus>(customization =>
            customization.FromFactory(generator =>
                {
                    var valid = RoadSegmentStatus.Edit.Editable.ToArray();
                    return valid[generator.Next() % valid.Length];
                }
            )
        );
    }

    public static void CustomizeRoadSegmentOutline(this IFixture fixture)
    {
        fixture.CustomizeRoadSegmentOutlineGeometryDrawMethod();
        fixture.CustomizeRoadSegmentOutlineMorphology();
        fixture.CustomizeRoadSegmentOutlineStatus();
        fixture.CustomizeRoadSegmentOutlineLaneCount();
        fixture.CustomizeRoadSegmentOutlineWidth();
    }

    public static void CustomizeRoadSegmentPosition(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentPosition>(customization =>
            customization.FromFactory<decimal>(
                value => new RoadSegmentPosition(Math.Abs(value).ToRoundedMeasurement())
            )
        );
    }

    public static void CustomizeRoadSegmentStatus(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentStatus>(customization =>
            customization.FromFactory(generator =>
                RoadSegmentStatus.All[generator.Next() % RoadSegmentStatus.All.Length]
            )
        );
    }

    public static void CustomizeRoadSegmentSurfaceAttribute(this IFixture fixture)
    {
        fixture.Customize<RoadRegistry.BackOffice.Core.RoadSegmentSurfaceAttribute>(customization =>
            customization.FromFactory<int>(
                value =>
                {
                    var generator = new Generator<RoadSegmentPosition>(fixture);
                    var from = generator.First();
                    var to = generator.First(candidate => candidate > from);
                    return new RoadRegistry.BackOffice.Core.RoadSegmentSurfaceAttribute(
                        fixture.Create<AttributeId>(),
                        fixture.Create<AttributeId>(),
                        fixture.Create<RoadSegmentSurfaceType>(),
                        from,
                        to,
                        fixture.Create<GeometryVersion>()
                    );
                }
            )
        );

        fixture.Customize<RoadSegmentSurfaceAttribute>(customization =>
            customization.FromFactory<int>(
                _ =>
                {
                    var surfaceAttribute = fixture.Create<RoadRegistry.BackOffice.Core.RoadSegmentSurfaceAttribute>();

                    return new RoadSegmentSurfaceAttribute(
                        surfaceAttribute.From,
                        surfaceAttribute.To,
                        surfaceAttribute.Type,
                        surfaceAttribute.AsOfGeometryVersion
                    );
                }
            )
        );
    }

    public static void CustomizeRoadSegmentSurfaceType(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentSurfaceType>(customization =>
            customization.FromFactory(generator =>
                RoadSegmentSurfaceType.All[generator.Next() % RoadSegmentSurfaceType.All.Length]
            )
        );
    }

    public static void CustomizeRoadSegmentWidth(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWidth>(customization =>
            customization.FromFactory<int>(
                value => value == RoadSegmentWidth.Unknown || value == RoadSegmentWidth.NotApplicable
                    ? new RoadSegmentWidth(value)
                    : new RoadSegmentWidth(Math.Abs(value) % RoadSegmentWidth.Maximum.ToInt32())
            )
        );
    }

    public static void CustomizeRoadSegmentOutlineWidth(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWidth>(customization =>
            customization.FromFactory(generator =>
            {
                var value = generator.Next(-1, RoadSegmentWidth.Maximum + 1);
                return value == -1 ? RoadSegmentWidth.NotApplicable
                    : value == 0 ? RoadSegmentWidth.Unknown
                    : new RoadSegmentWidth(value);
            })
        );
    }

    public static void CustomizeRoadSegmentWidthAttribute(this IFixture fixture)
    {
        fixture.Customize<RoadRegistry.BackOffice.Core.RoadSegmentWidthAttribute>(customization =>
            customization.FromFactory<int>(
                value =>
                {
                    var generator = new Generator<RoadSegmentPosition>(fixture);
                    var from = generator.First();
                    var to = generator.First(candidate => candidate > from);
                    return new RoadRegistry.BackOffice.Core.RoadSegmentWidthAttribute(
                        fixture.Create<AttributeId>(),
                        fixture.Create<AttributeId>(),
                        fixture.Create<RoadSegmentWidth>(),
                        from,
                        to,
                        fixture.Create<GeometryVersion>()
                    );
                }
            )
        );

        fixture.Customize<RoadSegmentWidthAttribute>(customization =>
            customization.FromFactory<int>(
                _ =>
                {
                    var widthAttribute = fixture.Create<RoadRegistry.BackOffice.Core.RoadSegmentWidthAttribute>();

                    return new RoadSegmentWidthAttribute(
                        widthAttribute.From,
                        widthAttribute.To,
                        widthAttribute.Width,
                        widthAttribute.AsOfGeometryVersion
                    );
                }
            )
        );
    }

    public static void CustomizeTransactionId(this IFixture fixture)
    {
        fixture.Customize<TransactionId>(composer =>
            composer.FromFactory(generator => new TransactionId(generator.Next()))
        );
    }

    public static void CustomizeStreetNameLocalId(this IFixture fixture)
    {
        fixture.Customize<StreetNameLocalId>(composer =>
            composer.FromFactory<int>(value => new StreetNameLocalId(Math.Abs(value)))
        );
    }

    public static IPostprocessComposer<T> FromFactory<T>(this IFactoryComposer<T> composer, Func<Random, T> factory)
    {
        return composer.FromFactory<int>(value => factory(new Random(value)));
    }
}
