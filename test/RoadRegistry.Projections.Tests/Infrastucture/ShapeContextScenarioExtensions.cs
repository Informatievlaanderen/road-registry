namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.Connector.Testing;
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
            var databaseName = Guid.NewGuid().ToString("N");
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging()
                .Options;

            var specification = scenario.Verify(async context =>
            {
                var comparer = new CompareLogic();
                var result = comparer.Compare(
                    new List<object>(await context.RoadNodes.ToListAsync()),
                    records.ToList()
                );
                return result.AreEqual
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail(result.DifferencesString);
            });

            using (var context = new ShapeContext(options))
            {
                var projector = new ConnectedProjector<ShapeContext>(specification.Resolver);
                foreach (var message in specification.Messages)
                {
                    await projector.ProjectAsync(context, message);
                }

                await context.SaveChangesAsync();
            }

            using (var context = new ShapeContext(options))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw new XunitException(result.Message);
            }
        }
    }
}
