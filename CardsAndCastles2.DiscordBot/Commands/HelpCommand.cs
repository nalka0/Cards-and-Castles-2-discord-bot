namespace CardsAndCastles2.DiscordBot.Commands;

using CardsAndCastles2.DiscordBot.Helpers;
using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class HelpCommand : IDiscordCommand
{
    private IEnumerable<IDiscordCommand> commands;

    public string Description => "Provides help about all the commands for the Cards and Castles 2 bot";

    public string Name => "help";

    public IEnumerable<DiscordCommandParameter> ParameterDefinitions { get; }

    public HelpCommand(IEnumerable<IDiscordCommand> commands)
    {
        this.commands = commands;
    }

    public Task<CommandResult> Execute(IReadOnlyDictionary<string, string> parameters)
    {
        return Task.FromResult(new CommandResult
        {
            Description = $"This bot is in beta, for anything (reporting bugs, getting help, suggesting improvements or new ideas, contributing...) DM or tag {MentionUtils.MentionUser(208199903241961472)}{Environment.NewLine}{FormatHelper.BulletList(commands.OrderBy(x => x.Name).Select(x => $"/{Format.Bold(x.Name)} : {x.Description}"))}",
            Title = $"Help for Cards and Castles 2 bot"
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
