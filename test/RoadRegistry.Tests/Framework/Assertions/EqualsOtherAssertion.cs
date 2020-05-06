namespace RoadRegistry.Framework.Assertions
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using AutoFixture.Idioms;
    using AutoFixture.Kernel;

    public class EqualsOtherAssertion : IdiomaticAssertion
    {
        public EqualsOtherAssertion(ISpecimenBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ISpecimenBuilder Builder { get; }

        public override void Verify(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (methodInfo.ReflectedType == null || !methodInfo.IsObjectEqualsOverrideMethod())
            {
                // The method is not an override of the Object.Equals(object) method
                return;
            }

            var self = Builder.CreateAnonymous(methodInfo.ReflectedType);
            var other = Builder.CreateAnonymous(methodInfo.ReflectedType);
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
