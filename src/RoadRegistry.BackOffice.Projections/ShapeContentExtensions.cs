namespace RoadRegistry.BackOffice.Projections
{
    using System.IO;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.IO;

    public static class ShapeContentExtensions
    {
        public static byte[] ToBytes(this ShapeContent content, RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            using (var output = manager.GetStream())
            using (var writer = new BinaryWriter(output, encoding))
            {
                content.Write(writer);
                writer.Flush();
                return output.ToArray();
            }
        }
    }
}
