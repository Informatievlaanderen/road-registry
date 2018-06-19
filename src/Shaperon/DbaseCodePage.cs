namespace Shaperon
{
    using System;
    using System.Text;

    public class DbaseCodePage : IEquatable<DbaseCodePage>
    {
        public static readonly DbaseCodePage DOSUSA = new DbaseCodePage(1, 437);
        public static readonly DbaseCodePage DOSMultilingual = new DbaseCodePage(2, 850);
        public static readonly DbaseCodePage WindowsANSI = new DbaseCodePage(3, 1252);
        public static readonly DbaseCodePage StandardMacintosh = new DbaseCodePage(4);
        public static readonly DbaseCodePage EEMSDOS = new DbaseCodePage(0x64, 852);
        public static readonly DbaseCodePage NordicMSDOS = new DbaseCodePage(0x65, 865);
        public static readonly DbaseCodePage RussianMSDOS = new DbaseCodePage(0x66, 866);
        public static readonly DbaseCodePage IcelandicMSDOS = new DbaseCodePage(0x67);
        public static readonly DbaseCodePage CzechMSDOS = new DbaseCodePage(0x68); // Kamenicky
        public static readonly DbaseCodePage PolishMSDOS = new DbaseCodePage(0x69); // Mazovia
        public static readonly DbaseCodePage GreekMSDOS = new DbaseCodePage(0x6A);
        public static readonly DbaseCodePage TurkishMSDOS = new DbaseCodePage(0x6B);
        public static readonly DbaseCodePage RussianMacintosh = new DbaseCodePage(0x96);
        public static readonly DbaseCodePage EasternEuropeanMacintosh = new DbaseCodePage(0x97);
        public static readonly DbaseCodePage GreekMacintosh = new DbaseCodePage(0x98);
        public static readonly DbaseCodePage WindowsEE = new DbaseCodePage(0xC8, 1250);
        public static readonly DbaseCodePage RussianWindows = new DbaseCodePage(0xC9);
        public static readonly DbaseCodePage TurkishWindows = new DbaseCodePage(0xCA);
        public static readonly DbaseCodePage GreekWindows = new DbaseCodePage(0xCB);

        public static readonly DbaseCodePage[] All =
        {
            DOSUSA, DOSMultilingual,
            EEMSDOS, NordicMSDOS, RussianMSDOS, IcelandicMSDOS, CzechMSDOS, PolishMSDOS, GreekMSDOS, TurkishMSDOS,
            StandardMacintosh, RussianMacintosh, EasternEuropeanMacintosh, GreekMacintosh,
            WindowsANSI, WindowsEE, RussianWindows, GreekWindows, TurkishWindows
        };

        private readonly byte _value;
        private readonly int? _codePage;

        private DbaseCodePage(byte value, int? codePage = default)
        {
            _value = value;
            _codePage = codePage;
        }

        public static DbaseCodePage Parse(byte value)
        {
            if(!Array.Exists(All, candidate => candidate._value == value))
            {
                throw new ArgumentException($"The dbase code page {value} is not supported.", nameof(value));
            }
            return Array.Find(All, candidate => candidate._value == value);
        }

        public static bool TryParse(byte value, out DbaseCodePage parsed)
        {
            if(!Array.Exists(All, candidate => candidate._value == value))
            {
                parsed = null;
            }
            else
            {
                parsed = Array.Find(All, candidate => candidate._value == value);
            }
            return parsed != null;
        }

        public bool IsCompatibleWith(Encoding encoding) => _codePage.HasValue ? encoding.CodePage == _codePage.Value : false;
        public Encoding GetEncodingOrDefault(Encoding fallback) => _codePage.HasValue ? Encoding.GetEncoding(_codePage.Value) : fallback;

        public bool Equals(DbaseCodePage other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is DbaseCodePage other && Equals(other);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();

        public byte ToByte() => _value;
        public static implicit operator byte(DbaseCodePage instance) => instance.ToByte();
    }
}
