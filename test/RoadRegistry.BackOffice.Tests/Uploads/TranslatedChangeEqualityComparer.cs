namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;

    public class TranslatedChangeEqualityComparer : IEqualityComparer<ITranslatedChange>
    {
        private readonly Dictionary<(Type, Type), IEqualityComparer<ITranslatedChange>> _comparers;

        public TranslatedChangeEqualityComparer()
        {
            _comparers = new Dictionary<(Type, Type), IEqualityComparer<ITranslatedChange>>
            {
                {
                    (typeof(AddRoadSegmentToEuropeanRoad), typeof(AddRoadSegmentToEuropeanRoad)),
                    new TranslatedChangeEqualityComparer<AddRoadSegmentToEuropeanRoad>(
                        new AddRoadSegmentToEuropeanRoadEqualityComparer()
                    )
                },
                {
                    (typeof(AddRoadSegmentToNationalRoad), typeof(AddRoadSegmentToNationalRoad)),
                    new TranslatedChangeEqualityComparer<AddRoadSegmentToNationalRoad>(
                        new AddRoadSegmentToNationalRoadEqualityComparer()
                    )
                },
                {
                    (typeof(AddRoadSegmentToNumberedRoad), typeof(AddRoadSegmentToNumberedRoad)),
                    new TranslatedChangeEqualityComparer<AddRoadSegmentToNumberedRoad>(
                        new AddRoadSegmentToNumberedRoadEqualityComparer()
                    )
                },
                {
                    (typeof(AddGradeSeparatedJunction), typeof(AddGradeSeparatedJunction)),
                    new TranslatedChangeEqualityComparer<AddGradeSeparatedJunction>(
                        new AddGradeSeparatedJunctionEqualityComparer()
                    )
                },
                {
                    (typeof(AddRoadNode), typeof(AddRoadNode)),
                    new TranslatedChangeEqualityComparer<AddRoadNode>(
                        new AddRoadNodeEqualityComparer()
                    )
                },
                {
                    (typeof(AddRoadSegment), typeof(AddRoadSegment)),
                    new TranslatedChangeEqualityComparer<AddRoadSegment>(
                        new AddRoadSegmentEqualityComparer()
                    )
                }
            };
        }

        public bool Equals(ITranslatedChange left, ITranslatedChange right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return _comparers.TryGetValue((left.GetType(), right.GetType()), out var comparer)
                   && comparer.Equals(left, right);
        }

        public int GetHashCode(ITranslatedChange instance)
        {
            throw new NotSupportedException();
        }
    }
}
