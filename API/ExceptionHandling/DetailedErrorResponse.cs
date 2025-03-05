namespace API.ExceptionHandling;

public sealed class DetailedErrorResponse : ErrorResponse
{
    public required string Reason { get; init; }
};

