namespace RoadRegistry.Wms.Projections.Framework
{
    using System.IO;

    class RowCountExpectationVerificationPassResult : ExpectationVerificationResult
    {
        public RowCountExpectationVerificationPassResult(IExpectation expectation) 
            : base(expectation, ExpectationVerificationResultState.Passed)
        {
        }

        public override void WriteTo(TextWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}