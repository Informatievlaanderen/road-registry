namespace Shaperon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class DbaseCodePageTests
    {
        private readonly Fixture _fixture;

        public DbaseCodePageTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeDbaseCodePage();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void ParseValueMustBeSupported()
        {
            var all_supported = Array.ConvertAll(DbaseCodePage.All, _ => _.ToByte());
            var value = new Generator<byte>(_fixture)
                .First(candidate => !Array.Exists(all_supported, supported => supported == candidate));

            Assert.Throws<ArgumentException>(() => DbaseCodePage.Parse(value));
        }

        [Fact]
        public void TryParseWithUnsupportedValueReturnsExpectedResult()
        {
            var all_supported = Array.ConvertAll(DbaseCodePage.All, _ => _.ToByte());
            var value = new Generator<byte>(_fixture)
                .First(candidate => !Array.Exists(all_supported, supported => supported == candidate));

            var result = DbaseCodePage.TryParse(value, out DbaseCodePage parsed);
            Assert.False(result);
            Assert.Null(parsed);
        }

        [Fact]
        public void TryParseWithSupportedValueReturnsExpectedResult()
        {
            var supported = _fixture.Create<DbaseCodePage>();

            var result = DbaseCodePage.TryParse(supported.ToByte(), out DbaseCodePage parsed);
            Assert.True(result);
            Assert.Equal(supported, parsed);
        }

        [Theory]
        [MemberData(nameof(GetEncodingOrDefaultCases))]
        public void GetEncodingOrDefaultReturnsExpectedResult(DbaseCodePage sut, Encoding fallback, Encoding expected)
        {
            var result = sut.GetEncodingOrDefault(fallback);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> GetEncodingOrDefaultCases
        {
            get {
                yield return new object[] { DbaseCodePage.DOSUSA, Encoding.Default, Encoding.GetEncoding(437) };
                yield return new object[] { DbaseCodePage.DOSMultilingual, Encoding.Default, Encoding.GetEncoding(850) };
                yield return new object[] { DbaseCodePage.WindowsANSI, Encoding.Default, Encoding.GetEncoding(1252) };
                yield return new object[] { DbaseCodePage.StandardMacintosh, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.EEMSDOS, Encoding.Default, Encoding.GetEncoding(852) };
                yield return new object[] { DbaseCodePage.NordicMSDOS, Encoding.Default, Encoding.GetEncoding(865) };
                yield return new object[] { DbaseCodePage.RussianMSDOS, Encoding.Default, Encoding.GetEncoding(866) };
                yield return new object[] { DbaseCodePage.IcelandicMSDOS, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.CzechMSDOS, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.PolishMSDOS, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.GreekMSDOS, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.TurkishMSDOS, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.RussianMacintosh, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.EasternEuropeanMacintosh, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.GreekMacintosh, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.WindowsEE, Encoding.Default, Encoding.GetEncoding(1250) };
                yield return new object[] { DbaseCodePage.RussianWindows, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.TurkishWindows, Encoding.Default, Encoding.Default };
                yield return new object[] { DbaseCodePage.GreekWindows, Encoding.Default, Encoding.Default };
            }
        }

        [Theory]
        [MemberData(nameof(IsCompatibleWithCases))]
        public void IsCompatibleWithReturnsExpectedResult(DbaseCodePage sut, Encoding encoding, bool expected)
        {
            var result = sut.IsCompatibleWith(encoding);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> IsCompatibleWithCases
        {
            get {
                yield return new object[] { DbaseCodePage.DOSUSA, Encoding.GetEncoding(437), true };
                yield return new object[] { DbaseCodePage.DOSUSA, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.DOSMultilingual, Encoding.GetEncoding(850), true };
                yield return new object[] { DbaseCodePage.DOSMultilingual, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.WindowsANSI, Encoding.GetEncoding(1252), true };
                yield return new object[] { DbaseCodePage.WindowsANSI, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.StandardMacintosh, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.EEMSDOS, Encoding.GetEncoding(852), true };
                yield return new object[] { DbaseCodePage.EEMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.NordicMSDOS, Encoding.GetEncoding(865), true };
                yield return new object[] { DbaseCodePage.NordicMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.RussianMSDOS, Encoding.GetEncoding(866), true };
                yield return new object[] { DbaseCodePage.RussianMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.IcelandicMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.CzechMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.PolishMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.GreekMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.TurkishMSDOS, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.RussianMacintosh, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.EasternEuropeanMacintosh, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.GreekMacintosh, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.WindowsEE, Encoding.GetEncoding(1250), true };
                yield return new object[] { DbaseCodePage.WindowsEE, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.RussianWindows, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.TurkishWindows, Encoding.Default, false };
                yield return new object[] { DbaseCodePage.GreekWindows, Encoding.Default, false };
            }
        }

        [Fact]
        public void ToByteReturnsExpectedValue()
        {
            var value = _fixture.Create<byte>().AsDbaseCodePageValue();
            var sut = DbaseCodePage.Parse(value);

            var result = sut.ToByte();

            Assert.Equal((byte)value, result);
        }

        [Fact]
        public void ImplicitConversionToByteReturnsExpectedValue()
        {
            var value = _fixture.Create<byte>().AsDbaseCodePageValue();
            var sut = DbaseCodePage.Parse(value);

            byte result = sut;

            Assert.Equal((byte)value, result);
        }

        [Fact]
        public void ToStringReturnsExpectedValue()
        {
            var value = _fixture.Create<byte>().AsDbaseCodePageValue();
            var sut = DbaseCodePage.Parse(value);

            var result = sut.ToString();

            Assert.Equal(value.ToString(), result);
        }

        [Fact]
        public void VerifyEquality()
        {
            new CompositeIdiomaticAssertion(
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(DbaseCodePage));
        }

        [Fact]
        public void IsEquatableToDbaseCodePage()
        {
            Assert.IsAssignableFrom<IEquatable<DbaseCodePage>>(_fixture.Create<DbaseCodePage>());
        }
    }
}
