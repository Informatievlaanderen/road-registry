namespace RoadRegistry.BackOffice.Projections
{
    using System.IO;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.IO;

    internal static class DbaseRecordExtensions
    {
        public static byte[] ToBytes(this DbaseRecord record, RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            using (var output = manager.GetStream())
            using (var writer = new BinaryWriter(output, encoding))
            {
                record.Write(writer);
                writer.Flush();
                return output.ToArray();
            }
        }
    }
}
