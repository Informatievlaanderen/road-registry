namespace RoadRegistry.BackOffice.Framework.Testing.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.EntityFrameworkCore;
    using Schema;
    using Xunit.Sdk;

    public static class ShapeContextScenarioExtensions
    {
        public static ConnectedProjectionScenario<ShapeContext> Scenario(this ConnectedProjection<ShapeContext> projection) =>
            new ConnectedProjectionScenario<ShapeContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));

        public static async Task ExpectNone(this ConnectedProjectionScenario<ShapeContext> scenario)
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
                var projector = new ConnectedProjector<ShapeContext>(specification.Resolver);
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
            this ConnectedProjectionScenario<ShapeContext> scenario,
            IEnumerable<object> records)
        {
            return scenario.Expect(records.ToArray());
        }

        public static async Task Expect(
            this ConnectedProjectionScenario<ShapeContext> scenario,
            params object[] records)
        {
            var database = Guid.NewGuid().ToString("N");

            var specification = scenario.Verify(async context =>
            {
                var comparisonConfig = new ComparisonConfig { MaxDifferences = 5 };
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
                var projector = new ConnectedProjector<ShapeContext>(specification.Resolver);
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

        private static async Task<object[]> AllRecords(this ShapeContext context)
        {
            var records = new List<object>();
            records.AddRange(await context.RoadNodes.ToArrayAsync());
            records.AddRange(await context.RoadSegments.ToArrayAsync());
            records.AddRange(await context.RoadSegmentLaneAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentWidthAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentSurfaceAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentEuropeanRoadAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentNationalRoadAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentNumberedRoadAttributes.ToArrayAsync());
            records.AddRange(await context.GradeSeparatedJunctions.ToArrayAsync());
            records.AddRange(await context.Organizations.ToArrayAsync());
            records.AddRange(await context.RoadNetworkInfo.ToArrayAsync());
            return records.ToArray();
        }

        private static ShapeContext CreateContextFor(string database)
        {
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseInMemoryDatabase(database)
                .EnableSensitiveDataLogging()
                .Options;

            return new ShapeContext(options);
        }

        private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<ShapeContext> specification, VerificationResult result)
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
