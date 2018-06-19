namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Kernel;

    public class IdentifierBuilder : ISpecimenBuilder
    {
        private readonly IDictionary<Type, IList<int>> _ids = new Dictionary<Type, IList<int>>();
        private readonly IEnumerable<string> _idNames =
            new []
            {
                "id",
                "attributeid"
            };

        public object Create(object request, ISpecimenContext context)
        {
            var propertyInfo = request as PropertyInfo;
            return IsIdentifier(propertyInfo)
                ? Create(propertyInfo, context)
                : (object)new NoSpecimen();
        }

        private object Create(PropertyInfo propertyInfo, ISpecimenContext context)
        {
            if(false == _ids.ContainsKey(propertyInfo.DeclaringType))
                _ids.Add(propertyInfo.DeclaringType, new List<int>());

            var id = 0;
            while (id <= 0 || _ids[propertyInfo.DeclaringType].Contains(id))
            {
                id = context.Create<int>();
            }

            _ids[propertyInfo.DeclaringType].Add(id);
            return id;
        }

        private bool IsIdentifier(PropertyInfo propertyInfo)
        {
            return
                null != propertyInfo
                && propertyInfo.PropertyType == typeof(int)
                && _idNames.Contains(propertyInfo.Name.ToLower());
        }
    }
}
