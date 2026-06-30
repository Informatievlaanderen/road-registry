namespace RoadRegistry.Projections.Tests.Projections.MartenMigration;

using System.Collections.Generic;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Infrastructure.MartenDb.Setup;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.MartenMigration.Projections;
using RoadRegistry.StreetName.Events.V2;

public class MartenMigrationStreetNameProjectionTests
{
    private const string When = "2025-01-01T00:00:00Z";

    [Fact]
    public Task WhenStreetNameCreated_ThenStreetNameWasCreated()
    {
        var message = new StreetNameCreated
        {
            Record = new StreetNameRecord { PersistentLocalId = 123, DutchName = "Foo" },
            When = When
        };

        return BuildProjection()
            .Scenario()
            .Given(message)
            .Expect(new StreetNameWasCreated
            {
                StreetNameId = new StreetNameLocalId(123),
                DutchName = "Foo",
                Provenance = ExpectedProvenance(Modification.Insert)
            });
    }

    [Fact]
    public Task WhenStreetNameModifiedWithNameChange_ThenStreetNameWasModified()
    {
        var message = new StreetNameModified
        {
            Record = new StreetNameRecord { PersistentLocalId = 123, DutchName = "Foo", NisCode = "44021", StreetNameStatus = "Current" },
            NameModified = true,
            When = When
        };

        return BuildProjection()
            .Scenario()
            .Given(message)
            .Expect(new StreetNameWasModified
            {
                StreetNameId = new StreetNameLocalId(123),
                DutchName = "Foo",
                NisCode = "44021",
                Status = "Current",
                Provenance = ExpectedProvenance(Modification.Update)
            });
    }

    [Fact]
    public Task WhenStreetNameModifiedWithoutNameChange_ThenNoEvent()
    {
        var message = new StreetNameModified
        {
            Record = new StreetNameRecord { PersistentLocalId = 123, DutchName = "Foo" },
            NameModified = false,
            When = When
        };

        return BuildProjection()
            .Scenario()
            .Given(message)
            .ExpectNone();
    }

    [Fact]
    public Task WhenStreetNameRemoved_ThenStreetNameWasRemoved()
    {
        var message = new StreetNameRemoved
        {
            StreetNameId = new StreetNameId(123).ToString(),
            When = When
        };

        return BuildProjection()
            .Scenario()
            .Given(message)
            .Expect(new StreetNameWasRemoved
            {
                StreetNameId = new StreetNameLocalId(123),
                Provenance = ExpectedProvenance(Modification.Delete)
            });
    }

    [Fact]
    public Task WhenSameEventIsProjectedTwice_ThenOnlyOnceProcessed()
    {
        var message = new StreetNameCreated
        {
            Record = new StreetNameRecord { PersistentLocalId = 123, DutchName = "Foo" },
            When = When
        };

        var envelope = new Envelope(message, new Dictionary<string, object>
        {
            { Envelope.PositionMetadataKey, 0L },
            { Envelope.StreamIdMetadataKey, RoadNetworkStreamNameProvider.Default.ToString() },
            { Envelope.CreatedUtcMetadataKey, Moment.EnvelopeCreatedUtc.ToUniversalTime() },
            { Envelope.EventNameMetadataKey, message.GetType().Name },
        });

        return BuildProjection()
            .Scenario()
            .Given(envelope, envelope)
            .Expect(new StreetNameWasCreated
            {
                StreetNameId = new StreetNameLocalId(123),
                DutchName = "Foo",
                Provenance = ExpectedProvenance(Modification.Insert)
            });
    }

    private static ProvenanceData ExpectedProvenance(Modification modification)
    {
        return new ProvenanceData(new Provenance(
            LocalDateTimeTranslator.TranslateFromWhen(When),
            Application.RoadRegistry,
            new Reason("StreetName"),
            new Operator("StreetNameRegistry"),
            modification,
            Organisation.DigitaalVlaanderen));
    }

    private (MartenMigrationProjection, InMemoryDocumentStoreSession) BuildProjection()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        return (new MartenMigrationProjection(store), store);
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        return storeOptions;
    }
}
