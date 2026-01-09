namespace RoadRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;

    public class FakeKafkaProvenance: Provenance
    {
        public FakeKafkaProvenance()
            : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }
    }
}
