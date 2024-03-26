namespace RoadRegistry.Tests;

using KellermanSoftware.CompareNetObjects;

public class DateTimeComparer : KellermanSoftware.CompareNetObjects.TypeComparers.DateTimeComparer
{
    public DateTimeComparer(RootComparer rootComparer) : base(rootComparer)
    {
    }

    public override void CompareType(CompareParms parms)
    {
        if (parms.Object1 == null || parms.Object2 == null)
        {
            return;
        }

        var dateTime1 = (DateTime)parms.Object1;
        var dateTime2 = (DateTime)parms.Object2;

        if (dateTime1.Kind != dateTime2.Kind)
        {
            if (dateTime1.Kind == DateTimeKind.Local)
            {
                dateTime1 = dateTime1.ToUniversalTime();
            }

            if (dateTime2.Kind == DateTimeKind.Local)
            {
                dateTime2 = dateTime2.ToUniversalTime();
            }
        }

        if (Math.Abs(dateTime1.Subtract(dateTime2).TotalMilliseconds) <= parms.Config.MaxMillisecondsDateDifference)
        {
            return;
        }

        AddDifference(parms);
    }
}
