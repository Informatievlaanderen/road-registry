namespace RoadRegistry.Projections
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoFixture.Kernel;

    public class LimitedLengthIntegerBuilder<TEntity> : ISpecimenBuilder
    {
        private readonly PropertyInfo _prop;
        private readonly Random _random;
        private readonly int _min;
        private readonly int _max;

        public LimitedLengthIntegerBuilder(Expression<Func<TEntity, int>> getter, int length)
        {
            if(length < 1)
                throw new ArgumentException("Can't create integers with a length shorter than 1");

            _min = length == 1 ? 0 : (int)Math.Pow(-10, length - 1) + 1;
            _max = (int)Math.Pow(10, length) - 1;
            _prop = (PropertyInfo)((MemberExpression)getter.Body).Member;
            _random = new Random();
        }

        public object Create(object request, ISpecimenContext context)
        {
            return IsDefinedProperty(request)
                ? _random.Next(_min, _max)
                : (object) new NoSpecimen();
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
