namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Hosts;
using Messages;
using Microsoft.Extensions.Configuration;
using NodaTime;
using NodaTime.Text;
using AcceptedChange = Messages.AcceptedChange;

public class WhenDeleteOutlineWithInvalidGeometryMethodFixture : WhenDeleteOutlineWithValidRequestFixture
{
    public WhenDeleteOutlineWithInvalidGeometryMethodFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
    }

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured.ToString();

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
                    RoadNodeAdded = TestData.StartNode1Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = TestData.EndNode1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = TestData.Segment1Added
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
    }

    protected override Task<bool> VerifyTicketAsync()
    {
        return Task.FromResult(VerifyThatTicketHasError("NotFound", "Dit wegsegment bestaat niet of heeft niet de geometriemethode 'ingeschetst'."));
    }
}
