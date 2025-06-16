namespace MartenPoc;

using System;
using System.Threading.Tasks;
using AutoFixture;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetTopologySuite.Geometries;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        /*TODO-pr flow:
         x events genereren voor wegsegmenten en wegknopen
         - inline projectie voor wegsegmenten en wegknopen
         - perf test: scoped roadnetwerk inladen voor een regio (inline projectie gebruiken voor wegknoop/wegsegment ids te vinden)
         - projectie maken adv causation_id
         */

        var sp = new ServiceCollection().AddMarten(options =>
        {
            ScopedWegennetwerkRepository.Configure(options);

            options.CreateDatabasesForTenants(c =>
            {
                // Specify a db to which to connect in case database needs to be created.
                // If not specified, defaults to 'postgres' on the connection for a tenant.
                c.ForTenant()
                    .CheckAgainstPgDatabase()
                    .WithOwner("postgres")
                    .WithEncoding("UTF-8")
                    .ConnectionLimit(-1);
            });
        }).Services.BuildServiceProvider();

        var store = sp.GetService<IDocumentStore>();

        // TODO-pr: add geometry column: ALTER TABLE mt_doc_roadnetworksegment ADD COLUMN geometry geometry(Geometry, 0);



        await SeedNetworkWithNewEntities(store);
        //TODO-pr await SeedNetworkWithChangesToExistingEntities();

        var repository = new ScopedWegennetwerkRepository(store);

        // var netwerk = await repository.Load(
        //     wegknoopIds:
        //     [
        //         eersteWegsegment.BeginknoopId,
        //         eersteWegsegment.EindknoopId,
        //         tweedeWegsegment.BeginknoopId,
        //         tweedeWegsegment.EindknoopId
        //     ],
        //     wegsegmentIds: [eersteWegsegmentId, tweedeWegsegmentId]);
        //
        // netwerk.WijzigWegknoopType(tweedeWegsegment.EindknoopId, "Schijnknoop");
        //
        // await repository.Save(netwerk);
    }

    private static async Task SeedNetworkWithNewEntities(IDocumentStore store)
    {
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
            }

            await repository.SaveRoadNetworkChange(netwerk);
        }
    }
}
