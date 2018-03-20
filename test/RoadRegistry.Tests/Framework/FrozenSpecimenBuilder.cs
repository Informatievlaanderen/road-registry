namespace RoadRegistry
{
    using AutoFixture.Kernel;

    public class FrozenSpecimenBuilder<T> : ISpecimenBuilder
    {
        public FrozenSpecimenBuilder(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public object Create(object request, ISpecimenContext context)
        {
            if (request != null && request.Equals(typeof(T)))
            {
                return Value;
            }

            return new NoSpecimen();
        }
    }
}
