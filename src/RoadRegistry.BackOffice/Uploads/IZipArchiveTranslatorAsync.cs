namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

public interface IZipArchiveTranslatorAsync
{
    Task<TranslatedChanges> Translate(ZipArchive archive, CancellationToken cancellation);
}
