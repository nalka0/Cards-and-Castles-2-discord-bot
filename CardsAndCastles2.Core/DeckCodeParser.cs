namespace CardsAndCastles2.Core;
using System;
using System.Collections.Generic;
using System.Linq;

internal class DeckCodeParser : IDeckCodeParser
{
    private readonly ICardDataProvider cardData;

    public DeckCodeParser(ICardDataProvider cardDataProvider)
    {
        cardData = cardDataProvider;
    }

    public List<Card> Parse(string deckCode)
    {
        int singleCopyNumber = GetNumberOfCards(int.Parse(deckCode[0].ToString()), deckCode[1]);
        List<Card> singleCopy = deckCode[2..(2 + singleCopyNumber * 2)].Chunk(2).Select(x => new string(x.ToArray())).Select(x => cardData.CardsById[x]).ToList();
        List<Card> duplicates = singleCopy.GroupBy(x => x).Where(x => x.Count() == 2).Select(x => x.First()).ToList();
        int twoCopiesNumber = GetNumberOfCards(int.Parse(deckCode[(singleCopyNumber + 1) * 2].ToString()), deckCode[(singleCopyNumber + 1) * 2 + 1]);
        List<Card> twoCopies = deckCode[(4 + singleCopyNumber * 2)..^2].Chunk(2).Select(x => new string(x.ToArray())).Select(x => cardData.CardsById[x]).Concat(duplicates).ToList();

        List<Card> ret = singleCopy.Except(duplicates).ToList();
        ret.AddRange(twoCopies);
        ret.AddRange(twoCopies);
        return ret;

        static int GetNumberOfCards(int first, char second)
        {
            int secondScore;
            if (char.IsDigit(second))
            {
                secondScore = (second - '1') / 2;
            }
            else
            {
                secondScore = 5 + (second - 'B') / 2;
            }
            return (first - 1) * 17 + secondScore;
        }
    }
}