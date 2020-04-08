namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class ConstructorBasedEqualsOtherAssertion : IdiomaticAssertion
    {
        public ConstructorBasedEqualsOtherAssertion(ISpecimenBuilder builder, ConstructorInfo constructor)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
        }

        public ISpecimenBuilder Builder { get; }
        public ConstructorInfo Constructor { get; }

        public override void Verify(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (methodInfo.ReflectedType == null || !methodInfo.IsObjectEqualsOverrideMethod())
            {
                // The method is not an override of the Object.Equals(object) method
                return;
            }

            var selfParameters = new object[Constructor.GetParameters().Length];
            foreach (var parameter in Constructor.GetParameters())
            {
                selfParameters[parameter.Position] = Builder.CreateAnonymous(parameter.ParameterType);
            }

            var otherParameters = new object[Constructor.GetParameters().Length];
            selfParameters.CopyTo(otherParameters, 0);
            var position = (int) Builder.CreateAnonymous(typeof(int)) % otherParameters.Length;
            while (otherParameters[position].Equals(selfParameters[position]))
            {
                otherParameters[position] =
                    Builder.CreateAnonymous(Constructor.GetParameters()[position].ParameterType);
            }

            var self = Constructor.Invoke(selfParameters);
            var other = Constructor.Invoke(otherParameters);

            var equalsResult = self.Equals(other);

            if (equalsResult)
            {
                throw new EqualsOverrideException(string.Format(CultureInfo.CurrentCulture,
                    "The type '{0}' overrides the object.Equals(object) method incorrectly, " +
                    "calling x.Equals(y) should return false.",
                    methodInfo.ReflectedType.FullName));
            }
        }
    }
}
