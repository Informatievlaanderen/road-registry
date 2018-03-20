namespace RoadRegistry
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class InequalityOperatorEqualsOtherAssertion : IdiomaticAssertion
    {
        public InequalityOperatorEqualsOtherAssertion(ISpecimenBuilder builder)
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
                    candidate.Name == "op_Inequality" 
                    && candidate.GetParameters().Length == 2
                    && candidate.GetParameters()[0].ParameterType == type
                    && candidate.GetParameters()[1].ParameterType == type)
                .SingleOrDefault();

            if(method == null)
            {
                throw new InequalityOperatorException(type, $"The type {type.Name} does not implement an inequality operator for {type.Name}.");
            }

            var self = this.Builder.CreateAnonymous(type);
            var other = this.Builder.CreateAnonymous(type);

            try
            {
                var result = (bool)method.Invoke(null, new object[] { self, other });
                if(!result)
                {
                    throw new InequalityOperatorException(type);
                }
            }
            catch(Exception exception)
            {
                throw new EqualityOperatorException(type, $"The inequality operator of type {type.Name} threw an exception", exception);
            }
            
        }
    }
}
