namespace RoadRegistry.Tests.BackOffice;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.DutchTranslations;
using RoadRegistry.BackOffice.Messages;

public class ProblemTranslatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ProblemTranslatorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EnsureAllProblemCodeHaveADutchTranslation()
    {
        LoadAllProblemCodes();

        var defaultProblemParameters = new Dictionary<ProblemCode, ProblemParameter[]>
        {
            {
                ProblemCode.RoadNode.TypeMismatch, [
                    new ProblemParameter { Name = "RoadNodeId", Value = "1" },
                    new ProblemParameter { Name = "ConnectedSegmentCount", Value = "1" },
                    new ProblemParameter { Name = "Actual", Value = RoadNodeType.EndNode.ToString() }
                ]
            },
            {
                ProblemCode.StreetName.RegistryUnexpectedError, [
                    new ProblemParameter { Name = "StatusCode", Value = "500" }
                ]
            },
            {
                ProblemCode.RoadSegment.MaintenanceAuthority.NotKnown, [
                    new ProblemParameter { Name = "OrganizationId", Value = "ABC" }
                ]
            },
            {
                ProblemCode.RoadSegment.NotRemovedBecauseCategoryIsInvalid, [
                    new ProblemParameter { Name = "Identifier", Value = "1" },
                    new ProblemParameter { Name = "Category", Value = "abc" }
                ]
            },
            {
                ProblemCode.RoadNetwork.Disconnected, [
                    new ProblemParameter { Name = "StartNodeId", Value = "1" },
                    new ProblemParameter { Name = "EndNodeId", Value = "1" }
                ]
            },
            {
                ProblemCode.RoadSegment.StartPoint.MeasureValueNotEqualToZero, [
                    new ProblemParameter("Identifier", "1"),
                    new ProblemParameter("PointX", "1.0"),
                    new ProblemParameter("PointY", "1.0"),
                    new ProblemParameter("Measure", "34"),
                    new ProblemParameter("Length", "234.4")
                ]
            },
            {
                ProblemCode.RoadSegment.EndPoint.MeasureValueNotEqualToLength, [
                    new ProblemParameter("Identifier", "1"),
                    new ProblemParameter("PointX", "1.0"),
                    new ProblemParameter("PointY", "1.0"),
                    new ProblemParameter("Measure", "34"),
                    new ProblemParameter("Length", "234.4")
                ]
            },
            {
                ProblemCode.RoadSegment.Point.MeasureValueDoesNotIncrease, [
                    new ProblemParameter("Identifier", "1"),
                    new ProblemParameter("PointX", "1.0"),
                    new ProblemParameter("PointY", "1.0"),
                    new ProblemParameter("Measure", "34"),
                    new ProblemParameter("PreviousMeasure", "34")
                ]
            },
            {
                ProblemCode.RoadSegment.Point.MeasureValueOutOfRange, [
                    new ProblemParameter("Identifier", "1"),
                    new ProblemParameter("PointX", "1.0"),
                    new ProblemParameter("PointY", "1.0"),
                    new ProblemParameter("Measure", "34"),
                    new ProblemParameter("MeasureLowerBoundary", "34"),
                    new ProblemParameter("MeasureUpperBoundary", "34")
                ]
            },
            {
                ProblemCode.RoadSegment.Geometry.LengthLessThanMinimum, [
                    new ProblemParameter("Identifier", "1"),
                    new ProblemParameter("Minimum", "1.0")
                ]
            },
            {
                ProblemCode.RoadSegment.Geometry.LengthTooLong, [
                    new ProblemParameter("Identifier", "1"),
                    new ProblemParameter("TooLongSegmentLength", "100000.0")
                ]
            },
            {
                ProblemCode.RoadSegment.Category.NotChangedBecauseCurrentIsNewerVersion, [
                    new ProblemParameter("Identifier", "1")
                ]
            },
            {
                ProblemCode.RoadSegment.StreetName.Left.NotProposedOrCurrent, [
                    new ProblemParameter("SegmentId", "1")
                ]
            },
            {
                ProblemCode.RoadSegment.StreetName.Left.NotFound, [
                    new ProblemParameter("SegmentId", "1")
                ]
            },
            {
                ProblemCode.RoadSegment.StreetName.Left.NotValid, [
                    new ProblemParameter("SegmentId", "1")
                ]
            },
            {
                ProblemCode.RoadSegment.StreetName.Right.NotProposedOrCurrent, [
                    new ProblemParameter("SegmentId", "1")
                ]
            },
            {
                ProblemCode.RoadSegment.StreetName.Right.NotFound, [
                    new ProblemParameter("SegmentId", "1")
                ]
            },
            {
                ProblemCode.RoadSegment.StreetName.Right.NotValid, [
                    new ProblemParameter("SegmentId", "1")
                ]
            },
        };

        var allValues = ProblemCode.GetValues();
        Assert.NotEmpty(allValues);

        var invalidProblemCodes = new List<ProblemCode>();

        foreach (var problemCode in allValues)
        {
            var parameters = defaultProblemParameters.ContainsKey(problemCode)
                ? defaultProblemParameters[problemCode]
                : new ProblemParameter[10].Select(_ => new ProblemParameter()).ToArray();

            var problem = new Problem
            {
                Severity = ProblemSeverity.Error,
                Reason = problemCode,
                Parameters = parameters
            };
            try
            {
                var problemTranslation = ProblemTranslator.Dutch(problem);
                if (problemTranslation.Message == ProblemTranslator.CreateMissingTranslationMessage(problemCode))
                {
                    invalidProblemCodes.Add(problemCode);
                }
            }
            catch(Exception ex)
            {
                _testOutputHelper.WriteLine($"Failed trying to translate problem {problemCode}: {ex.Message}");
                invalidProblemCodes.Add(problemCode);
            }
        }

        foreach (var problemCode in invalidProblemCodes.OrderBy(x => x.ToString()))
        {
            _testOutputHelper.WriteLine($"Missing translation: {problemCode}");
        }

        Assert.Empty(invalidProblemCodes);
    }

    private void LoadAllProblemCodes()
    {
        var problemCodeType = typeof(ProblemCode);
        var subClassTypes = problemCodeType.Assembly
            .GetTypes()
            .Where(x => x.IsNested && x.FullName!.StartsWith($"{problemCodeType}+"))
            .ToArray();

        foreach (var subClassType in subClassTypes)
        {
            var field = subClassType.GetFields().FirstOrDefault();
            if (field is not null)
            {
                field.GetValue(null);
            }
        }
    }
}
