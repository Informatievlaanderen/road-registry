namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Kernel;

    public class StringPropertyTruncateSpecimenBuilder<TEntity> : ISpecimenBuilder
    {
        private readonly int _length;
        private readonly PropertyInfo _prop;

        public StringPropertyTruncateSpecimenBuilder(Expression<Func<TEntity, string>> getter, int length)
        {
            _length = length;
            _prop = (PropertyInfo)((MemberExpression)getter.Body).Member;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!IsDefinedProperty(request))
                return (object)new NoSpecimen();

            var value = context.Create<string>();
            return value.Length > _length
                ? value.Substring(0, _length)
                : value;
        }

        private bool IsDefinedProperty(object request)
        {
            var pi = request as PropertyInfo;
            return
                null != pi
                && pi.DeclaringType == _prop.DeclaringType
                && pi.Name == _prop.Name;
        }
    }
}
