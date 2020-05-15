namespace RoadRegistry.Wms.Projections.Framework
{
    using System.IO;

    class NonEmptyResultSetExpectationVerificationResult : ExpectationVerificationResult
    {
        public NonEmptyResultSetExpectationVerificationResult(IExpectation expectation, ExpectationVerificationResultState state)
            : base(expectation, state)
        {
        }

        public override void WriteTo(TextWriter writer)
        {
            if (Passed)
            {

            }
            else if (Failed)
            {

            }
        }
    }
}