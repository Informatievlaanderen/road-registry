namespace Shaperon
{
    using System;
    using System.Linq;
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

        public static DbaseFieldLength GenerateDbaseInt32LengthLessThan(this IFixture fixture, DbaseFieldLength maxLength)
        {
            return new Generator<DbaseFieldLength>(fixture)
                .First(specimen => specimen < maxLength);
        }

        public static DbaseFieldLength GenerateDbaseDoubleLength(this IFixture fixture)
        {
            return new Generator<DbaseFieldLength>(fixture)
                .First(specimen => specimen > 2);
        }

        public static DbaseFieldLength GenerateDbaseDoubleLengthLessThan(this IFixture fixture, DbaseFieldLength maxLength)
        {
            return new Generator<DbaseFieldLength>(fixture)
                .First(specimen => specimen > 2 && specimen < maxLength);
        }

        public static DbaseDecimalCount GenerateDbaseDoubleDecimalCount(this IFixture fixture, DbaseFieldLength length)
        {
            return new Generator<DbaseDecimalCount>(fixture)
                .First(specimen => specimen < length - 2);
        }

        public static DbaseFieldLength GenerateDbaseSingleLength(this IFixture fixture)
        {
            return new Generator<DbaseFieldLength>(fixture)
                .First(specimen => specimen > 2);
        }

        public static DbaseFieldLength GenerateDbaseSingleLengthLessThan(this IFixture fixture, DbaseFieldLength maxLength)
        {
            return new Generator<DbaseFieldLength>(fixture)
                .First(specimen => specimen > 2 && specimen < maxLength);
        }

        public static DbaseDecimalCount GenerateDbaseSingleDecimalCount(this IFixture fixture, DbaseFieldLength length)
        {
            return new Generator<DbaseDecimalCount>(fixture)
                .First(specimen => specimen < length - 2);
        }

        public static void CustomizeDbaseField(this IFixture fixture)
        {
            fixture.Customize<DbaseField>(
                customization =>
                    customization.FromFactory<int>(
                        value => {
                            DbaseField field;
                            switch(value % 4)
                            {
                                case 1: // datetime
                                    field = new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.DateTime,
                                        fixture.Create<ByteOffset>(),
                                        new DbaseFieldLength(15),
                                        new DbaseDecimalCount(0)
                                    );
                                    break;
                                case 2: // number
                                    var doubleLength = fixture.GenerateDbaseDoubleLength();
                                    var doubleDecimalCount = fixture.GenerateDbaseDoubleDecimalCount(doubleLength);
                                    field = new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.Number,
                                        fixture.Create<ByteOffset>(),
                                        doubleLength,
                                        doubleDecimalCount
                                    );
                                    break;
                                case 3: // float
                                    var singleLength = fixture.GenerateDbaseSingleLength();
                                    var singleDecimalCount = fixture.GenerateDbaseSingleDecimalCount(singleLength);
                                    field = new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.Number,
                                        fixture.Create<ByteOffset>(),
                                        singleLength,
                                        singleDecimalCount
                                    );
                                    break;
                                default:
                                // case 0: // character
                                    field = new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.Character,
                                        fixture.Create<ByteOffset>(),
                                        fixture.Create<DbaseFieldLength>(),
                                        new DbaseDecimalCount(0)
                                    );
                                    break;
                            }
                            return field;
                        }
                    )
            );
        }

        public static void CustomizeDbaseString(this IFixture fixture)
        {
            fixture.Customize<DbaseString>(
                customization =>
                    customization
                        .FromFactory<int>(
                            value => {
                                var length = new DbaseFieldLength(new Random(value).Next(0, 255));
                                return new DbaseString(
                                    new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.Character,
                                        fixture.Create<ByteOffset>(),
                                        length,
                                        new DbaseDecimalCount(0)
                                    ),
                                    new string('a', new Random(value).Next(0, length.ToInt32())));
                            }
                        )
                        .OmitAutoProperties());
        }

        public static void CustomizeDbaseInt32(this IFixture fixture)
        {
            fixture.Customize<DbaseInt32>(
                customization =>
                    customization
                        .FromFactory<int?>(
                            value => new DbaseInt32(
                                new DbaseField(
                                    fixture.Create<DbaseFieldName>(),
                                    DbaseFieldType.Number,
                                    fixture.Create<ByteOffset>(),
                                    fixture.Create<DbaseFieldLength>(),
                                    new DbaseDecimalCount(0)
                                ), value)
                        )
                        .OmitAutoProperties());
        }

        public static void CustomizeDbaseDouble(this IFixture fixture)
        {
            fixture.Customize<DbaseDouble>(
                customization =>
                    customization
                        .FromFactory<double?>(
                            value => {
                                var length = fixture.GenerateDbaseDoubleLength();
                                var decimalCount = fixture.GenerateDbaseDoubleDecimalCount(length);
                                return new DbaseDouble(
                                    new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.Number,
                                        fixture.Create<ByteOffset>(),
                                        length,
                                        decimalCount
                                    ), value);
                            }
                        )
                        .OmitAutoProperties());
        }

        public static void CustomizeDbaseSingle(this IFixture fixture)
        {
            fixture.Customize<DbaseSingle>(
                customization =>
                    customization
                        .FromFactory<float?>(
                            value => {
                                var length = fixture.GenerateDbaseSingleLength();
                                var decimalCount = fixture.GenerateDbaseSingleDecimalCount(length);
                                return new DbaseSingle(
                                    new DbaseField(
                                        fixture.Create<DbaseFieldName>(),
                                        DbaseFieldType.Float,
                                        fixture.Create<ByteOffset>(),
                                        length,
                                        decimalCount
                                    ), value);
                            }
                        )
                        .OmitAutoProperties());
        }

        public static void CustomizeDbaseDateTime(this IFixture fixture)
        {
            fixture.Customize<DbaseDateTime>(
                customization =>
                    customization
                        .FromFactory<DateTime?>(
                            value => new DbaseDateTime(
                                new DbaseField(
                                    fixture.Create<DbaseFieldName>(),
                                    DbaseFieldType.DateTime,
                                    fixture.Create<ByteOffset>(),
                                    new DbaseFieldLength(15),
                                    new DbaseDecimalCount(0)
                                ), value)
                        )
                        .OmitAutoProperties());
        }
    }
}
