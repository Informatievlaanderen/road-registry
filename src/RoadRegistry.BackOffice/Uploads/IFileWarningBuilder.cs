namespace RoadRegistry.BackOffice.Uploads
{
    using Core;

    public interface IFileWarningBuilder
    {
        IFileWarningBuilder WithParameter(ProblemParameter parameter);
        IFileWarningBuilder WithParameters(params ProblemParameter[] parameters);

        FileWarning Build();
    }
}
