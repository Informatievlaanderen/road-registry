namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System.Text;
using BackOffice.Extracts.Dbase;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class DbaseFileTests
{
    [Fact]
    public async Task When_dbase_file_provided()
    {
        ExtractDescription? extractDescription = null;

        await using var stream = await EmbeddedResourceReader.ReadAsync("Transactiezones.dbf");

        using var reader = new BinaryReader(stream, Encoding.UTF8);
        var header = DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));

        using var records = header.CreateDbaseRecordEnumerator<TransactionZoneDbaseRecord>(reader);
        while (records.MoveNext())
        {
            extractDescription = new ExtractDescription(records.Current!.BESCHRIJV.Value);
        }

        Assert.NotNull(extractDescription);
    }
}
