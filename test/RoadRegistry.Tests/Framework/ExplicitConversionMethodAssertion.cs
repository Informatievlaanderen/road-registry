namespace RoadRegistry
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ExplicitConversionMethodAssertion<TResult> : IdiomaticAssertion
    {
        public ExplicitConversionMethodAssertion(ISpecimenBuilder builder)
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
                    candidate.Name == ("To" + typeof(TResult).Name)
                    && candidate.GetParameters().Length == 0
                    && candidate.ReturnParameter.ParameterType == typeof(TResult))
                .SingleOrDefault();
            
            if (method == null)
            {
                throw new ExplicitConversionMethodException(type, typeof(TResult), 
                    $"The type '{type.Name}' does not define an explicit conversion method to type '{typeof(TResult).Name}' called 'To{typeof(TResult).Name}()'.");
            }

            var value = this.Builder.Create<TResult>();
            var composedBuilder = 
                new CompositeSpecimenBuilder(
                    new FrozenSpecimenBuilder<TResult>(value),
                    this.Builder);

            var instance = composedBuilder.CreateAnonymous(type);

            try
            {
                var result = (TResult)method.Invoke(instance, new object[0]);
                if(!result.Equals(value))
                {
                    throw new ExplicitConversionMethodException(type, typeof(TResult));
                }
            }
            catch(Exception exception)
            {
                throw new ImplicitConversionOperatorException(type, typeof(TResult), 
                    $"The explicit conversion method to type '{nameof(TResult)}' of type '{type.Name}' threw an exception.", exception);
            }            
        }
    }
}
