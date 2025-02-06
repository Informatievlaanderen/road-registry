namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Core;
using Framework;
using Handlers;
using Hosts;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using Moq;
using Requests;
using RoadRegistry.StreetName;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;

public class WhenChangeAttributesTestBase : BackOfficeLambdaTest
{
    protected async Task<IReadOnlyList<ITranslatedChange>> HandleRequest(ChangeRoadSegmentAttributesRequest request)
    {
        var sqsRequest = new ChangeRoadSegmentAttributesSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new ChangeRoadSegmentAttributesSqsLambdaRequest(RoadNetwork.Identifier.ToString(), sqsRequest);

        var translatedChanges = TranslatedChanges.Empty;
        var changeRoadNetworkDispatcherMock = new Mock<IChangeRoadNetworkDispatcher>();
        changeRoadNetworkDispatcherMock
            .Setup(x => x.DispatchAsync(
                It.IsAny<SqsLambdaRequest>(),
                It.IsAny<string>(),
                It.IsAny<Func<TranslatedChanges, Task<TranslatedChanges>>>(),
                It.IsAny<CancellationToken>()))
            .Callback(
                (SqsLambdaRequest _, string _, Func<TranslatedChanges, Task<TranslatedChanges>> builder, CancellationToken _) =>
                {
                    translatedChanges = builder(translatedChanges).GetAwaiter().GetResult();
                });

        var sqsLambdaRequestHandler = new ChangeRoadSegmentAttributesSqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            changeRoadNetworkDispatcherMock.Object,
            EditorContext,
            new RecyclableMemoryStreamManager(),
            FileEncoding.UTF8,
            OrganizationCache,
            new StreetNameCacheClient(new FakeStreetNameCache()
                .AddStreetName(WellKnownStreetNameIds.Proposed, "Proposed street", "voorgesteld")
                .AddStreetName(WellKnownStreetNameIds.Current, "Current street", "inGebruik")
                .AddStreetName(WellKnownStreetNameIds.Retired, "Retired street", "gehistoreerd")
                .AddStreetName(WellKnownStreetNameIds.Null, "Not found street", null)
            ),
            new NullLogger<ChangeRoadSegmentAttributesSqsLambdaRequestHandler>());

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);

        return translatedChanges.ToList();
    }

    protected static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
        public const int Null = 4;
    }
}
