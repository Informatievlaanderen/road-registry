using Be.Vlaanderen.Basisregisters.Shaperon;
using Castle.Components.DictionaryAdapter.Xml;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Core;
using System.IO;
using System.Text;

namespace RoadRegistry.BackOffice.Api.Tests
{
    using BackOffice.Uploads.Schema.V1;
    using Polly;
    using RoadRegistry.BackOffice.Messages;

    public class DbaseFileTests
    {

        [Fact]
        public async Task When_dbase_file_provided()
        {
            ExtractDescription extractDescription;

            using (var sourceStream = new MemoryStream())
            {
                await using (var embeddedStream =
                             typeof(DbaseFileTests).Assembly.GetManifestResourceStream(typeof(DbaseFileTests),
                                 "Transactiezones.dbf"))
                {
                    embeddedStream.CopyTo(sourceStream);
                }

                sourceStream.Position = 0;
                using (var reader = new BinaryReader(sourceStream, Encoding.UTF8))
                {
                    var header = Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));
                    if (header is not null)
                    {
                        using var records = header.CreateDbaseRecordEnumerator<TransactionZoneDbaseRecord>(reader);
                        while (records.MoveNext())
                        {
                            extractDescription = new ExtractDescription(records.Current.BESCHRIJV.Value);
                        }
                    }
                }
            }

            Assert.NotNull(extractDescription);
        }
    }
}
