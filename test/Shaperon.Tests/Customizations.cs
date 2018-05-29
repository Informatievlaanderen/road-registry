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

        //TODO: Explore what effect the conversion of byte length to word length has when the byte length is not divisible by 2.
        // public static void CustomizeByteLengthCompatibleWithWordLength(this IFixture fixture)
        // {
        //     fixture.Customize<ByteOffset>(
        //         customization =>
        //             customization.FromFactory<int>(
        //                 value => value % 2 == 0 ? new ByteOffset(Math.Abs(value)) : new ByteOffset(Math.Abs(value) + 1)
        //             ));
        // }
    }
}
