namespace ClinicAPI.Errors;

public static class PollErrors
{
    public static readonly Error PollNotFound =
        new("Poll.NotFound", "There is no poll with this ID", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedPollTitle =
        new("Poll.DuplicatedTitle", "Another poll with same title exists", StatusCodes.Status409Conflict);
}