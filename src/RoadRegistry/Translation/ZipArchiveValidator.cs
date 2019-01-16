namespace RoadRegistry.Translation
{
    using System.IO.Compression;
    using System.Threading.Tasks;

    public class ZipArchiveValidator
    {
        public Task<ZipArchiveErrors> ValidateAsync(ZipArchive archive)
        {

            return Task.FromResult(ZipArchiveErrors.None);
        }
    }
}