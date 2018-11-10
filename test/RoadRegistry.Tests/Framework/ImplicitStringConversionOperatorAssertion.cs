namespace RoadRegistry
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ImplicitStringConversionOperatorAssertion : IdiomaticAssertion
    {
        public ImplicitStringConversionOperatorAssertion(ISpecimenBuilder builder, Func<string> factory)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public ISpecimenBuilder Builder { get; }
        public Func<string> Factory { get; }

        public override void Verify(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var method = type
                .GetMethods()
                .SingleOrDefault(candidate =>
                    candidate.Name == "op_Implicit"
                    && candidate.GetParameters().Length == 1
                    && candidate.GetParameters()[0].ParameterType == type
                    && candidate.ReturnParameter.ParameterType == typeof(string));

            if (method == null)
            {
                throw new ImplicitConversionOperatorException(type, typeof(string),
                    $"The type '{type.Name}' does not define an implicit conversion operator to type 'String'.");
            }

            var constructor = type
                .GetConstructors()
                .SingleOrDefault(candidate =>
                    candidate.GetParameters().Length == 1
                    && candidate.GetParameters()[0].ParameterType == typeof(string));
            
            if(constructor == null)
            {
                throw new ImplicitConversionOperatorException(type, typeof(string),
                    $"The type '{type.Name}' does not define a constructor that takes a 'String' as argument.");
            }


            var value = Factory();
            var instance = constructor.Invoke(new object[] {value});

            object result;
            try
            {
                result = method.Invoke(null, new[] { instance });
            }
            catch (Exception exception)
            {
                throw new ImplicitConversionOperatorException(type, typeof(string),
                    $"The implicit conversion operator to type 'String' of type '{type.Name}' threw an exception.", exception);
            }

            if (!((string)result).Equals(value))
                throw new ImplicitConversionOperatorException(type, typeof(string));
        }
    }
}