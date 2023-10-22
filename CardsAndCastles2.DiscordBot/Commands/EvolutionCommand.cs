namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot;
using CardsAndCastles2.DiscordBot.Helpers;
using Discord;

internal class EvolutionCommand : IAutocompleteDiscordCommand
{
    private readonly EvolutionSimulator simulator;

    private readonly ICardDataProvider cardDataProvider;

    private Node[] nodes;

    public string Name => "evolution";

    public string Description => "Performs a simulation for the Evolution card and provides possible outcomes with their probability";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public EvolutionCommand(ICardDataProvider cardDataProvider)
    {
        simulator = new();
        this.cardDataProvider = cardDataProvider;
        DiscordCommandParameter[] parameterDefinitions = new DiscordCommandParameter[Math.Min(DiscordConstants.MaximumCommandParameters, GameConstants.MaxUnnocupiedTiles)];
        for (int i = 1; i <= parameterDefinitions.Length; i++)
        {
            parameterDefinitions[i - 1] = new DiscordCommandParameter
            {
                Name = $"unit{i}",
                CanAutocomplete = true,
                IsRequired = i == 1,
                Description = "Your unit's name (evolvable units only)"
            };
        }

        ParameterDefinitions = parameterDefinitions;
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            cardDataProvider.RegisterDedicatedCommand(Name);
            List<Node> evolutionNodes = cardDataProvider.Cards.OfType<Unit>().Where(x => x.Types?.Count > 0).Select(x => new Node(x)).ToList();
            foreach (Node node in evolutionNodes)
            {
                if (node.Unit.Name.Equals("stone drake", StringComparison.InvariantCultureIgnoreCase))
                {
                    // No next node for stone drake because it's inert
                    node.NextNodes = [];
                }
                else
                {
                    node.NextNodes = evolutionNodes.Where(x => x.Unit.Cost == node.Unit.Cost + 1 && x.Unit.Types.Any(t => node.Unit.Types.Contains(t))).ToList();
                }
            }

            nodes = [.. evolutionNodes];
        });
    }

    public async Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        return await Task.Run(() =>
        {
            Node[] startNodes = parameters.Select(y => nodes.Single(x => x.Unit.Name.Contains(y.Value, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            int depth = 8;
            Dictionary<NodeSet<Node>, int> results = simulator.ExecuteSerial(startNodes, depth);
            while (depth > 1)
            {
                depth--;
                foreach (KeyValuePair<NodeSet<Node>, int> lowerResult in simulator.ExecuteSerial(startNodes, depth))
                {
                    if (lowerResult.Key.Nodes.All(x => x.NextNodes.Count == 0))
                    {
                        results.Add(lowerResult.Key, lowerResult.Value);
                    }
                }
            }

            double total = results.Values.Sum(x => x);
            string baseTitle = $"Evolution simulation results for {Format.Code(string.Join(" | ", startNodes.Select(x => x.Unit.Name)))}{Environment.NewLine}{total} possibilities leading to {results.Count} different outcomes";
            string baseDescription = FormatHelper.BulletList(results.OrderByDescending(x => x.Value).Select(r => Format.Code(string.Join(" | ", r.Key.Nodes.Select(x => $"{x.Unit.Name}")))));
            return new CommandResult()
            {
                Title = baseTitle,
                Description = baseDescription
            };
        });
    }

    public IEnumerable<string> AutoCompleteParameter(string parameterName, string value)
    {
        return nodes.Where(x => x.NextNodes.Count > 0 && x.Unit.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Unit.Name).OrderBy(x => x);
    }
}
