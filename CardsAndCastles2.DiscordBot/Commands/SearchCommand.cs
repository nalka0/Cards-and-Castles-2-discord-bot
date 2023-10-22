namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class SearchCommand : IAutocompleteDiscordCommand
{
    private const string NameParameter = "name";

    private const string KindParameter = "kind";

    private const string TypeParameter = "type";

    private const string CostParameter = "cost";

    private const string TextParameter = "text";

    private const string FactionParameter = "faction";

    private readonly ICardDataProvider cardData;

    public string Description => "Searches cards matching given parameters";

    public string Name => "search";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public SearchCommand(ICardDataProvider cardData)
    {
        this.cardData = cardData;
        ParameterDefinitions = new DiscordCommandParameter[]
        {
            new()
            {
                CanAutocomplete = true,
                Description = "The card's name",
                Name = NameParameter
            },
            new()
            {
                CanAutocomplete = true,
                Description = "The card kind",
                Name = KindParameter,
            },
            new()
            {
                Description = "The card's cost",
                Name = CostParameter
            },
            new()
            {
                CanAutocomplete = true,
                Description = "The card's type",
                Name = TypeParameter
            },
            new()
            {
                Description = "The card's text",
                Name = TextParameter
            },
            new()
            {
                CanAutocomplete = true,
                Description = "The card's faction",
                Name = FactionParameter
            }
        };
    }

    public IEnumerable<string> AutoCompleteParameter(string parameterName, string value)
    {
        return parameterName switch
        {
            KindParameter => new[] { nameof(Unit), nameof(Spell), nameof(Building) },
            FactionParameter => Enum.GetNames<Faction>(),
            TypeParameter => Enum.GetNames<CardType>().Select(x => x.Replace('_', ' ')),
            NameParameter => cardData.Cards.Select(x => x.Name).Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x),
            _ => Enumerable.Empty<string>()
        };
    }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        IEnumerable<Card> results = cardData.Cards;
        if (parameters.TryGetValue(CostParameter, out string parsableCost) && int.TryParse(parsableCost, out int cost))
        {
            results = results.Where(x => x.Cost == cost);
        }
        if (parameters.TryGetValue(TypeParameter, out string parsableType) && Enum.TryParse(parsableType.Replace(' ', '_'), out CardType type))
        {
            results = results.Where(x => x.Types.Contains(type));
        }
        if (parameters.TryGetValue(FactionParameter, out string parsableFaction) && Enum.TryParse(parsableFaction, out Faction faction))
        {
            results = results.Where(x => x.Faction == faction);
        }
        if (parameters.TryGetValue(TextParameter, out string text) && !string.IsNullOrWhiteSpace(text))
        {
            results = results.Where(x => x.Text.Contains(text, StringComparison.InvariantCultureIgnoreCase));
        }
        if (parameters.TryGetValue(NameParameter, out string name) && !string.IsNullOrWhiteSpace(name))
        {
            results = results.Where(x => x.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase));
        }
        if (parameters.TryGetValue(KindParameter, out string kind))
        {
            results = kind switch
            {
                nameof(Unit) => results.OfType<Unit>(),
                nameof(Spell) => results.OfType<Spell>(),
                nameof(Building) => results.OfType<Building>(),
                _ => results
            };
        }

        results = results.OrderBy(x => x.Name).ToList();
        return Task.FromResult(new CommandResult()
        {
            Title = $"{results.Count()} search results",
            Description = FormatHelper.BulletList(results.Select(x => x.Name))
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
