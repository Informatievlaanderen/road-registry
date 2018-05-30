namespace Shaperon
{
    using System;
    using AutoFixture;

    internal static class Customizations
    {
        public static void CustomizeWordLength(this IFixture fixture)
        {
            fixture.Customize<WordLength>(
                customization =>
                    customization.FromFactory<int>(
                        value => new WordLength(Math.Abs(value))
                    ));
        }

        public static void CustomizeWordOffset(this IFixture fixture)
        {
            fixture.Customize<WordOffset>(
                customization =>
                    customization.FromFactory<int>(
                        value => new WordOffset(Math.Abs(value))
                    ));
        }

        public static void CustomizeByteOffset(this IFixture fixture)
        {
            fixture.Customize<ByteOffset>(
                customization =>
                    customization.FromFactory<int>(
                        value => new ByteOffset(Math.Abs(value))
                    ));
        }

        public static void CustomizeByteLength(this IFixture fixture)
        {
            fixture.Customize<ByteLength>(
                customization =>
                    customization.FromFactory<int>(
                        value => new ByteLength(value.AsByteLengthValue())
                    ));
        }
        public static void CustomizeRecordNumber(this IFixture fixture)
        {
            fixture.Customize<RecordNumber>(
                customization =>
                    customization.FromFactory<int>(
                        value => new RecordNumber(value.AsRecordNumberValue())
                    ));
        }
    }
}
