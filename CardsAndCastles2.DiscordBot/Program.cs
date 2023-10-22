namespace CardsAndCastles2.DiscordBot;

using CardsAndCastles2.Core;
using CardsAndCastles2.DiscordBot.Commands;
using Unity;

public class Program
{
    public static async Task Main()
    {
        Dependency.Container.RegisterSingleton<IDiscordCommand, EvolutionCommand>("a");
        Dependency.Container.RegisterSingleton<IDiscordCommand, SpectralRiderCommand>("ab");
        Dependency.Container.RegisterSingleton<IDiscordCommand, HelpCommand>("abc");
        Dependency.Container.RegisterSingleton<IDiscordCommand, CardCommand>("abcd");
        Dependency.Container.RegisterSingleton<IDiscordCommand, SearchCommand>("abcde");
        Dependency.Container.RegisterSingleton<IDiscordCommand, DeckCommand>("abcdef");
        Dependency.Container.RegisterSingleton<IDiscordCommand, DigForTreasureCommand>("abcdefg");
        Dependency.Container.RegisterSingleton<IBot, Bot>();

        // Waiting for better async initialization methods...
        ICardDataProvider cardService = Dependency.Container.Resolve<ICardDataProvider>();
        IEnumerable<IDiscordCommand> commands = Dependency.Container.ResolveAll<IDiscordCommand>();
        IBot discordBot = Dependency.Container.Resolve<IBot>();
        await cardService.InitializeAsync();
        await Task.WhenAll(commands.Select(x => x.InitializeAsync()));
        await discordBot.InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }
}