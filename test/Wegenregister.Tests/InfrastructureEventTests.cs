namespace Wegenregister.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.EventHandling;
    using FluentAssertions;
    using Xunit;

    public class InfrastructureEventTests
    {
        [Fact]
        public void HasEventNameAttributes()
        {
            foreach (var type in GetAllEventTypes())
                type.GetCustomAttributes(typeof(EventNameAttribute), true)
                    .Should().NotBeEmpty($"Forgot EventName attribute on {type.FullName}");
        }

        [Fact]
        public void HasEventDescriptionAttributes()
        {
            foreach (var type in GetAllEventTypes())
                type.GetCustomAttributes(typeof(EventDescriptionAttribute), true)
                    .Should().NotBeEmpty($"Forgot EventDescription attribute on {type.FullName}");
        }

        [Fact]
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

        private static IEnumerable<Type> GetAllEventTypes()
        {
            bool IsEventNamespace(string @namespace) =>
                @namespace.EndsWith("Events") || @namespace.Contains(".Events.");

            return typeof(DomainAssemblyMarker)
                .Assembly
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace != null && IsEventNamespace(t.Namespace));
        }
    }
}
