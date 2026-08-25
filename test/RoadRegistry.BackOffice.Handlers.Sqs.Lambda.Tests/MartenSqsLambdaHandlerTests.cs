namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using TicketingService.Abstractions;

// Every action recovers its response from the persisted scoped road network aggregate. An action that turns out to
// change nothing writes no events at all, so that aggregate is never created - which used to be dereferenced blindly
// and threw a NullReferenceException on what is a successful no-op. The guard lives on the shared base class, so it
// is covered once here for every action that uses it.
public class MartenSqsLambdaHandlerTests
{
    [Fact]
    public async Task WhenTheScopedRoadNetworkWasNeverWritten_ThenTheSummaryIsEmpty()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var handler = new TestHandler(store);

        var summary = await handler.GetSummary(new ScopedRoadNetworkId(Guid.NewGuid()), CancellationToken.None);

        summary.Should().NotBeNull();
        summary.RoadSegments.Added.Should().BeEmpty();
        summary.RoadSegments.Modified.Should().BeEmpty();
        summary.RoadSegments.Removed.Should().BeEmpty();
        summary.RoadNodes.Added.Should().BeEmpty();
        summary.GradeJunctions.Added.Should().BeEmpty();
        summary.GradeSeparatedJunctions.Added.Should().BeEmpty();
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        return storeOptions;
    }

    // The base class carries the helper; nothing here handles a request, so the request type is irrelevant.
    private sealed class TestHandler : MartenSqsLambdaHandler<SqsLambdaRequest>
    {
        public TestHandler(IDocumentStore store)
            : base(new FakeSqsLambdaHandlerOptions(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                Mock.Of<IIdempotentCommandHandler>(),
                store,
                new NullLoggerFactory())
        {
        }

        public Task<RoadNetworkChangesSummary> GetSummary(ScopedRoadNetworkId id, CancellationToken cancellationToken)
            => GetSummaryOfLastChange(id, cancellationToken);

        protected override Task<object> InnerHandle(SqsLambdaRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
