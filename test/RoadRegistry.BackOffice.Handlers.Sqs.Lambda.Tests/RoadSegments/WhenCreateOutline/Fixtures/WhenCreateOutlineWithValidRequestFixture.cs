namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Abstractions.Fixtures;
using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Hosts;
using Messages;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice;
using GeometryTranslator = BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;

public class WhenCreateOutlineWithValidRequestFixture : WhenCreateOutlineFixture
{
    public WhenCreateOutlineWithValidRequestFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        ObjectProvider.CustomizeRoadSegmentOutline();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();

        ObjectProvider.Customize<LineString>(customization =>
            customization.FromFactory(generator => new LineString(
                new CoordinateArraySequence(new Coordinate[] { new CoordinateM(0, 0, 0), new CoordinateM(10, 0, 10) }),
                GeometryConfiguration.GeometryFactory)
            ).OmitAutoProperties()
        );
    }

    protected override CreateRoadSegmentOutlineRequest Request => new(
        GeometryTranslator.Translate(ObjectProvider.Create<MultiLineString>()),
        ObjectProvider.Create<RoadSegmentStatus>(),
        ObjectProvider.Create<RoadSegmentMorphology>(),
        ObjectProvider.Create<RoadSegmentAccessRestriction>(),
        ObjectProvider.Create<OrganizationId>(),
        ObjectProvider.Create<RoadSegmentSurfaceType>(),
        ObjectProvider.Create<RoadSegmentWidth>(),
        ObjectProvider.Create<RoadSegmentLaneCount>(),
        ObjectProvider.Create<RoadSegmentLaneDirection>()
    );

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
    }

    protected override async Task<bool> VerifyTicketAsync()
    {
        var rejectCommand = await Store.GetLastCommandIfTypeIs<RoadNetworkChangesRejected>();
        if (rejectCommand != null)
        {
            var problems = rejectCommand.Changes.SelectMany(change => change.Problems).ToArray();
            if (problems.Any())
            {
                throw new Exception(string.Join(Environment.NewLine, problems.Select(x => x.ToString())));
            }
        }

        var roadSegmentId = new RoadSegmentId(1);

        await VerifyThatTicketHasCompleted(roadSegmentId);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        return command.Changes.Single().RoadSegmentAdded.Id == roadSegmentId;
    }
}
