namespace API.ExceptionHandling;

public class ErrorResponse
{
    public required string StatusMessage { get; init; }
    public required string Information { get; init; }
}
