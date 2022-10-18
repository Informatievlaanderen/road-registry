namespace RoadRegistry.Tests.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class ShapeRecordEqualityComparer : IEqualityComparer<ShapeRecord>
{
    private readonly IEqualityComparer<ShapeContent> _comparer;

    public ShapeRecordEqualityComparer()
    {
        _comparer = new ShapeContentEqualityComparer();
    }

    public bool Equals(ShapeRecord left, ShapeRecord right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        var sameHeader = left.Header.Equals(right.Header);
        var sameLength = left.Length.Equals(right.Length);
        var sameContent = _comparer.Equals(left.Content, right.Content);
        return sameHeader && sameLength && sameContent;
    }

    public int GetHashCode(ShapeRecord instance)
    {
        return instance.Header.GetHashCode()
               ^ instance.Length.GetHashCode()
               ^ _comparer.GetHashCode(instance.Content);
    }

    private class ShapeContentEqualityComparer : IEqualityComparer<ShapeContent>
    {
        public bool Equals(ShapeContent left, ShapeContent right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            if (left is NullShapeContent && right is NullShapeContent)
                return true;
            if (left is PointShapeContent leftPointContent && right is PointShapeContent rightPointContent)
                return Equals(leftPointContent, rightPointContent);
            if (left is PolyLineMShapeContent leftLineContent && right is PolyLineMShapeContent rightLineContent)
                return Equals(leftLineContent, rightLineContent);
            return false;
        }

        private bool Equals(PointShapeContent left, PointShapeContent right)
        {
            var sameContentLength = left.ContentLength.Equals(right.ContentLength);
            var sameShapeType = left.ShapeType.Equals(right.ShapeType);
            var sameShape = left.Shape.Equals(right.Shape);
            return sameContentLength && sameShapeType && sameShape;
        }

        private bool Equals(PolyLineMShapeContent left, PolyLineMShapeContent right)
        {
            var sameContentLength = left.ContentLength.Equals(right.ContentLength);
            var sameShapeType = left.ShapeType.Equals(right.ShapeType);
            var sameShape = left.Shape.Equals(right.Shape);
            return sameContentLength && sameShapeType && sameShape;
        }

        public int GetHashCode(ShapeContent instance)
        {
            if (instance is NullShapeContent)
                return 0;
            if (instance is PointShapeContent pointContent)
                return pointContent.ContentLength.GetHashCode() ^ pointContent.ShapeType.GetHashCode() ^
                       pointContent.Shape.GetHashCode();
            if (instance is PolyLineMShapeContent lineContent)
                return lineContent.ContentLength.GetHashCode() ^ lineContent.ShapeType.GetHashCode() ^
                       lineContent.Shape.GetHashCode();
            return -1;
        }
    }
}