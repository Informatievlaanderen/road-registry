namespace RoadRegistry.Translation
{
    using System;
    using Aiv.Vbr.Shaperon;

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