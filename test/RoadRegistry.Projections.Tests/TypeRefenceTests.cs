namespace RoadRegistry.Projections.Tests
{
    using Events;
    using Shaperon;
    using Xunit;

    public class TypeRefenceTests
    {
        [Fact]
        public void All_reference_point_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.ReferencePointTypes,
                new[]
                {
                    new ReferencePointTypeDbaseRecord(ReferencePointType.Unknown),
                    new ReferencePointTypeDbaseRecord(ReferencePointType.KilometerMarker),
                    new ReferencePointTypeDbaseRecord(ReferencePointType.HectometerMarker),
                });
        }

        private void AssertDbaseRecordCollectionsContainSameElements<T>(T[] actualRecords, T[] expectedRecords)
            where T : DbaseRecord
        {
            Assert.Equal(expectedRecords.Length, actualRecords.Length);
            for (var i = 0; i < expectedRecords.Length; i++)
            {
                Assert.Equal(expectedRecords[i], actualRecords[i]);
            }
        }
    }
}
