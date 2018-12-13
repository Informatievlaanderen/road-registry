namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;
    using Messages;

    public partial class RoadNetwork
    {
        public static readonly double OneMillimeterTolerance = 0.001;

        public void Change(IRequestedChanges requestedChanges)
        {
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            var context = new ChangeContext(_view.With(requestedChanges), requestedChanges);
            requestedChanges
                .Aggregate(
                    VerifiedChanges.Empty,
                    (verifiedChanges, change) => verifiedChanges.Append(change.Verify(context)))
                .RecordUsing(Apply);
        }
    }
}
