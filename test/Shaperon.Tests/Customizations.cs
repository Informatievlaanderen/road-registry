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

        public static void CustomizeDbaseFieldName(this IFixture fixture)
        {
            fixture.Customize<DbaseFieldName>(customization =>
                customization.FromFactory<int>(value =>
                    new DbaseFieldName(new string('a', value.AsDbaseFieldNameLength()))));
        }

        public static void CustomizeDbaseFieldLength(this IFixture fixture)
        {
            fixture.Customize<DbaseFieldLength>(
                customization =>
                    customization.FromFactory<int>(
                        value => new DbaseFieldLength(value.AsDbaseFieldLengthValue())
                    ));
        }

        public static void CustomizeDbaseDecimalCount(this IFixture fixture)
        {
            fixture.Customize<DbaseDecimalCount>(
                customization =>
                    customization.FromFactory<int>(
                        value => new DbaseDecimalCount(value.AsDbaseDecimalCountValue())
                    ));
        }

        public static void CustomizeDbaseString(this IFixture fixture)
        {
            fixture.Customize<DbaseString>(
                customization =>
                    customization.FromFactory<int>(
                        value => new DbaseString(
                            new DbaseField(
                                fixture.Create<DbaseFieldName>(),
                                DbaseFieldType.Character,
                                fixture.Create<ByteOffset>(),
                                new DbaseFieldLength(new Random(value).Next(10, 200)),
                                new DbaseDecimalCount(0)
                            ), new string('a', new Random(value).Next(1, 10)))
                    ));
        }

        public static void CustomizeDbaseInt32(this IFixture fixture)
        {
            fixture.Customize<DbaseInt32>(
                customization =>
                    customization.FromFactory<int?>(
                        value => new DbaseInt32(
                            new DbaseField(
                                fixture.Create<DbaseFieldName>(),
                                DbaseFieldType.Number,
                                fixture.Create<ByteOffset>(),
                                fixture.Create<DbaseFieldLength>(),
                                new DbaseDecimalCount(0)
                            ), value)
                    ));
        }

        public static void CustomizeDbaseDouble(this IFixture fixture)
        {
            fixture.Customize<DbaseDouble>(
                customization =>
                    customization.FromFactory<double?>(
                        value => new DbaseDouble(
                            new DbaseField(
                                fixture.Create<DbaseFieldName>(),
                                DbaseFieldType.Number,
                                fixture.Create<ByteOffset>(),
                                new DbaseFieldLength(new Random().Next(10, 20)),
                                new DbaseDecimalCount(new Random().Next(1, 10))
                            ), value)
                    ));
        }

        public static void CustomizeDbaseDateTime(this IFixture fixture)
        {
            fixture.Customize<DbaseDateTime>(
                customization =>
                    customization.FromFactory<DateTime?>(
                        value => new DbaseDateTime(
                            new DbaseField(
                                fixture.Create<DbaseFieldName>(),
                                DbaseFieldType.DateTime,
                                fixture.Create<ByteOffset>(),
                                new DbaseFieldLength(15),
                                new DbaseDecimalCount(0)
                            ), value)
                    ));
        }
    }
}
