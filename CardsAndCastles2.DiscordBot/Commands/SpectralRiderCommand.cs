namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class SpectralRiderCommand : IDiscordCommand
{
    private readonly ICardDataProvider cardData;

    private Dictionary<uint, string> results;

    public string Description => "Lists units that can be summoned by a spectral rider given its attack";

    public string Name => "spectral_rider";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public SpectralRiderCommand(ICardDataProvider cardData)
    {
        this.cardData = cardData;
        ParameterDefinitions = new DiscordCommandParameter[]
        {
            new()
            {
                CanAutocomplete = false,
                Description = "The spectral rider's attack",
                IsRequired = true,
                Name = "attack"
            }
        };
    }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        uint riderAttack = uint.Parse(parameters.Values.First());
        string result;
        while (!results.TryGetValue(riderAttack, out result) && riderAttack > 0)
        {
            riderAttack--;
        }

        return Task.FromResult(new CommandResult
        {
            Title = $"Units summoned by a spectral rider with {riderAttack} attack",
            Description = riderAttack != 0 ? result : "nothing"
        });
    }

    public async Task InitializeAsync()
    {
        cardData.RegisterDedicatedCommand(Name);
        await Task.Run(() =>
        {
            results = cardData.Cards.OfType<Unit>().Where(x => x.Faction == Faction.Undead && x.Cost > 0).GroupBy(x => x.Cost).ToDictionary(x => x.Key, x => FormatHelper.BulletList(x.Select(x => x.Name).OrderBy(x => x)));
        });
    }
}