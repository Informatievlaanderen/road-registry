namespace RoadRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NodaTime;

    public class FakeProvenance: Provenance
    {
        public FakeProvenance()
            : base(Instant.MaxValue, Application.RoadRegistry, new Reason(string.Empty), new Operator("TEST"), Modification.Unknown, Organisation.DigitaalVlaanderen)
        {
        }
    }
}
