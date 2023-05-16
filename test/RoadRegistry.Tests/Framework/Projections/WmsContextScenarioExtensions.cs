namespace RoadRegistry.Tests.Framework.Projections;

using System.Text;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Wms.Schema;
using Xunit.Sdk;

public static class WmsContextScenarioExtensions
{
    private static async Task<object[]> AllRecords(this WmsContext context)
    {
        var records = new List<object>();
        records.AddRange(await context.RoadSegments.ToArrayAsync());
        records.AddRange(await context.RoadSegmentEuropeanRoadAttributes.ToArrayAsync());
        records.AddRange(await context.RoadSegmentNationalRoadAttributes.ToArrayAsync());
        return records.ToArray();
    }

    private static WmsContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<WmsContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new WmsContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<WmsContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<WmsContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.Expect(records.ToArray());
    }

    public static async Task Expect(
        this ConnectedProjectionScenario<WmsContext> scenario,
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
                    new GeometryLineStringComparer(RootComparerFactory.GetRootComparer()),
                    new DateTimeComparer(RootComparerFactory.GetRootComparer())
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

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<WmsContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { "Position", position } }).ToGenericEnvelope();
                await projector.ProjectAsync(context, envelope);
                position++;
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContextFor(database))
        {
            var result = await specification.Verification(context, CancellationToken.None);

            if (result.Failed)
            {
                throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }
    }

    public static async Task ExpectNone(this ConnectedProjectionScenario<WmsContext> scenario)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var actualRecords = await context.AllRecords();
            return actualRecords.Length == 0
                ? VerificationResult.Pass()
                : VerificationResult.Fail($"Expected 0 records but found {actualRecords.Length}.");
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<WmsContext>(specification.Resolver);
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object>()).ToGenericEnvelope();
                await projector.ProjectAsync(context, envelope);
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContextFor(database))
        {
            var result = await specification.Verification(context, CancellationToken.None);

            if (result.Failed)
            {
                throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }
    }

    public static ConnectedProjectionScenario<WmsContext> Scenario(this ConnectedProjection<WmsContext> projection)
    {
        return new ConnectedProjectionScenario<WmsContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));
    }
}