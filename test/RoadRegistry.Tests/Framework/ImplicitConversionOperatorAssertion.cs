namespace RoadRegistry
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ImplicitConversionOperatorAssertion<TResult> : IdiomaticAssertion
    {
        public ImplicitConversionOperatorAssertion(
            Func<TResult> valueFactory,
            Func<TResult, object> sutFactory)
        {
            ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            SutFactory = sutFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        }

        public Func<TResult> ValueFactory { get; }
        public Func<TResult, object> SutFactory { get; }

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
                    && candidate.ReturnParameter.ParameterType == typeof(TResult));

            if (method == null)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult),
                    $"The type '{type.Name}' does not define an implicit conversion operator to type '{typeof(TResult).Name}'.");
            }

            var value = ValueFactory();
            var instance = SutFactory(value);

            object result;
            try
            {
                result = method.Invoke(null, new[] { instance });
            }
            catch (Exception exception)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult),
                    $"The implicit conversion operator to type '{typeof(TResult).Name}' of type '{type.Name}' threw an exception.", exception);
            }

            if (!((TResult)result).Equals(value))
                throw new ImplicitConversionOperatorException(type, typeof(TResult));
        }
    }
}
