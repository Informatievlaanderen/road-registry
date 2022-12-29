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
        public void When_dbase_file_provided()
        {
            ExtractDescription extractDescription;

            using (var stream = File.OpenRead("Transactiezones.dbf"))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader header = null;

                    try
                    {
                        header = Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));

                        if (header is not null)
                        {
                            using var records = header.CreateDbaseRecordEnumerator<TransactionZoneDbaseRecord>(reader);
                            while (records.MoveNext())
                            {
                                extractDescription = new ExtractDescription(records.Current.BESCHRIJV.Value);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                    }
                }
            }

            Assert.NotNull(extractDescription);
        }
    }
}
