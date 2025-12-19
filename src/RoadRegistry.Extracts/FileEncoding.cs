namespace RoadRegistry.Extracts;

using System.Text;
using RoadRegistry.Extensions;

public sealed class FileEncoding
{
    public Encoding Encoding { get; }

    public static FileEncoding UTF8 => new(Encoding.UTF8);
    public static FileEncoding WindowsAnsi => new(WellKnownEncodings.WindowsAnsi);

    public FileEncoding(Encoding encoding)
    {
        Encoding = encoding.ThrowIfNull();
    }

    public static implicit operator Encoding(FileEncoding dbaseEncoding)
    {
        return dbaseEncoding.Encoding;
    }
}
