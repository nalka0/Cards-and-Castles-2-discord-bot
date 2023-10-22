namespace CardsAndCastles2.DiscordBot;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot.Commands;

public interface IBot : IAsyncInitializable
{
    IDictionary<string, IDiscordCommand> Commands { get; }
}