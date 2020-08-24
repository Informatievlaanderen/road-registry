namespace RoadRegistry.Syndication.Projections
{
    using ProjectionHost.Mapping;
    using Xunit;

    public class AtomEntrySerializerMappingTests
    {
        [Fact]
        public void Has_serializer_for_gemeente()
        {
            Assert.True(new AtomEntrySerializerMapping().HasSerializerFor("https://data.vlaanderen.be/ns/gemeente"));
        }

        [Fact]
        public void Has_serializer_for_straatnaam()
        {
            Assert.True(new AtomEntrySerializerMapping().HasSerializerFor("https://data.vlaanderen.be/ns/straatnaam"));
        }
    }
}
