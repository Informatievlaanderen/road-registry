namespace RoadRegistry.Tests.Framework.Projections;

using System.Text;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.EntityFrameworkCore;
using Producer.Snapshot.ProjectionHost.RoadNode;
using Producer.Snapshot.ProjectionHost.RoadSegment;
using Xunit.Sdk;

public class MemoryRoadNodeProducerSnapshotContext : RoadNodeProducerSnapshotContext
{
    public MemoryRoadNodeProducerSnapshotContext(DbContextOptions<RoadNodeProducerSnapshotContext> options) : base(options)
    {
    }
}

public class MemoryRoadSegmentProducerSnapshotContext : RoadSegmentProducerSnapshotContext
{
    public MemoryRoadSegmentProducerSnapshotContext(DbContextOptions<RoadSegmentProducerSnapshotContext> options) : base(options)
    {
    }
}

public static class RoadNodeProducerSnapshotContextScenarioExtensions
{
    //IMPORTANT: Each time you change the db sets on the context, you must adjust this method as well.
    //
    private static async Task<object[]> AllRecords(this RoadNodeProducerSnapshotContext context)
    {
        var records = new List<object>();
        records.AddRange(await context.RoadNodes.ToArrayAsync());

        return records.ToArray();
    }

    private static RoadNodeProducerSnapshotContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<RoadNodeProducerSnapshotContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new MemoryRoadNodeProducerSnapshotContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<RoadNodeProducerSnapshotContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<RoadNodeProducerSnapshotContext> scenario,
        DateTime created,
        IEnumerable<object> records)
    {
        return scenario.Expect(created, records.ToArray());
    }

    public static async Task Expect(
        this ConnectedProjectionScenario<RoadNodeProducerSnapshotContext> scenario,
        DateTime created,
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
                    new GeometryMultiPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPointComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryMultiLineStringComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
                }
            };
            var comparer = new CompareLogic(comparisonConfig);
            var actualRecords = await context.AllRecords();
            var result = comparer.Compare(
                records,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<RoadNodeProducerSnapshotContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { Envelope.PositionMetadataKey, position }, { Envelope.CreatedUtcMetadataKey, created.ToUniversalTime() } }).ToGenericEnvelope();
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

    public static Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<RoadNodeProducerSnapshotContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.ExpectInAnyOrder(records.ToArray());
    }

    public static async Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<RoadNodeProducerSnapshotContext> scenario,
        params object[] records)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var comparisonConfig = new ComparisonConfig { MaxDifferences = 5, IgnoreCollectionOrder = true };
            var comparer = new CompareLogic(comparisonConfig);
            var actualRecords = await context.AllRecords();
            var result = comparer.Compare(
                records,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<RoadNodeProducerSnapshotContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { Envelope.PositionMetadataKey, position } }).ToGenericEnvelope();
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

    public static async Task ExpectNone(this ConnectedProjectionScenario<RoadNodeProducerSnapshotContext> scenario)
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
            var projector = new ConnectedProjector<RoadNodeProducerSnapshotContext>(specification.Resolver);
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { Envelope.CreatedUtcMetadataKey, DateTime.Now } }).ToGenericEnvelope();
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

    public static ConnectedProjectionScenario<RoadNodeProducerSnapshotContext> Scenario(this ConnectedProjection<RoadNodeProducerSnapshotContext> projection)
    {
        return new ConnectedProjectionScenario<RoadNodeProducerSnapshotContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));
    }
}

public static class RoadSegmentProducerSnapshotContextScenarioExtensions
{
    //IMPORTANT: Each time you change the db sets on the context, you must adjust this method as well.
    //
    private static async Task<object[]> AllRecords(this RoadSegmentProducerSnapshotContext context)
    {
        var records = new List<object>();
        records.AddRange(await context.RoadSegments.ToArrayAsync());

        return records.ToArray();
    }

    private static RoadSegmentProducerSnapshotContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<RoadSegmentProducerSnapshotContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new MemoryRoadSegmentProducerSnapshotContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<RoadSegmentProducerSnapshotContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext> scenario,
        DateTime created,
        IEnumerable<object> records)
    {
        return scenario.Expect(created, records.ToArray());
    }

    public static async Task Expect(
        this ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext> scenario,
        DateTime created,
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
                    new GeometryMultiPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPointComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryMultiLineStringComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
                },
                MembersToIgnore = new List<string>
                {
                    "Version"
                }
            };
            var comparer = new CompareLogic(comparisonConfig);
            var actualRecords = await context.AllRecords();
            var result = comparer.Compare(
                records,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<RoadSegmentProducerSnapshotContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { Envelope.PositionMetadataKey, position }, { Envelope.CreatedUtcMetadataKey, created.ToUniversalTime() } }).ToGenericEnvelope();
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

    public static Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.ExpectInAnyOrder(records.ToArray());
    }

    public static async Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext> scenario,
        params object[] records)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var comparisonConfig = new ComparisonConfig { MaxDifferences = 5, IgnoreCollectionOrder = true };
            var comparer = new CompareLogic(comparisonConfig);
            var actualRecords = await context.AllRecords();
            var result = comparer.Compare(
                records,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<RoadSegmentProducerSnapshotContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { Envelope.PositionMetadataKey, position } }).ToGenericEnvelope();
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

    public static async Task ExpectNone(this ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext> scenario)
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
            var projector = new ConnectedProjector<RoadSegmentProducerSnapshotContext>(specification.Resolver);
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { Envelope.CreatedUtcMetadataKey, DateTime.Now } }).ToGenericEnvelope();
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

    public static ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext> Scenario(this ConnectedProjection<RoadSegmentProducerSnapshotContext> projection)
    {
        return new ConnectedProjectionScenario<RoadSegmentProducerSnapshotContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));
    }
}
