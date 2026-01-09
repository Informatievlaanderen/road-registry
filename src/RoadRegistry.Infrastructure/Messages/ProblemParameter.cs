namespace RoadRegistry.Infrastructure.Messages;

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
