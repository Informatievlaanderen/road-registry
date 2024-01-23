namespace RoadRegistry.StreetNameConsumer.Projections;

using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Schema;
using StreetName;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class StreetNameConsumerProjection : ConnectedProjection<StreetNameConsumerContext>
{
    public StreetNameConsumerProjection()
    {
        When<StreetNameSnapshotOsloWasProduced>(StreetNameSnapshotOsloWasProduced);
    }

    private async Task StreetNameSnapshotOsloWasProduced(StreetNameConsumerContext context, StreetNameSnapshotOsloWasProduced message, CancellationToken token)
    {
        var streetNameConsumerItem = await context.StreetNames.FindAsync(new object[] { message.StreetNameId }, token).ConfigureAwait(false);

        if (streetNameConsumerItem is null)
        {
            streetNameConsumerItem = new StreetNameConsumerItem
            {
                StreetNameId = message.Record.Identificator.Id
            };
            await context.StreetNames.AddAsync(streetNameConsumerItem, token);
        }
        
        if (message.Record.Identificator is null)
        {
            streetNameConsumerItem.IsRemoved = true;
        }
        else
        {
            streetNameConsumerItem.PersistentLocalId = int.Parse(message.Record.Identificator.ObjectId);
            streetNameConsumerItem.NisCode = message.Record.Gemeente.ObjectId;

            streetNameConsumerItem.DutchName = GetSpelling(message.Record.Straatnamen, Taal.NL);
            streetNameConsumerItem.FrenchName = GetSpelling(message.Record.Straatnamen, Taal.FR);
            streetNameConsumerItem.GermanName = GetSpelling(message.Record.Straatnamen, Taal.DE);
            streetNameConsumerItem.EnglishName = GetSpelling(message.Record.Straatnamen, Taal.EN);

            streetNameConsumerItem.DutchHomonymAddition = GetSpelling(message.Record.HomoniemToevoegingen, Taal.NL);
            streetNameConsumerItem.FrenchHomonymAddition = GetSpelling(message.Record.HomoniemToevoegingen, Taal.FR);
            streetNameConsumerItem.GermanHomonymAddition = GetSpelling(message.Record.HomoniemToevoegingen, Taal.DE);
            streetNameConsumerItem.EnglishHomonymAddition = GetSpelling(message.Record.HomoniemToevoegingen, Taal.EN);

            streetNameConsumerItem.StreetNameStatus = message.Record.StraatnaamStatus;
        }
    }

    private static string? GetSpelling(List<DeseriazableGeografischeNaam>? namen, Taal taal)
    {
        return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
    }
}
