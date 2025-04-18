namespace RoadRegistry.BackOffice.Extensions
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class StreamExtensions
    {
        public static async Task<MemoryStream> CopyToNewMemoryStreamAsync(this Stream source, CancellationToken cancellationToken)
        {
            var readStream = new MemoryStream();
            await source.CopyToAsync(readStream, cancellationToken);
            readStream.Position = 0;
            return readStream;
        }

        public static MemoryStream CopyToNewMemoryStream(this Stream source)
        {
            var readStream = new MemoryStream();
            source.CopyTo(readStream);
            readStream.Position = 0;
            return readStream;
        }
    }
}
