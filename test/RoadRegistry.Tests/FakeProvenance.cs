namespace RoadRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;

    public class FakeProvenance: Provenance
    {
        public FakeProvenance()
            : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }
    }
}
