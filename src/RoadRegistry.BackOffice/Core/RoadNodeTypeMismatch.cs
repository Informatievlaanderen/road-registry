namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class RoadNodeTypeMismatch : Error
    {
        private RoadNodeTypeMismatch(IReadOnlyCollection<ProblemParameter> parameters) : base(nameof(RoadNodeTypeMismatch), parameters)
        {
        }

        public static RoadNodeTypeMismatch New(RoadNodeId node, int connectedSegmentCount,
            RoadNodeType actualType,
            RoadNodeType[] expectedTypes)
        {
            if (expectedTypes == null)
                throw new ArgumentNullException(nameof(expectedTypes));
            if (expectedTypes.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(expectedTypes));

            var parameters = new List<ProblemParameter>
            {
                new ProblemParameter("RoadNodeId",
                    node.ToInt32().ToString()),
                new ProblemParameter("ConnectedSegmentCount",
                    connectedSegmentCount.ToString(CultureInfo.InvariantCulture)),
                new ProblemParameter("Actual", actualType.ToString())
            };
            parameters.AddRange(expectedTypes.Select(type => new ProblemParameter("Expected", type.ToString())));
            return new RoadNodeTypeMismatch(parameters);
        }
    }
}
