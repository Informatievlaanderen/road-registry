namespace RoadRegistry.BackOffice.Uploads;

public static class ProjectionFormatFileProblems
{
    public static FileError ProjectionFormatInvalid(this IFileProblemBuilder builder)
    {
        return builder.Error(nameof(ProjectionFormatInvalid)).Build();
    }
}
