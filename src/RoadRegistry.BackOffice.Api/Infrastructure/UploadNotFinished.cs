namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using FluentValidation;
    using FluentValidation.Results;

    public static class UploadNotFinished
    {
        public const string ErrorCode = "UploadNietAfgerond";

        public static ValidationException Exception()
        {
            return new ValidationException([
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = ErrorCode,
                    ErrorMessage = "Er is nog een upload bezig voor dit extract.",
                }
            ]);
        }
    }
}
