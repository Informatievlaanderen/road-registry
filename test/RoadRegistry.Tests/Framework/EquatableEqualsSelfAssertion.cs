namespace RoadRegistry
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class EquatableEqualsSelfAssertion : IdiomaticAssertion
    {
        public EquatableEqualsSelfAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ISpecimenBuilder Builder { get; }

        public override void Verify(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var equatableType = typeof(IEquatable<>).MakeGenericType(type);
            if(!equatableType.IsAssignableFrom(type))
            {
                throw new EquatableEqualsException(type, $"The type {type.Name} does not implement IEquatable<{type.Name}>.");
            }

            var method = equatableType.GetMethods().Single();
            var self = this.Builder.CreateAnonymous(type);

            try
            {
                var result = (bool)method.Invoke(self, new object[] { self });
                if(!result)
                {
                    throw new EquatableEqualsException(type);
                }
            }
            catch(Exception exception)
            {
                throw new EquatableEqualsException(type, $"The IEquatable<{type.Name}>.Equals method of type {type.Name} threw an exception.", exception);
            }
        }
    }
}
