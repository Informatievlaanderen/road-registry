namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.Connector.Testing;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
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
                var result = comparer.Compare(
                    new List<object>(await context.RoadNodes.ToListAsync()),
                    expectedRecords
                );

                return result.AreEqual
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail(result.CreateDifferenceMessage());
            });

            await specification.ApplyMessages(() => CreateContextFor(specificationId));

            using (var context = CreateContextFor(specificationId))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw new XunitException(CreateExceptionMessage(specification.Messages, expectedRecords, result.Message));
            }
        }


        private static async Task ApplyMessages(this ConnectedProjectionTestSpecification<ShapeContext> specification, Func<ShapeContext> createContext)
        {
            using (var context = createContext())
            {
                var projector = new ConnectedProjector<ShapeContext>(specification.Resolver);
                foreach (var message in specification.Messages)
                {
                    await projector.ProjectAsync(context, message);
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

        private static string CreateExceptionMessage(IEnumerable given, IEnumerable expected, string differenceMessage)
        {
            var title = string.Empty;
            string NamedMessageFormat(object message) => $"{message.GetType().Name} - {JsonConvert.SerializeObject(message, Formatting.Indented)}";

            var exceptionMessage = new StringBuilder().AppendLine(title);

            exceptionMessage
                .AppendLine("Given:")
                .AppendLines(given, NamedMessageFormat)
                .AppendLine();

            exceptionMessage
                .AppendLine("Expected:")
                .AppendLines(expected, NamedMessageFormat)
                .AppendLine();

            exceptionMessage
                .AppendLine("But:")
                .Append(differenceMessage);

            return exceptionMessage.ToString();
        }
    }
}
