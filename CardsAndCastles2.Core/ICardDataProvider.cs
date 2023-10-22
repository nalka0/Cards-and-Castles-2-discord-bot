namespace CardsAndCastles2.Core;
public interface ICardDataProvider : IAsyncInitializable
{
    Dictionary<string, Card> CardsById { get; }

    IReadOnlyCollection<Card> Cards { get; }

    void RegisterDedicatedCommand(string commandName);
}