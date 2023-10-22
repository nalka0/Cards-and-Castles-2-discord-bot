namespace CardsAndCastles2.Core;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Collections.Generic;
using System.Configuration;

internal class CookieCardDatabase : ICardDataProvider
{
    public Dictionary<string, Card> CardsById { get; set; }

    public IReadOnlyCollection<Card> Cards => CardsById.Values;

    public async Task InitializeAsync()
    {
        SheetsService service = new(new BaseClientService.Initializer()
        {
            ApiKey = ConfigurationManager.AppSettings["SheetsApiKey"]
        });
        Dictionary<string, Card> cards = [];
        foreach (Faction faction in Enum.GetValues<Faction>())
        {
            foreach (IList<object> row in (await service.Spreadsheets.Values.Get(ConfigurationManager.AppSettings["SpreadsheetId"], $"{faction}!A:G").ExecuteAsync()).Values.Skip(1))
            {
                Card card = Card.Create(row, faction);
                cards.Add(card.Id, card);
            }
        }

        CardsById = cards;
    }

    public void RegisterDedicatedCommand(string commandName)
    {
        Cards.Single(x => x.Name.Equals(commandName.Replace('_', ' '), StringComparison.InvariantCultureIgnoreCase)).DedicatedCommand = commandName;
    }
}
