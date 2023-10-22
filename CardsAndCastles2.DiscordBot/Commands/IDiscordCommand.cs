namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.Core;

public interface IDiscordCommand : IAsyncInitializable
{
    string Description { get; }

    string Name { get; }

    IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters);
}
