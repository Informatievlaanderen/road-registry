namespace RoadRegistry.Tests.BackOffice;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.DutchTranslations;
using RoadRegistry.BackOffice.Messages;

public class ProblemTranslatorTests
{
    [Fact]
    public void EnsureAllProblemCodeHaveADutchTranslation()
    {
        LoadAllProblemCodes();

        var defaultProblemParameters = new Dictionary<ProblemCode, ProblemParameter[]>
        {
            {
                ProblemCode.RoadNode.TypeMismatch, new[]
                {
                    new ProblemParameter { Name = "RoadNodeId", Value = "1" },
                    new ProblemParameter { Name = "ConnectedSegmentCount", Value = "1" },
                    new ProblemParameter { Name = "Actual", Value = RoadNodeType.EndNode.ToString() }
                }
            },
            {
                ProblemCode.StreetName.RegistryUnexpectedError, new[]
                {
                    new ProblemParameter { Name = "StatusCode", Value = "500" },
                }
            },
        };

        var allValues = ProblemCode.GetValues();
        Assert.NotEmpty(allValues);

        var problemCodesWithoutTranslation = new List<ProblemCode>();

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
            var problemTranslation = ProblemTranslator.Dutch(problem);
            if (problemTranslation.Message == ProblemTranslator.CreateMissingTranslationMessage(problemCode))
            {
                problemCodesWithoutTranslation.Add(problemCode);
            }
        }

        Assert.Empty(problemCodesWithoutTranslation);
    }

    private void LoadAllProblemCodes()
    {
        var problemCodeType = typeof(ProblemCode);
        var subClassTypes = problemCodeType.Assembly
            .GetTypes()
            .Where(x => x.IsNested && x.FullName.StartsWith($"{problemCodeType}+"))
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
