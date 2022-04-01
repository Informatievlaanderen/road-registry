namespace RoadRegistry.BackOffice.Validators.AddRoadNode.After
{
    using Core;

    internal class AddRoadNodeWithAfterVerificationContext
    {
        public AddRoadNode AddRoadNode { get; }
        public AfterVerificationContext AfterVerificationContext { get; }

        public AddRoadNodeWithAfterVerificationContext(AddRoadNode addRoadNode, AfterVerificationContext afterVerificationContext)
        {
            AddRoadNode = addRoadNode;
            AfterVerificationContext = afterVerificationContext;
        }
    }
}
