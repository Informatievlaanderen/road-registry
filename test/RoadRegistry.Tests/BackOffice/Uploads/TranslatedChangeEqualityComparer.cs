namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

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
                (typeof(RemoveRoadSegmentFromEuropeanRoad), typeof(RemoveRoadSegmentFromEuropeanRoad)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegmentFromEuropeanRoad>(
                    new RemoveRoadSegmentFromEuropeanRoadEqualityComparer()
                )
            },
            {
                (typeof(AddRoadSegmentToNationalRoad), typeof(AddRoadSegmentToNationalRoad)),
                new TranslatedChangeEqualityComparer<AddRoadSegmentToNationalRoad>(
                    new AddRoadSegmentToNationalRoadEqualityComparer()
                )
            },
            {
                (typeof(RemoveRoadSegmentFromNationalRoad), typeof(RemoveRoadSegmentFromNationalRoad)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegmentFromNationalRoad>(
                    new RemoveRoadSegmentFromNationalRoadEqualityComparer()
                )
            },
            {
                (typeof(AddRoadSegmentToNumberedRoad), typeof(AddRoadSegmentToNumberedRoad)),
                new TranslatedChangeEqualityComparer<AddRoadSegmentToNumberedRoad>(
                    new AddRoadSegmentToNumberedRoadEqualityComparer()
                )
            },
            {
                (typeof(ModifyRoadSegmentOnNumberedRoad), typeof(ModifyRoadSegmentOnNumberedRoad)),
                new TranslatedChangeEqualityComparer<ModifyRoadSegmentOnNumberedRoad>(
                    new ModifyRoadSegmentOnNumberedRoadEqualityComparer()
                )
            },
            {
                (typeof(RemoveRoadSegmentFromNumberedRoad), typeof(RemoveRoadSegmentFromNumberedRoad)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegmentFromNumberedRoad>(
                    new RemoveRoadSegmentFromNumberedRoadEqualityComparer()
                )
            },
            {
                (typeof(AddGradeSeparatedJunction), typeof(AddGradeSeparatedJunction)),
                new TranslatedChangeEqualityComparer<AddGradeSeparatedJunction>(
                    new AddGradeSeparatedJunctionEqualityComparer()
                )
            },
            {
                (typeof(ModifyGradeSeparatedJunction), typeof(ModifyGradeSeparatedJunction)),
                new TranslatedChangeEqualityComparer<ModifyGradeSeparatedJunction>(
                    new ModifyGradeSeparatedJunctionEqualityComparer()
                )
            },
            {
                (typeof(RemoveGradeSeparatedJunction), typeof(RemoveGradeSeparatedJunction)),
                new TranslatedChangeEqualityComparer<RemoveGradeSeparatedJunction>(
                    new RemoveGradeSeparatedJunctionEqualityComparer()
                )
            },
            {
                (typeof(AddRoadNode), typeof(AddRoadNode)),
                new TranslatedChangeEqualityComparer<AddRoadNode>(
                    new AddRoadNodeEqualityComparer()
                )
            },
            {
                (typeof(ModifyRoadNode), typeof(ModifyRoadNode)),
                new TranslatedChangeEqualityComparer<ModifyRoadNode>(
                    new ModifyRoadNodeEqualityComparer()
                )
            },
            {
                (typeof(RemoveRoadNode), typeof(RemoveRoadNode)),
                new TranslatedChangeEqualityComparer<RemoveRoadNode>(
                    new RemoveRoadNodeEqualityComparer()
                )
            },
            {
                (typeof(AddRoadSegment), typeof(AddRoadSegment)),
                new TranslatedChangeEqualityComparer<AddRoadSegment>(
                    new AddRoadSegmentEqualityComparer()
                )
            },
            {
                (typeof(ModifyRoadSegment), typeof(ModifyRoadSegment)),
                new TranslatedChangeEqualityComparer<ModifyRoadSegment>(
                    new ModifyRoadSegmentEqualityComparer()
                )
            },
            {
                (typeof(RemoveRoadSegment), typeof(RemoveRoadSegment)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegment>(
                    new RemoveRoadSegmentEqualityComparer()
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
