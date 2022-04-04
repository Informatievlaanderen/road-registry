namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.After
{
    using Core;

    internal class AddRoadSegmentWithAfterVerificationContext
    {
        public AddRoadSegment AddRoadSegment { get; }
        public AfterVerificationContext AfterVerificationContext { get; }

        public AddRoadSegmentWithAfterVerificationContext(AddRoadSegment addRoadSegment, AfterVerificationContext afterVerificationContext)
        {
            AddRoadSegment = addRoadSegment;
            AfterVerificationContext = afterVerificationContext;
        }
    }
}
