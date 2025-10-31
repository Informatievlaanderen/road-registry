namespace RoadRegistry.Integration.ProjectionHost.Tests;

using System.Text;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Xunit.Sdk;

public static class IntegrationContextScenarioExtensions
{
    public const long InitialPosition = 0L;

    //IMPORTANT: Each time you change the db sets on the context, you must adjust this method as well.
    //
    private static Task<object[]> AllRecords(this IntegrationContext context)
    {
        var records = new List<object>();
        records.AddRange(context.Organizations.Local);
        records.AddRange(context.RoadNodes.Local);
        records.AddRange(context.RoadSegments.Local);
        records.AddRange(context.RoadSegmentLaneAttributes.Local);
        records.AddRange(context.RoadSegmentSurfaceAttributes.Local);
        records.AddRange(context.RoadSegmentWidthAttributes.Local);
        records.AddRange(context.RoadSegmentEuropeanRoadAttributes.Local);
        records.AddRange(context.RoadSegmentNationalRoadAttributes.Local);
        records.AddRange(context.RoadSegmentNumberedRoadAttributes.Local);
        records.AddRange(context.GradeSeparatedJunctions.Local);

        records.AddRange(context.OrganizationVersions.Local);
        records.AddRange(context.RoadNodeVersions.Local);
        records.AddRange(context.RoadSegmentVersions.Local);
        records.AddRange(context.GradeSeparatedJunctionVersions.Local);

        return Task.FromResult(records.ToArray());
    }

    private static IntegrationContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<IntegrationContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new IntegrationContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<IntegrationContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<IntegrationContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.Expect(records.ToArray());
    }

    public static async Task Expect(
        this ConnectedProjectionScenario<IntegrationContext> scenario,
        object[] records)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var comparisonConfig = new ComparisonConfig
            {
                MaxDifferences = 10,
                CustomComparers =
                [
                    new GeometryMultiPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPointComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryMultiLineStringComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
                ]
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

        await using var context = CreateContextFor(database);

        var projector = new ConnectedProjector<IntegrationContext>(specification.Resolver);
        var position = InitialPosition;
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

        var result = await specification.Verification(context, CancellationToken.None);

        if (result.Failed)
        {
            throw specification.CreateFailedScenarioExceptionFor(result);
        }
    }

    public static Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<IntegrationContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.ExpectInAnyOrder(records.ToArray());
    }

    public static async Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<IntegrationContext> scenario,
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

        await using var context = CreateContextFor(database);

        var projector = new ConnectedProjector<IntegrationContext>(specification.Resolver);
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

        var result = await specification.Verification(context, CancellationToken.None);

        if (result.Failed)
        {
            throw specification.CreateFailedScenarioExceptionFor(result);
        }
    }

    public static ConnectedProjectionScenario<IntegrationContext> Scenario(this ConnectedProjection<IntegrationContext> projection)
    {
        return new ConnectedProjectionScenario<IntegrationContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));
    }
}
