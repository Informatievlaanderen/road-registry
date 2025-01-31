namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Abstractions.Fixtures;
using BackOffice.Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Hosts;
using Messages;
using Microsoft.Extensions.Configuration;
using Moq;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Extracts;
using TicketingService.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class WhenChangeAttributesWithDowngradedCategoryFixture : WhenChangeAttributesFixture
{
    public WhenChangeAttributesWithDowngradedCategoryFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        Request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.Category = RoadSegmentCategory.MainRoad;
            });
    }

    protected override ChangeRoadSegmentAttributesRequest Request { get; }

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.Category = RoadSegmentCategory.EuropeanMainRoad;

        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes =
            [
                new AcceptedChange
                {
                    RoadSegmentAdded = TestData.Segment1Added
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();
    }

    protected override Task<bool> VerifyTicketAsync()
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        var ticketError = new TicketError([
            new TicketError("Wegcategorie werd niet gewijzigd voor wegsegment 1 omdat het record reeds een recentere versie bevat.", "WegcategorieNietVeranderdHuidigeBevatRecentereVersie")
        ]);

        TicketingMock.Verify(x =>
            x.Error(It.IsAny<Guid>(),
                ticketError,
                CancellationToken.None));

        return Task.FromResult(true);
    }
}
