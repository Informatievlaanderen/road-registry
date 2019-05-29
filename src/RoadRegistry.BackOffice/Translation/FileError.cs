namespace RoadRegistry.BackOffice.Translation
{
    using Model;

    public class FileError : FileProblem
    {
        public FileError(string file, string reason, params ProblemParameter[] parameters)
            : base(file, reason, parameters)
        {
        }
    }
}
