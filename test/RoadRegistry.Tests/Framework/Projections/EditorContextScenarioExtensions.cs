namespace RoadRegistry.Tests.Framework.Projections;

using System.Text;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Editor.Schema;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Core;
using Xunit.Sdk;

public class MemoryEditorContext : EditorContext
{
    public MemoryEditorContext(DbContextOptions<EditorContext> options) : base(options)
    {
    }

    protected override void OnModelQueryTypes(ModelBuilder builder)
    {
        builder
            .Entity<RoadNodeBoundingBox2D>()
            .HasNoKey()
            .ToInMemoryQuery(() =>
                from node in RoadNodes
                group node by node.Id
                into nodes
                select new RoadNodeBoundingBox2D
                {
                    MinimumX = nodes.Min(n => n.BoundingBoxMinimumX ?? 0),
                    MaximumX = nodes.Max(n => n.BoundingBoxMaximumX ?? 0),
                    MinimumY = nodes.Min(n => n.BoundingBoxMinimumY ?? 0),
                    MaximumY = nodes.Max(n => n.BoundingBoxMaximumY ?? 0)
                }
            );
        builder
            .Entity<RoadSegmentBoundingBox3D>()
            .HasNoKey()
            .ToInMemoryQuery(() =>
                from segment in RoadSegments
                group segment by segment.Id
                into segments
                select new RoadSegmentBoundingBox3D
                {
                    MinimumX = segments.Min(n => n.BoundingBoxMinimumX ?? 0),
                    MaximumX = segments.Max(n => n.BoundingBoxMaximumX ?? 0),
                    MinimumY = segments.Min(n => n.BoundingBoxMinimumY ?? 0),
                    MaximumY = segments.Max(n => n.BoundingBoxMaximumY ?? 0),
                    MinimumM = segments.Min(n => n.BoundingBoxMinimumM ?? 0),
                    MaximumM = segments.Max(n => n.BoundingBoxMaximumM ?? 0)
                }
            );
    }
}

public static class EditorContextScenarioExtensions
{
    //IMPORTANT: Each time you change the db sets on the context, you must adjust this method as well.
    //
    private static async Task<object[]> AllRecords(this EditorContext context, bool ignoreQueryFilters = false)
    {
        var records = new List<object>();
        records.AddRange(await context.RoadNodes.ToArrayAsync());
        records.AddRange(await context.RoadSegments.IgnoreQueryFilters(ignoreQueryFilters).ToArrayAsync());
        records.AddRange(await context.RoadSegmentVersions.ToArrayAsync());
        records.AddRange(await context.RoadSegmentLaneAttributes.ToArrayAsync());
        records.AddRange(await context.RoadSegmentWidthAttributes.ToArrayAsync());
        records.AddRange(await context.RoadSegmentSurfaceAttributes.ToArrayAsync());
        records.AddRange(await context.RoadSegmentEuropeanRoadAttributes.ToArrayAsync());
        records.AddRange(await context.RoadSegmentNationalRoadAttributes.ToArrayAsync());
        records.AddRange(await context.RoadSegmentNumberedRoadAttributes.ToArrayAsync());
        records.AddRange(await context.GradeSeparatedJunctions.ToArrayAsync());
        records.AddRange(await context.Organizations.ToArrayAsync());
        records.AddRange(await context.OrganizationsV2.ToArrayAsync());
        records.AddRange(await context.RoadNetworkInfo.ToArrayAsync());
        records.AddRange(await context.RoadNetworkChanges.ToArrayAsync());
        records.AddRange(await context.RoadNetworkChangeRequestsBasedOnArchive.ToArrayAsync());
        records.AddRange(await context.ExtractDownloads.ToArrayAsync());
        records.AddRange(await context.ExtractRequests.ToArrayAsync());
        records.AddRange(await context.ExtractUploads.ToArrayAsync());

        return records.ToArray();
    }

    private static EditorContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new MemoryEditorContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<EditorContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<EditorContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.Expect(records.ToArray());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<EditorContext> scenario,
        params object[] records)
    {
        return Expect(scenario, false, records);
    }

    public static Task ExpectWhileIgnoringQueryFilters(
        this ConnectedProjectionScenario<EditorContext> scenario,
        params object[] records)
    {
        return Expect(scenario, true, records);
    }

    private static async Task Expect(
        this ConnectedProjectionScenario<EditorContext> scenario,
        bool ignoreQueryFilters,
        object[] records)
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
            var actualRecords = await context.AllRecords(ignoreQueryFilters: ignoreQueryFilters);
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
            var projector = new ConnectedProjector<EditorContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object>
                {
                    { "Position", position },
                    { "StreamId", RoadNetworkStreamNameProvider.Default.ToString() }
                }).ToGenericEnvelope();
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

    // public static Task ExpectInAnyOrder(
    //     this ConnectedProjectionScenario<EditorContext> scenario,
    //     IEnumerable<object> records)
    // {
    //     return scenario.ExpectInAnyOrder(records.ToArray());
    // }
    //
    // public static async Task ExpectInAnyOrder(
    //     this ConnectedProjectionScenario<EditorContext> scenario,
    //     params object[] records)
    // {
    //     var database = Guid.NewGuid().ToString("N");
    //
    //     var specification = scenario.Verify(async context =>
    //     {
    //         var comparisonConfig = new ComparisonConfig
    //         {
    //             IgnoreCollectionOrder = true,
    //             MaxDifferences = 10,
    //             CustomComparers = new List<BaseTypeComparer>
    //             {
    //                 new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
    //             }
    //         };
    //
    //         var comparer = new CompareLogic(comparisonConfig);
    //         var actualRecords = await context.AllRecords();
    //         var result = comparer.Compare(
    //             actualRecords,
    //             records
    //         );
    //
    //         return result.AreEqual
    //             ? VerificationResult.Pass()
    //             : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
    //     });
    //
    //     using (var context = CreateContextFor(database))
    //     {
    //         var projector = new ConnectedProjector<EditorContext>(specification.Resolver);
    //         var position = 0L;
    //         foreach (var message in specification.Messages)
    //         {
    //             var envelope = new Envelope(message, new Dictionary<string, object> { { "Position", position }}).ToGenericEnvelope();
    //             await projector.ProjectAsync(context, envelope);
    //             position++;
    //         }
    //
    //         await context.SaveChangesAsync();
    //     }
    //
    //     using (var context = CreateContextFor(database))
    //     {
    //         var result = await specification.Verification(context, CancellationToken.None);
    //
    //         if (result.Failed)
    //             throw specification.CreateFailedScenarioExceptionFor(result);
    //     }
    // }

    public static Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<EditorContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.ExpectInAnyOrder(records.ToArray());
    }

    public static async Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<EditorContext> scenario,
        params object[] records)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var comparisonConfig = new ComparisonConfig { MaxDifferences = 5, IgnoreCollectionOrder = true };
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
            var projector = new ConnectedProjector<EditorContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object>
                {
                    { "Position", position },
                    { "StreamId", RoadNetworkStreamNameProvider.Default.ToString() }
                }).ToGenericEnvelope();
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

    public static async Task ExpectNone(this ConnectedProjectionScenario<EditorContext> scenario)
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
            var projector = new ConnectedProjector<EditorContext>(specification.Resolver);
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

    public static ConnectedProjectionScenario<EditorContext> Scenario(this ConnectedProjection<EditorContext> projection)
    {
        return new ConnectedProjectionScenario<EditorContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));
    }
}
