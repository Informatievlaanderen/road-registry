namespace RoadRegistry.Wms.Projections.Framework
{
    using System.IO;

    class RowCountExpectationVerificationFailResult : ExpectationVerificationResult
    {
        private readonly int _actualRowCount;

        public RowCountExpectationVerificationFailResult(IExpectation expectation, int actualRowCount)
            : base(expectation, ExpectationVerificationResultState.Failed)
        {
            _actualRowCount = actualRowCount;
        }

        public override void WriteTo(TextWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}