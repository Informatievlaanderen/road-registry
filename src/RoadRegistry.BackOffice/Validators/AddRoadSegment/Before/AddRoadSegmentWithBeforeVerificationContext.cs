namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.Before
{
    using Core;

    internal class AddRoadSegmentWithBeforeVerificationContext
    {
        public AddRoadSegment AddRoadSegment { get; }
        public BeforeVerificationContext BeforeVerificationContext { get; }

        public AddRoadSegmentWithBeforeVerificationContext(AddRoadSegment addRoadSegment, BeforeVerificationContext beforeVerificationContext)
        {
            AddRoadSegment = addRoadSegment;
            BeforeVerificationContext = beforeVerificationContext;
        }
    }
}
