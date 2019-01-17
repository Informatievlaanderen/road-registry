namespace RoadRegistry.Translation
{
    using System;
    using System.Collections.Generic;
    using Aiv.Vbr.Shaperon;

    public class EuropeanRoadComparisonDbaseRecord : DbaseRecord
    {
        public static readonly EuropeanRoadComparisonDbaseSchema Schema = new EuropeanRoadComparisonDbaseSchema();

        public EuropeanRoadComparisonDbaseRecord()
        {
            EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            EUNUMMER = new DbaseString(Schema.EUNUMMER);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                EU_OIDN,
                WS_OIDN,
                EUNUMMER,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 EU_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseString EUNUMMER { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }

        public void PopulateFrom(DbaseRecord record)
        {
            var index = new Dictionary<DbaseField, Action<DbaseFieldValue>>
            {
                {EU_OIDN.Field, value => value.CopyValueTo(EU_OIDN)},
                {WS_OIDN.Field, value => value.CopyValueTo(WS_OIDN)},
                {EUNUMMER.Field, value => value.CopyValueTo(EUNUMMER)},
                {TransactID.Field, value => value.CopyValueTo(TransactID)},
                {RecordType.Field, value => value.CopyValueTo(RecordType)}
            };
            foreach (var value in record.Values)
            {
                if (index.TryGetValue(value.Field, out var copier))
                {
                    copier(value);
                }
            }
        }
    }

    public static class DbaseFieldValueExtensions
    {
        public static void CopyValueTo(this DbaseFieldValue value, DbaseInt32 other)
        {
            value.Inspect(new DbaseInt32Setter(other));
        }

        public static void CopyValueTo(this DbaseFieldValue value, DbaseString other)
        {
            value.Inspect(new DbaseStringSetter(other));
        }

        public static void CopyValueTo(this DbaseFieldValue value, DbaseDouble other)
        {
            value.Inspect(new DbaseDoubleSetter(other));
        }

        public static void CopyValueTo(this DbaseFieldValue value, DbaseSingle other)
        {
            value.Inspect(new DbaseSingleSetter(other));
        }

        public static void CopyValueTo(this DbaseFieldValue value, DbaseDecimal other)
        {
            value.Inspect(new DbaseDecimalSetter(other));
        }

        public static void CopyValueTo(this DbaseFieldValue value, DbaseBoolean other)
        {
            value.Inspect(new DbaseBooleanSetter(other));
        }

        private class DbaseDateTimeSetter : DbaseFieldValueSetter
        {
            private readonly DbaseDateTime _value;

            public DbaseDateTimeSetter(DbaseDateTime value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseDateTime value)
            {
                _value.Value = value.Value;
            }
        }

        private class DbaseInt32Setter : DbaseFieldValueSetter
        {
            private readonly DbaseInt32 _value;

            public DbaseInt32Setter(DbaseInt32 value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseInt32 value)
            {
                _value.Value = value.Value;
            }
        }

        private class DbaseDecimalSetter : DbaseFieldValueSetter
        {
            private readonly DbaseDecimal _value;

            public DbaseDecimalSetter(DbaseDecimal value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseDecimal value)
            {
                _value.Value = value.Value;
            }
        }


        private class DbaseBooleanSetter : DbaseFieldValueSetter
        {
            private readonly DbaseBoolean _value;

            public DbaseBooleanSetter(DbaseBoolean value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseBoolean value)
            {
                _value.Value = value.Value;
            }
        }

        private class DbaseStringSetter : DbaseFieldValueSetter
        {
            private readonly DbaseString _value;

            public DbaseStringSetter(DbaseString value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseString value)
            {
                _value.Value = value.Value;
            }
        }

        private class DbaseSingleSetter : DbaseFieldValueSetter
        {
            private readonly DbaseSingle _value;

            public DbaseSingleSetter(DbaseSingle value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseSingle value)
            {
                _value.Value = value.Value;
            }
        }

        private class DbaseDoubleSetter : DbaseFieldValueSetter
        {
            private readonly DbaseDouble _value;

            public DbaseDoubleSetter(DbaseDouble value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public override void Inspect(DbaseDouble value)
            {
                _value.Value = value.Value;
            }
        }

        private abstract class DbaseFieldValueSetter : IDbaseFieldValueInspector
        {
            public virtual void Inspect(DbaseDateTime value)
            {
            }

            public virtual void Inspect(DbaseDecimal value)
            {
            }

            public virtual void Inspect(DbaseDouble value)
            {
            }

            public virtual void Inspect(DbaseSingle value)
            {
            }

            public virtual void Inspect(DbaseInt32 value)
            {
            }

            public virtual void Inspect(DbaseString value)
            {
            }

            public virtual void Inspect(DbaseBoolean value)
            {
            }
        }

        private class DbaseFieldValueSetter2 : IDbaseFieldValueInspector
        {
            private readonly DbaseFieldValue _target;

            public DbaseFieldValueSetter2(DbaseFieldValue target)
            {
                _target = target ?? throw new ArgumentNullException(nameof(target));
            }

            public void Inspect(DbaseDateTime value)
            {
                if (_target.Field.Equals(value.Field))
                {

                }
            }

            public virtual void Inspect(DbaseDecimal value)
            {
            }

            public virtual void Inspect(DbaseDouble value)
            {
            }

            public virtual void Inspect(DbaseSingle value)
            {
            }

            public virtual void Inspect(DbaseInt32 value)
            {
            }

            public virtual void Inspect(DbaseString value)
            {
            }

            public virtual void Inspect(DbaseBoolean value)
            {
            }
        }
    }
}
