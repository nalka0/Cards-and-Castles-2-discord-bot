namespace CardsAndCastles2.DiscordBot.Commands;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class CompareDecksCommand : IDiscordCommand
{
    public string Description => "Compares two decks";

    public string Name => "compare_decks";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }
}
