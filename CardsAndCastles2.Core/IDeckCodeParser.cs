namespace CardsAndCastles2.Core;

public interface IDeckCodeParser
{
    List<Card> Parse(string deckCode);
}