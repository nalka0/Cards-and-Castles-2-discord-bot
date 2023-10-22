namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

internal class DigForTreasureCommand : IDiscordCommand
{
    private static readonly string DeckSizeParameterName = "deck_size";

    private static readonly string StoppingCardsParameterName = "stopping_cards";

    private ICardDataProvider cardData;

    public string Description => "Provides dig for treasure's probabilities to draw X cards based on the state of your deck";

    public string Name => "dig";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public DigForTreasureCommand(ICardDataProvider cardDataProvider)
    {
        cardData = cardDataProvider;
        ParameterDefinitions = new DiscordCommandParameter[]
        {
            new()
            {
                IsRequired = true,
                Name = DeckSizeParameterName,
                Description = "The size of your deck"
            },
            new()
            {
                IsRequired = true,
                Name = StoppingCardsParameterName,
                Description = "The number of cards in your deck that cost 5 or more (= that'd stop dig)"
            }
        };
    }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        if (uint.TryParse(parameters[DeckSizeParameterName], out uint deckSize) && uint.TryParse(parameters[StoppingCardsParameterName], out uint stoppingCards))
        {
            Dictionary<int, double> results = [];
            int i = 2;
            AddResult(1, 1);
            while (deckSize - i >= stoppingCards - 1)
            {
                AddResult(i, 1 - (double)stoppingCards / (deckSize - i + 2));
                i++;
            }

            return Task.FromResult(new CommandResult()
            {
                Title = $"Dig probabilities for a {deckSize} cards deck containing {stoppingCards} cards costing 5+",
                Description = FormatHelper.BulletList(results.Select(x => $"{x.Key} => {x.Value.ToString("P", CultureInfo.InvariantCulture)}"))
            });

            void AddResult(int i, double continuationProbability)
            {
                if (results.Count > 0)
                {
                    results.Add(i, results[i - 1] * continuationProbability);
                }
                else
                {
                    results.Add(i, continuationProbability);
                }
            }
        }
        else
        {
            throw new ArgumentException("Required parameters are invalid");
        }
    }

    public Task InitializeAsync()
    {
        cardData.RegisterDedicatedCommand("Dig for Treasure");
        return Task.CompletedTask;
    }
}
