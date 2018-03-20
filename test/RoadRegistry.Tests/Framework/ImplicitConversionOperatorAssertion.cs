namespace RoadRegistry
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ImplicitConversionOperatorAssertion<TResult> : IdiomaticAssertion
    {
        public ImplicitConversionOperatorAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ISpecimenBuilder Builder { get; }

        public override void Verify(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var method = type
                .GetMethods()
                .Where(candidate => 
                    candidate.Name == "op_Implicit" 
                    && candidate.GetParameters().Length == 1
                    && candidate.GetParameters()[0].ParameterType == type
                    && candidate.ReturnParameter.ParameterType == typeof(TResult))
                .SingleOrDefault();
            
            if (method == null)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult), 
                    $"The type '{type.Name}' does not define an implicit conversion operator to type '{typeof(TResult).Name}'.");
            }

            var value = this.Builder.Create<TResult>();
            var composedBuilder = 
                new CompositeSpecimenBuilder(
                    new FrozenSpecimenBuilder<TResult>(value),
                    this.Builder);

            var instance = composedBuilder.CreateAnonymous(type);

            try
            {
                
                var result = (TResult)method.Invoke(null, new object[] { instance });
                if(!result.Equals(value))
                {
                    throw new ImplicitConversionOperatorException(type, typeof(TResult));
                }
            }
            catch(Exception exception)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult), 
                    $"The implicit conversion operator to type '{typeof(TResult).Name}' of type '{type.Name}' threw an exception.", exception);
            }
        }
    }
}
