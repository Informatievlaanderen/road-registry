namespace RoadRegistry.Sync.StreetNameRegistry;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using RoadRegistry.BackOffice.Messages;
using System.Threading;
using System.Threading.Tasks;

public class StreetNameConsumerProjection : ConnectedProjection<StreetNameConsumerContext>
{
    public StreetNameConsumerProjection()
    {
        When<StreetNameCreated>(StreetNameCreated);
        When<StreetNameModified>(StreetNameModified);
        When<StreetNameRemoved>(StreetNameRemoved);
    }

    private async Task StreetNameCreated(StreetNameConsumerContext context, StreetNameCreated message, CancellationToken token)
    {
        var dbRecord = new StreetNameRecord();
        CopyTo(message.Record, dbRecord);

        await context.StreetNames.AddAsync(dbRecord, token);
    }

    private async Task StreetNameModified(StreetNameConsumerContext context, StreetNameModified message, CancellationToken token)
    {
        var dbRecord = await context.StreetNames.FindAsync(new object[] { message.Record.StreetNameId }, token).ConfigureAwait(false);

        if (dbRecord is null)
        {
            dbRecord = new StreetNameRecord
            {
                StreetNameId = message.Record.StreetNameId
            };
            await context.StreetNames.AddAsync(dbRecord, token);
        }

        CopyTo(message.Record, dbRecord);
    }

    private async Task StreetNameRemoved(StreetNameConsumerContext context, StreetNameRemoved message, CancellationToken token)
    {
        var dbRecord = await context.StreetNames.FindAsync(new object[] { message.StreetNameId }, token).ConfigureAwait(false);

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
