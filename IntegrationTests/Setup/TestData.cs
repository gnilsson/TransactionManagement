namespace IntegrationTests.Setup;

public sealed class TestData
{
    public required Guid[] AccountIds { get; init; }
    public required int AmountOfTransactions { get; init; }
}
