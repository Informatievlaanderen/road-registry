namespace RoadRegistry.BackOffice.Messages
{
    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class SchemaDump
    {
        private readonly ITestOutputHelper _output;

        public SchemaDump(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "Useful to run when you want to review message names")]
        public void DumpMessageNames()
        {
            var message = String.Join(Environment.NewLine, typeof(RoadNetworkEvents).Assembly.GetTypes()
                .Where(type => type.Namespace != null && type.Namespace.StartsWith(typeof(RoadNetworkEvents).Namespace) && !type.IsNested)
                .Select(type => type.Name)
                .OrderBy(name => name));
            _output.WriteLine("Message Names:");
            _output.WriteLine(message);
        }

        [Fact(Skip = "Useful to run when you want to review message property names")]
        public void DumpMessageProperties()
        {
            var message = String.Join(Environment.NewLine, typeof(RoadNetworkEvents).Assembly.GetTypes()
                .Where(type => type.Namespace != null && type.Namespace.StartsWith(typeof(RoadNetworkEvents).Namespace) && !type.IsNested)
                .SelectMany(type => type.GetProperties())
                .Select(property => property.Name)
                .Distinct()
                .OrderBy(name => name));
            _output.WriteLine("Propery Names:");
            _output.WriteLine(message);
        }
    }
}
