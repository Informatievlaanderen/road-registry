namespace RoadRegistry.Tests.BackOffice;

using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class ObjectHasherTests
{
    [Fact]
    public void GetHashFields_Complex()
    {
        var complexObject = new ComplexObject
        {
            Int = 90,
            NullableInt = 91,
            Boolean = true,
            Enum = TestEnum.Value2,
            Double = 3.1,
            DateTime = DateTime.Now,
            String = "String",
            StringArray = new[] { "String1", "String2", "String3" },
            ObjectEnumerable = new[] { new BasicObject { Int = 10, NullableInt = null }, new BasicObject { Int = 11, NullableInt = 12 } },
            HashFieldsObjectCollection = new[] { new HashFieldsObject { Name = "Name", NameNotToBeHashed = "NameNotToBeHashed" } },
            GradeSeparatedJunctionType = GradeSeparatedJunctionType.Bridge,
            HashObject = new HashObject { Int = 50 }
        };

        var hashFields = ObjectHasher.GetHashFields(complexObject);
        var expected = new[]
        {
            complexObject.Boolean.ToString(),
            complexObject.Enum.ToString(),
            complexObject.Double.ToString(),
            complexObject.DateTime.ToString(),
            complexObject.String,
            complexObject.StringArray[0],
            complexObject.StringArray[1],
            complexObject.StringArray[2],
            complexObject.ObjectEnumerable.First().Int.ToString(),
            string.Empty,
            complexObject.ObjectEnumerable.Last().Int.ToString(),
            complexObject.ObjectEnumerable.Last().NullableInt.ToString(),
            complexObject.HashFieldsObjectCollection.Single().Name,
            complexObject.GradeSeparatedJunctionType.Translation.Description,
            complexObject.GradeSeparatedJunctionType.Translation.Identifier.ToString(),
            complexObject.GradeSeparatedJunctionType.Translation.Name,
            complexObject.HashObject.Int.ToString(),
            complexObject.Int.ToString(),
            complexObject.NullableInt.ToString()
        };

        Assert.Equal(expected, hashFields);

        hashFields = ObjectHasher.GetHashFields(complexObject.HashObject);
        expected = new[]
        {
            complexObject.HashObject.Int.ToString()
        };

        Assert.Equal(expected, hashFields);
    }

    private class BasicObject
    {
        public int Int { get; init; }
        public int? NullableInt { get; init; }
    }

    private class ComplexObject : BasicObject
    {
        public bool Boolean { get; init; }
        public TestEnum Enum { get; init; }
        public double Double { get; init; }
        public DateTime DateTime { get; init; }
        public string String { get; init; }
        public string[] StringArray { get; init; }
        public IEnumerable<BasicObject> ObjectEnumerable { get; init; }
        public ICollection<HashFieldsObject> HashFieldsObjectCollection { get; init; }
        public GradeSeparatedJunctionType GradeSeparatedJunctionType { get; init; }
        public HashObject HashObject { get; init; }
    }

    private enum TestEnum
    {
        Value1,
        Value2
    }

    public class HashFieldsObject : IHaveHashFields
    {
        public string Name { get; init; }
        public string NameNotToBeHashed { get; init; }

        public IEnumerable<string> GetHashFields()
        {
            yield return Name ?? string.Empty;
        }
    }

    public class HashObject : IHaveHash
    {
        public int Int { get; init; }

        public IEnumerable<string> GetHashFields()
        {
            return ObjectHasher.GetHashFields(this);
        }

        public string GetHash()
        {
            return this.ToEventHash();
        }
    }
}
