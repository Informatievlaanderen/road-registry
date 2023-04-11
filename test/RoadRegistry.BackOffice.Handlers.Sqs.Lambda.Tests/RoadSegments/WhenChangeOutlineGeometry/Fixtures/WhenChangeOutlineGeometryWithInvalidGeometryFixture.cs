namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using NodaTime;
using Hosts;

public class WhenChangeOutlineGeometryWithInvalidGeometryFixture : WhenChangeOutlineGeometryWithValidRequestFixture
{
    public WhenChangeOutlineGeometryWithInvalidGeometryFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        Request = new ChangeRoadSegmentOutlineGeometryRequest(
            new RoadSegmentId(TestData.Segment1Added.Id),
            GeometryTranslator.Translate(ObjectProvider.Create<MultiLineString>()) //TODO-rik create invalid geometry
        );
    }

    protected override ChangeRoadSegmentOutlineGeometryRequest Request { get; }

    protected override Task<bool> VerifyTicketAsync()
    {
        return Task.FromResult(VerifyThatTicketHasError("NotFound", "Dit wegsegment bestaat niet of heeft niet de geometriemethode ingeschetst."));
    }
}
