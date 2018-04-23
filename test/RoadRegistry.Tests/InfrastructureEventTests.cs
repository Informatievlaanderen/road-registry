namespace RoadRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aiv.Vbr.AggregateSource;
    using Aiv.Vbr.EventHandling;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Events;
    using Xunit;
    using Xunit.Categories;

    public class InfrastructureEventTests
    {
        [Fact]
        [SystemTest]
        public void HasEventNameAttributes()
        {
            foreach (var type in GetAllEventTypes())
                type.GetCustomAttributes(typeof(EventNameAttribute), true)
                    .Should().NotBeEmpty($"Forgot EventName attribute on {type.FullName}");
        }

        [Fact]
        [SystemTest]
        public void HasEventDescriptionAttributes()
        {
            foreach (var type in GetAllEventTypes())
                type.GetCustomAttributes(typeof(EventDescriptionAttribute), true)
                    .Should().NotBeEmpty($"Forgot EventDescription attribute on {type.FullName}");
        }

        [Fact]
        [SystemTest]
        public void HasNoDuplicateEventNameAttributes()
        {
            var eventNames = new List<string>();

            foreach (var eventType in GetAllEventTypes())
            {
                var newNames = eventType.GetCustomAttributes(typeof(EventNameAttribute), true)
                    .OfType<EventNameAttribute>()
                    .Select(s => s.Value);

                foreach (var newName in newNames)
                {
                    eventNames.Contains(newName).Should().BeFalse($"Duplicate event name {newName}");
                    eventNames.Add(newName);
                }
            }
        }

        [Fact]
        [SystemTest]
        public void HasNoValueObjectProperty()
        {
            foreach (var type in GetAllEventTypes())
            {
                type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .SelectMany(s => GetParentTypes(s.PropertyType))
                    .Where(s => s.IsGenericType && typeof(ValueObject<>).IsAssignableFrom(s.GetGenericTypeDefinition()))
                    .Should()
                    .BeEmpty();
            }
        }

        [Fact]
        [SystemTest]
        public void HasJsonConstructor()
        {
            foreach (var type in GetAllEventTypes())
            {
                type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SelectMany(s => s.GetCustomAttributes(typeof(JsonConstructorAttribute), true))
                    .Should()
                    .NotBeEmpty($"Forgot JsonConstructor on {type.FullName}");
            }
        }

        private static IEnumerable<Type> GetAllEventTypes()
        {
            return RoadNetworkEvents.All;
        }

        //internal static IEnumerable<Type> GetAllEventTypes() =>
        //    typeof(DomainAssemblyMarker)
        //        .Assembly
        //        .GetTypes()
        //        .Where(t => t.IsClass && t.Namespace != null && t.Namespace.EndsWith(".Events"));

        public static IEnumerable<Type> GetParentTypes(Type type)
        {
            // is there any base type?
            if (type == null || (type.BaseType == null))
                yield break;

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
                yield return i;

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}
