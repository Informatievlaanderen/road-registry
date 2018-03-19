namespace RoadRegistry.Projections.Oslo.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.Connector.Testing;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Microsoft.EntityFrameworkCore;
    using RoadList;
    using ValueObjects;
    using RoadRegistry.Road.Events;
    using Xunit;

    public class When_building_RoadList_projection
    {
        [Fact]
        public async Task Given_no_messages_Then_list_is_empty()
        {
            var projection = new RoadListProjections();
            var resolver = ConcurrentResolve.WhenEqualToHandlerMessageType(projection.Handlers);

            await new ConnectedProjectionScenario<OsloContext>(resolver)
                .Given()
                .Verify(async context =>
                    await context.RoadList.AnyAsync()
                        ? VerificationResult.Fail()
                        : VerificationResult.Pass())
                .Assert();
        }

        [Fact]
        public async Task Given_RoadWasRegistered_Then_expected_item_is_added()
        {
            var projection = new RoadListProjections();
            var resolver = ConcurrentResolve.WhenEqualToHandlerMessageType(projection.Handlers);

            var roadId = Guid.NewGuid();
            await new ConnectedProjectionScenario<OsloContext>(resolver)
                .Given(new Envelope<RoadWasRegistered>(new Envelope(new RoadWasRegistered(new RoadId(roadId)), new ConcurrentDictionary<string, object>())))
                .Verify(async context =>
                {
                    var road = await context.RoadList.FirstAsync(a => a.RoadId == roadId);

                    road.RoadId
                        .Should()
                        .Be(roadId);

                    return VerificationResult.Pass();
                })
                .Assert();
        }
    }

    public static class ConnectedProjectionTestSpecificationExtensions
    {
        public static async Task Assert(this ConnectedProjectionTestSpecification<OsloContext> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            var options = new DbContextOptionsBuilder<OsloContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new OsloContext(options))
            {
                context.Database.EnsureCreated();

                foreach (var message in specification.Messages)
                {
                    await new ConnectedProjector<OsloContext>(specification.Resolver)
                        .ProjectAsync(context, message);

                    await context.SaveChangesAsync();
                }

                var result = await specification.Verification(context, CancellationToken.None);
                if (result.Failed)
                {
                    throw new AssertionFailedException(result.Message);
                }
            }
        }
    }
}
