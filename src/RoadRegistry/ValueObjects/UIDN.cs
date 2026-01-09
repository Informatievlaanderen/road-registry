namespace RoadRegistry.ValueObjects
{
    using System;

    public sealed record UIDN(int Id, int Version)
    {
        public static UIDN Parse(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var data = value.Split('_');
            if (data.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new UIDN(int.Parse(data[0]), int.Parse(data[1]));
        }

        public override string ToString()
        {
            return $"{Id}_{Version}";
        }

        public static implicit operator string?(UIDN? instance)
        {
            return instance?.ToString();
        }
    }
}
