namespace RoadRegistry.BackOffice.Translation
{
    using Model;

    public interface IFileWarningBuilder
    {
        IFileWarningBuilder WithParameter(ProblemParameter parameter);
        IFileWarningBuilder WithParameters(params ProblemParameter[] parameters);

        FileWarning Build();
    }
}