namespace RoadRegistry.BackOffice.Translation
{
    using Model;

    public interface IFileErrorBuilder
    {
        IFileErrorBuilder WithParameter(ProblemParameter parameter);
        IFileErrorBuilder WithParameters(params ProblemParameter[] parameters);
        FileError Build();
    }
}