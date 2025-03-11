namespace IntegrationTests.Setup;

public sealed class TestAnswers
{
    public required Guid UserId { get; init; }
    public required Guid[] AccountIds { get; init; }
    public required int AmountOfTransactions { get; init; }
}
