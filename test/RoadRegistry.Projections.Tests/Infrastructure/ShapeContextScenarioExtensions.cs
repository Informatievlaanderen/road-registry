namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.Connector.Testing;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.EntityFrameworkCore;
    using Xunit.Sdk;

    public static class ShapeContextScenarioExtensions
    {
        public static ConnectedProjectionScenario<ShapeContext> Scenario(this ConnectedProjection<ShapeContext> projection) =>
            new ConnectedProjectionScenario<ShapeContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));

        public static async Task Expect(
            this ConnectedProjectionScenario<ShapeContext> scenario,
            IEnumerable<object> records)
        {
            var specificationId = Guid.NewGuid();
            var expectedRecords = records.ToList();

            var specification = scenario.Verify(async context =>
            {
                var comparisonConfig = new ComparisonConfig { MaxDifferences = 5 };
                var comparer = new CompareLogic(comparisonConfig);
                var actualRecords = new List<object>(await context.AllRecordsToListAsync());
                var result = comparer.Compare(
                    actualRecords,
                    expectedRecords
                );

                return result.AreEqual
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, expectedRecords));
            });

            await specification.ApplyMessages(() => CreateContextFor(specificationId));

            using (var context = CreateContextFor(specificationId))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }

        private static async Task<List<object>> AllRecordsToListAsync(this ShapeContext context)
        {
            var records = new List<object>();
            records.AddRange(await context.RoadNodes.ToListAsync());
            records.AddRange(await context.RoadSegments.ToListAsync());
            records.AddRange(await context.RoadReferencePoints.ToListAsync());
            records.AddRange(await context.RoadLaneAttributes.ToListAsync());
            records.AddRange(await context.RoadWidthAttributes.ToListAsync());
            records.AddRange(await context.RoadHardeningAttributes.ToListAsync());
            records.AddRange(await context.EuropeanRoadAttributes.ToListAsync());
            records.AddRange(await context.NationalRoadAttributes.ToListAsync());
            records.AddRange(await context.NumberedRoadAttributes.ToListAsync());
            records.AddRange(await context.GradeSeperatedJunctions.ToListAsync());
            records.AddRange(await context.Organizations.ToListAsync());

            return records;
        }

        private static async Task ApplyMessages(this ConnectedProjectionTestSpecification<ShapeContext> specification, Func<ShapeContext> createContext)
        {
            using (var context = createContext())
            {
                var projector = new ConnectedProjector<ShapeContext>(specification.Resolver);
                foreach (var message in specification.Messages)
                {
                    var envelope = new Envelope(message, new Dictionary<string, object>()).ToGenericEnvelope();
                    await projector.ProjectAsync(context, envelope);
                }

                await context.SaveChangesAsync();
            }
        }

        private static ShapeContext CreateContextFor(Guid specificationId)
        {
            var databaseName = specificationId.ToString("N");
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseInMemoryDatabase(databaseName)
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
