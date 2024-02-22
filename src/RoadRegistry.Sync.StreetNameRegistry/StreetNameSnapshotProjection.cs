namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using System.Threading;
using System.Threading.Tasks;
using StreetNameRecord = Models.StreetNameRecord;

public class StreetNameSnapshotProjection : ConnectedProjection<StreetNameSnapshotProjectionContext>
{
    public StreetNameSnapshotProjection()
    {
        When<Envelope<StreetNameCreated>>(StreetNameCreated);
        When<Envelope<StreetNameModified>>(StreetNameModified);
        When<Envelope<StreetNameRemoved>>(StreetNameRemoved);
    }

    private async Task StreetNameCreated(StreetNameSnapshotProjectionContext context, Envelope<StreetNameCreated> envelope, CancellationToken token)
    {
        var dbRecord = new StreetNameRecord();
        CopyTo(envelope.Message.Record, dbRecord);

        await context.StreetNames.AddAsync(dbRecord, token);
    }

    private async Task StreetNameModified(StreetNameSnapshotProjectionContext context, Envelope<StreetNameModified> envelope, CancellationToken token)
    {
        var dbRecord = await context.StreetNames.FindAsync(new object[] { envelope.Message.Record.StreetNameId }, token).ConfigureAwait(false);

        if (dbRecord is null)
        {
            dbRecord = new StreetNameRecord
            {
                StreetNameId = envelope.Message.Record.StreetNameId
            };
            await context.StreetNames.AddAsync(dbRecord, token);
        }
        else
        {
            dbRecord.IsRemoved = false;
        }

        CopyTo(envelope.Message.Record, dbRecord);
    }

    private async Task StreetNameRemoved(StreetNameSnapshotProjectionContext context, Envelope<StreetNameRemoved> envelope, CancellationToken token)
    {
        var dbRecord = await context.StreetNames.FindAsync(new object[] { envelope.Message.StreetNameId }, token).ConfigureAwait(false);

        if (dbRecord is not null)
        {
            dbRecord.IsRemoved = true;
        }
    }
    
    private void CopyTo(BackOffice.Messages.StreetNameRecord eventRecord, StreetNameRecord dbRecord)
    {
        dbRecord.StreetNameId = eventRecord.StreetNameId;
        dbRecord.PersistentLocalId = eventRecord.PersistentLocalId;
        dbRecord.NisCode = eventRecord.NisCode;
        dbRecord.DutchName = eventRecord.DutchName;
        dbRecord.FrenchName = eventRecord.FrenchName;
        dbRecord.GermanName = eventRecord.GermanName;
        dbRecord.EnglishName = eventRecord.EnglishName;
        dbRecord.DutchHomonymAddition = eventRecord.DutchHomonymAddition;
        dbRecord.FrenchHomonymAddition = eventRecord.FrenchHomonymAddition;
        dbRecord.GermanHomonymAddition = eventRecord.GermanHomonymAddition;
        dbRecord.EnglishHomonymAddition = eventRecord.EnglishHomonymAddition;
        dbRecord.DutchNameWithHomonymAddition = eventRecord.DutchNameWithHomonymAddition;
        dbRecord.FrenchNameWithHomonymAddition = eventRecord.FrenchNameWithHomonymAddition;
        dbRecord.GermanNameWithHomonymAddition = eventRecord.GermanNameWithHomonymAddition;
        dbRecord.EnglishNameWithHomonymAddition = eventRecord.EnglishNameWithHomonymAddition;
        dbRecord.StreetNameStatus = eventRecord.StreetNameStatus;
    }
}
