namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class CardCommand : IAutocompleteDiscordCommand
{
    private const string CardNameParameterName = "cardname";

    private readonly ICardDataProvider cardDataProvider;

    public string Name => "card";

    public string Description => "Displays all infos on a given card";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public CardCommand(ICardDataProvider cardDataProvider)
    {
        this.cardDataProvider = cardDataProvider;
        ParameterDefinitions = new DiscordCommandParameter[]
        {
            new()
            {
                CanAutocomplete = true,
                Description = "The card's name",
                Name = CardNameParameterName,
                IsRequired = true
            },
        };
    }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        Card card = cardDataProvider.Cards.Single(x => x.Name.Equals(parameters.First().Value, StringComparison.InvariantCultureIgnoreCase));
        string description = card.ToString();
        if (!string.IsNullOrEmpty(card.DedicatedCommand))
        {
            description += $"{Environment.NewLine}{Environment.NewLine}For a simulation of this card's ability, use /{card.DedicatedCommand}";
        }

        return Task.FromResult(new CommandResult
        {
            Title = card.Name,
            Description = description,
            ImageUrl = $"https://github.com/nalka0/temp/blob/main/{card.Name.Replace(' ', '_')}.png?raw=true",
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public IEnumerable<string> AutoCompleteParameter(string parameterName, string value)
    {
        return parameterName switch
        {
            CardNameParameterName => cardDataProvider.Cards.Select(x => x.Name).Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x),
            _ => throw new ArgumentException($"Unknown parameter name for command '{Name}' : {parameterName}", nameof(parameterName))
        };
    }
}