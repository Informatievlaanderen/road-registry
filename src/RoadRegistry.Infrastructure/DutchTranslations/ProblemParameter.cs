namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;

public class ProblemParameter
{
    public string Name { get; set; }
    public string Value { get; set; }

    public ProblemParameter()
    {
    }

    public ProblemParameter(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
