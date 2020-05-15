namespace RoadRegistry.Wms.Projections.Framework
{
    using System.IO;

    class EmptyResultSetExpectationVerificationResult : ExpectationVerificationResult
    {
        public EmptyResultSetExpectationVerificationResult(IExpectation expectation, ExpectationVerificationResultState state) 
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