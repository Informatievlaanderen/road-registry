namespace RoadRegistry.Framework.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using KellermanSoftware.CompareNetObjects;
    using KellermanSoftware.CompareNetObjects.TypeComparers;
    using Microsoft.EntityFrameworkCore;
    using Syndication.Schema;
    using Xunit.Sdk;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public static class SyndicationContextScenarioExtensions
    {
        public static ConnectedProjectionScenario<SyndicationContext> Scenario(this ConnectedProjection<SyndicationContext> projection) =>
            new ConnectedProjectionScenario<SyndicationContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));

        public static async Task ExpectNone(this ConnectedProjectionScenario<SyndicationContext> scenario)
        {
            var database = Guid.NewGuid().ToString("N");

            var specification = scenario.Verify(async context =>
            {
                var actualRecords = await context.AllRecords();
                return actualRecords.Length == 0
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail($"Expected 0 records but found {actualRecords.Length}.");
            });

            using (var context = CreateContextFor(database))
            {
                var projector = new ConnectedProjector<SyndicationContext>(specification.Resolver);
                foreach (var message in specification.Messages)
                {
                    var envelope = new Envelope(message, new Dictionary<string, object>()).ToGenericEnvelope();
                    await projector.ProjectAsync(context, envelope);
                }
                await context.SaveChangesAsync();
            }

            using (var context = CreateContextFor(database))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }

        public static Task Expect(
            this ConnectedProjectionScenario<SyndicationContext> scenario,
            IEnumerable<object> records)
        {
            return scenario.Expect(records.ToArray());
        }

        public static async Task Expect(
            this ConnectedProjectionScenario<SyndicationContext> scenario,
            params object[] records)
        {
            var database = Guid.NewGuid().ToString("N");

            var specification = scenario.Verify(async context =>
            {
                var comparisonConfig = new ComparisonConfig
                {
                    MaxDifferences = 10,
                    CustomComparers = new List<BaseTypeComparer>
                    {
                        new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
                    }
                };
                var comparer = new CompareLogic(comparisonConfig);
                var actualRecords = await context.AllRecords();
                var result = comparer.Compare(
                    actualRecords,
                    records
                );

                return result.AreEqual
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
            });

            using (var context = CreateContextFor(database))
            {
                var projector = new ConnectedProjector<SyndicationContext>(specification.Resolver);
                var position = 0L;
                foreach (var message in specification.Messages)
                {
                    var envelope = new Envelope(message, new Dictionary<string, object> { { "Position", position }}).ToGenericEnvelope();
                    await projector.ProjectAsync(context, envelope);
                    position++;
                }

                await context.SaveChangesAsync();
            }

            using (var context = CreateContextFor(database))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }

        private static async Task<object[]> AllRecords(this SyndicationContext context)
        {
            var records = new List<object>();
            records.AddRange(await context.Municipalities.ToArrayAsync());
            records.AddRange(await context.StreetNames.ToArrayAsync());
            return records.ToArray();
        }

        private static SyndicationContext CreateContextFor(string database)
        {
            var options = new DbContextOptionsBuilder<SyndicationContext>()
                .UseInMemoryDatabase(database)
                .EnableSensitiveDataLogging()
                .Options;

            return new SyndicationContext(options);
        }

        private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<SyndicationContext> specification, VerificationResult result)
        {
            var title = string.Empty;
            var exceptionMessage = new StringBuilder()
                .AppendLine(title)
                .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
                .Append(result.Message);

            return new XunitException(exceptionMessage.ToString());
        }
    }
}
