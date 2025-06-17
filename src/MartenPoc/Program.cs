namespace MartenPoc;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using Domain;
using JasperFx;
using JasperFx.Blocks;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetTopologySuite.Geometries;
using Npgsql;
using Projections;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services
                    .AddMarten(options =>
                    {
                        options.Connection(new NpgsqlDataSourceBuilder("Host=localhost;port=5440;Username=postgres;Password=postgres")
                            //.UseNetTopologySuite()
                            .Build());
                        options.DatabaseSchemaName = "road";

                        ScopedWegennetwerkRepository.Configure(options);

                        FlattenedRoadNetworkChangeProjection.Configure(options);
                    })
                    .AddAsyncDaemon(DaemonMode.Solo);
            })
            .Build();

        await using var store = host.Services.GetService<IDocumentStore>();

        //await RebuildRoadSegmentSnapshots(store);

        //await SeedNetworkWithNewEntities(store);
        //await SeedNetworkWithChangesToExistingEntities(store);
        //await SeedNetworkWithChangesToExistingEntities(store);

        //await LoadRoadNetworkForRegion(store);

        await host.RunAsync();
    }

    private static async Task RebuildRoadSegmentSnapshots(IDocumentStore store)
    {
        var projectionDaemon = await store.BuildProjectionDaemonAsync();
        await projectionDaemon.RebuildProjectionAsync<Wegsegment>(CancellationToken.None);
    }

    private static async Task LoadRoadNetworkForRegion(IDocumentStore store)
    {
        var repository = new ScopedWegennetwerkRepository(store);

        var minXY = 200100;
        var width = 5000;
        // warmup needed -> because of schema check?
        var _ = await repository.Load(new Polygon(new LinearRing([
            new Coordinate(minXY, minXY),
            new Coordinate(minXY, minXY+width),
            new Coordinate(minXY+width, minXY+width),
            new Coordinate(minXY+width, minXY),
            new Coordinate(minXY, minXY)])), limitSegments: 1);

        var sw = Stopwatch.StartNew();
        var network = await repository.Load(new Polygon(new LinearRing([
            new Coordinate(minXY, minXY),
            new Coordinate(minXY, minXY+width),
            new Coordinate(minXY+width, minXY+width),
            new Coordinate(minXY+width, minXY),
            new Coordinate(minXY, minXY)])), limitSegments: 10000);
        var elapsed = sw.Elapsed;

        var segments = network.Wegsegmenten.OrderByDescending(x => x.Version).ToList();
    }

    private static async Task SeedNetworkWithNewEntities(IDocumentStore store)
    {
        //return;

        var fixture = new Fixture();

        var netwerk = ScopedWegennetwerk.Empty;
        var repository = new ScopedWegennetwerkRepository(store);

        var minXY = 200000;
        var maxXY = 201000;

        for (var change = 0; change < 50; change++)
        {
            for (var i = 0; i < 100; i++)
            {
                var x1 = new Random().Next(minXY, maxXY);
                var y1 = new Random().Next(minXY, maxXY);
                var x2 = new Random().Next(x1 - 50, x1 + 50);
                var y2 = new Random().Next(y1 - 50, y1 + 50);

                var knoopId1 = Guid.NewGuid();
                var knoopId2 = Guid.NewGuid();

                netwerk.VoegWegknoopToe(knoopId1, new Point(x1, y1).AsText(), fixture.Create<string>());
                netwerk.VoegWegknoopToe(knoopId2, new Point(x2, y2).AsText(), fixture.Create<string>());
                netwerk.VoegWegsegmentToe(Guid.NewGuid(), new LineString([new Coordinate(x1, y1), new Coordinate(x2, y2)]).AsText(),
                    knoopId1, knoopId2,
                    fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(),
                    fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());
                //break; //TODO-pr temp do only 1
            }

            await repository.SaveRoadNetworkChange(netwerk);
            //break; //TODO-pr temp do only 1
        }
    }

    private static async Task SeedNetworkWithChangesToExistingEntities(IDocumentStore store)
    {
        var fixture = new Fixture();

        await using var session = store.LightweightSession();

        var repository = new ScopedWegennetwerkRepository(store);
        var minXY = 200000;
        var width = 1000;
        var netwerk = await repository.Load(new Polygon(new LinearRing([
            new Coordinate(minXY, minXY),
            new Coordinate(minXY, minXY+width),
            new Coordinate(minXY+width, minXY+width),
            new Coordinate(minXY+width, minXY),
            new Coordinate(minXY, minXY)])));

        foreach (var segment in netwerk.Wegsegmenten)
        {
            var changes = new Random().Next(0, 10);
            for (var c = 0; c < changes; c++)
            {
                netwerk.WijzigWegsegment(segment.Id, segment.Geometry, segment.BeginknoopId, segment.EindknoopId,
                    fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(),
                    fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());
                //break; //TODO-pr temp do only 1
            }

            await repository.SaveRoadNetworkChange(netwerk);
        }

        foreach (var knoop in netwerk.Wegknopen)
        {
            var changes = new Random().Next(0, 10);
            for (var c = 0; c < changes; c++)
            {
                netwerk.WijzigWegknoop(knoop.Id, knoop.Geometry, fixture.Create<string>());
            }

            await repository.SaveRoadNetworkChange(netwerk);
        }
    }
}
