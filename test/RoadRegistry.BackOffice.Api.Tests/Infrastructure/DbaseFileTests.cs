namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System.Text;
using BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class DbaseFileTests
{
    [Fact]
    public async Task When_dbase_file_provided()
    {
        ExtractDescription? extractDescription = null;

        using (var sourceStream = new MemoryStream())
        {
            await using var embeddedStream = typeof(DbaseFileTests).Assembly.GetManifestResourceStream(typeof(DbaseFileTests), "Transactiezones.dbf");
            await embeddedStream!.CopyToAsync(sourceStream);
            sourceStream.Position = 0;

            using (var reader = new BinaryReader(sourceStream, Encoding.UTF8))
            {
                var header = DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));

                using var records = header.CreateDbaseRecordEnumerator<TransactionZoneDbaseRecord>(reader);
                while (records.MoveNext())
                {
                    extractDescription = new ExtractDescription(records.Current!.BESCHRIJV.Value);
                }
            }
        }

        Assert.NotNull(extractDescription);
    }
}
