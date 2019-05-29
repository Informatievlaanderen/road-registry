namespace RoadRegistry.BackOffice.Translation
{
    using Model;

    public class FileWarning : FileProblem
    {
        public FileWarning(string file, string reason, params ProblemParameter[] parameters)
            : base(file, reason, parameters)
        {
        }
    }
}