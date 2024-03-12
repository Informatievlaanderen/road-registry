namespace RoadRegistry.BackOffice.Extensions
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class StreamExtensions
    {
        public static async Task<MemoryStream> CopyToNewMemoryStream(this Stream source, CancellationToken cancellationToken)
        {
            var readStream = new MemoryStream();
            await source.CopyToAsync(readStream, cancellationToken);
            readStream.Position = 0;
            return readStream;
        }
    }
}
