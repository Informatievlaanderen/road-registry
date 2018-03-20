namespace RoadRegistry
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class EqualityOperatorEqualsSelfAssertion : IdiomaticAssertion
    {
        public EqualityOperatorEqualsSelfAssertion(ISpecimenBuilder builder)
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
                    candidate.Name == "op_Equality" 
                    && candidate.GetParameters().Length == 2
                    && candidate.GetParameters()[0].ParameterType == type
                    && candidate.GetParameters()[1].ParameterType == type)
                .SingleOrDefault();

            if(method == null)
            {
                throw new EqualityOperatorException(type, $"The type {type.Name} does not implement an equality operator for {type.Name}.");
            }

            var self = this.Builder.CreateAnonymous(type);

            try
            {
                var result = (bool)method.Invoke(null, new object[] { self, self });
                if(!result)
                {
                    throw new EqualityOperatorException(type);
                }
            }
            catch(Exception exception)
            {
                throw new EqualityOperatorException(type, $"The equality operator of type {type.Name} threw an exception", exception);
            }
        }
    }
}
