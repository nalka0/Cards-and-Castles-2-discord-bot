namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot.Helpers;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal class DeckCommand : IDiscordCommand
{
    private readonly IDeckCodeParser deckCodeParser;

    public string Description => "Provides a deck's information based on its code (obtained via the export button in the game)";

    public string Name => "deck";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public DeckCommand(IDeckCodeParser deckCodeParser)
    {
        this.deckCodeParser = deckCodeParser;
        ParameterDefinitions = new DiscordCommandParameter[]
        {
            new()
            {
                Description = "The deck's code (obtained via the export button in the game)",
                Name = "code",
                IsRequired = true
            }
        };
    }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        string deckCode = parameters.Values.First();
        List<Card> deck = deckCodeParser.Parse(deckCode);
        return Task.FromResult(new CommandResult
        {
            Title = $"Deck {Format.Code(deckCode)}",
            Description = string.Join(Environment.NewLine, 
                $"Factions : {deck.Select(x => x.Faction).Distinct().Select(x => Emoji.Parse($":{x.ToString().ToLowerInvariant()}:"))}",
                $"Total shard cost : {deck.Sum(x => x.ShardsCost)}",
                FormatHelper.BulletList(deck.Distinct().OrderBy(x => x.Cost).ThenBy(x => x.Name).Select(x => $"{x.Name} x {deck.Count(y => y.Id == x.Id)}"))),
            Color = deck.Count == 30 ? Color.Green : Color.Red
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}