namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class TranslatedChangeEqualityComparer : IEqualityComparer<ITranslatedChange>
{
    private readonly Dictionary<(Type, Type), IEqualityComparer<ITranslatedChange>> _comparers;

    public TranslatedChangeEqualityComparer(bool ignoreRecordNumber = false)
    {
        _comparers = new Dictionary<(Type, Type), IEqualityComparer<ITranslatedChange>>
        {
            {
                (typeof(AddRoadSegmentToEuropeanRoad), typeof(AddRoadSegmentToEuropeanRoad)),
                new TranslatedChangeEqualityComparer<AddRoadSegmentToEuropeanRoad>(
                    new AddRoadSegmentToEuropeanRoadEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveRoadSegmentFromEuropeanRoad), typeof(RemoveRoadSegmentFromEuropeanRoad)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegmentFromEuropeanRoad>(
                    new RemoveRoadSegmentFromEuropeanRoadEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(AddRoadSegmentToNationalRoad), typeof(AddRoadSegmentToNationalRoad)),
                new TranslatedChangeEqualityComparer<AddRoadSegmentToNationalRoad>(
                    new AddRoadSegmentToNationalRoadEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveRoadSegmentFromNationalRoad), typeof(RemoveRoadSegmentFromNationalRoad)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegmentFromNationalRoad>(
                    new RemoveRoadSegmentFromNationalRoadEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(AddRoadSegmentToNumberedRoad), typeof(AddRoadSegmentToNumberedRoad)),
                new TranslatedChangeEqualityComparer<AddRoadSegmentToNumberedRoad>(
                    new AddRoadSegmentToNumberedRoadEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveRoadSegmentFromNumberedRoad), typeof(RemoveRoadSegmentFromNumberedRoad)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegmentFromNumberedRoad>(
                    new RemoveRoadSegmentFromNumberedRoadEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(AddGradeSeparatedJunction), typeof(AddGradeSeparatedJunction)),
                new TranslatedChangeEqualityComparer<AddGradeSeparatedJunction>(
                    new AddGradeSeparatedJunctionEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveGradeSeparatedJunction), typeof(RemoveGradeSeparatedJunction)),
                new TranslatedChangeEqualityComparer<RemoveGradeSeparatedJunction>(
                    new RemoveGradeSeparatedJunctionEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(AddRoadNode), typeof(AddRoadNode)),
                new TranslatedChangeEqualityComparer<AddRoadNode>(
                    new AddRoadNodeEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(ModifyRoadNode), typeof(ModifyRoadNode)),
                new TranslatedChangeEqualityComparer<ModifyRoadNode>(
                    new ModifyRoadNodeEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveRoadNode), typeof(RemoveRoadNode)),
                new TranslatedChangeEqualityComparer<RemoveRoadNode>(
                    new RemoveRoadNodeEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(AddRoadSegment), typeof(AddRoadSegment)),
                new TranslatedChangeEqualityComparer<AddRoadSegment>(
                    new AddRoadSegmentEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(ModifyRoadSegment), typeof(ModifyRoadSegment)),
                new TranslatedChangeEqualityComparer<ModifyRoadSegment>(
                    new ModifyRoadSegmentEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveRoadSegment), typeof(RemoveRoadSegment)),
                new TranslatedChangeEqualityComparer<RemoveRoadSegment>(
                    new RemoveRoadSegmentEqualityComparer(ignoreRecordNumber)
                )
            },
            {
                (typeof(RemoveOutlinedRoadSegment), typeof(RemoveOutlinedRoadSegment)),
                new TranslatedChangeEqualityComparer<RemoveOutlinedRoadSegment>(
                    new RemoveOutlinedRoadSegmentEqualityComparer(ignoreRecordNumber)
                )
            }
        };
    }

    public bool Equals(ITranslatedChange left, ITranslatedChange right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return _comparers.TryGetValue((left.GetType(), right.GetType()), out var comparer)
               && comparer.Equals(left, right);
    }

    public int GetHashCode(ITranslatedChange instance)
    {
        throw new NotSupportedException();
    }
}
