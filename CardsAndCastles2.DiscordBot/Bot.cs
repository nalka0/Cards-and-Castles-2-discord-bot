namespace CardsAndCastles2.DiscordBot;

using CardsAndCastles2.DiscordBot.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Bot : IBot
{
    private static Emoji rightArrow = Emoji.Parse(":arrow_right:");

    private static Emoji leftArrow = Emoji.Parse(":arrow_left:");

    private static readonly char buttonDataSplitter = '|';

    private int latestPageCacheId;

    private DiscordSocketClient client;

    private Dictionary<int, EmbedBuilder[]> cachedPages;

    public IDictionary<string, IDiscordCommand> Commands { get; }

    public Bot(IEnumerable<IDiscordCommand> commands)
    {
        cachedPages = [];
        Commands = new Dictionary<string, IDiscordCommand>();
        foreach (IDiscordCommand command in commands)
        {
            Commands[command.Name] = command;
        }
    }

    public async Task InitializeAsync()
    {
        client = new(new DiscordSocketConfig());
        client.Ready += ClientReady;
        client.SlashCommandExecuted += SlashCommandHandler;
        client.AutocompleteExecuted += AutoComplete;
        client.ButtonExecuted += ButtonExecuted;
        await client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["DiscordBotToken"]).ContinueWith(_ => client.StartAsync());
    }

    private async Task ButtonExecuted(SocketMessageComponent arg)
    {
        try
        {
            string[] data = arg.Data.CustomId.Split('|');
            int cacheId = int.Parse(data[0]);
            EmbedBuilder[] cache = cachedPages[cacheId];
            int page = int.Parse(data[1]);
            await arg.UpdateAsync(x =>
            {
                x.Embed = cache[page].Build();
                x.Components = new ComponentBuilder()
                /*:arrow_left:*/  .WithButton("Previous page", $"{cacheId}{buttonDataSplitter}{page - 1}", disabled: page <= 0, emote: leftArrow)
                /*:arrow_right:*/ .WithButton("Next page", $"{cacheId}{buttonDataSplitter}{page + 1}", disabled: cache.Length <= page + 1, emote: rightArrow)
                                  .Build();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task AutoComplete(SocketAutocompleteInteraction arg)
    {
        try
        {
            if (Commands.TryGetValue(arg.Data.CommandName, out IDiscordCommand command) && command is IAutocompleteDiscordCommand autocompleteCommand)
            {
                await arg.RespondAsync(autocompleteCommand.AutoCompleteParameter(arg.Data.Current.Name, arg.Data.Current.Value.ToString()).Take(SlashCommandOptionBuilder.MaxChoiceCount).Select(x => new AutocompleteResult
                {
                    Name = x,
                    Value = x
                }));
            }
            else
            {
                await arg.DeleteOriginalResponseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand slashCommand)
    {
        try
        {
            if (Commands.TryGetValue(slashCommand.CommandName, out IDiscordCommand command))
            {
                Task deferedResponse = slashCommand.DeferAsync();
                Dictionary<string, string> parameters = slashCommand.Data.Options.ToDictionary(x => x.Name, x => x.Value.ToString());
                CommandResult result = await command.Execute(parameters);
                await deferedResponse;
                if (result.Description.Length > EmbedBuilder.MaxDescriptionLength)
                {
                    int id = latestPageCacheId++;
                    EmbedBuilder[] pages = new EmbedBuilder[result.Description.Length / EmbedBuilder.MaxDescriptionLength + 1];
                    int currentPage = 0;
                    pages[0] = new EmbedBuilder()
                    {
                        Title = $"{result.Title}{Environment.NewLine}Page 1/{pages.Length}",
                        Color = result.Color ?? Color.Green
                    };
                    StringBuilder builder = new();
                    foreach (string line in result.Description.Split(Environment.NewLine))
                    {
                        if (builder.Length + Environment.NewLine.Length + line.Length <= EmbedBuilder.MaxDescriptionLength)
                        {
                            builder.AppendLine(line);
                        }
                        else
                        {
                            pages[currentPage].Description = builder.ToString();
                            currentPage++;
                            pages[currentPage] = new()
                            {
                                Color = result.Color ?? Color.Green,
                                Title = $"{result.Title}{Environment.NewLine}Page {currentPage + 1}/{pages.Length}"
                            };
                            builder.Clear();
                        }
                    }

                    pages[currentPage].Description = builder.ToString();
                    cachedPages[id] = pages;
                    await slashCommand.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = pages[0].Build();
                        x.Components = new ComponentBuilder()
                          /*:arrow_left:*/  .WithButton("Previous page", $"{id}{buttonDataSplitter}-1", disabled: true, emote: leftArrow)
                          /*:arrow_right:*/ .WithButton("Next page", $"{id}{buttonDataSplitter}1", emote: rightArrow)
                                            .Build();
                    });
                }
                else
                {
                    await slashCommand.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = new EmbedBuilder()
                        {
                            Description = result.Description,
                            Title = result.Title,
                            Color = result.Color ?? Color.Green,
                            ImageUrl = result.ImageUrl
                        }.Build();
                    });
                }
            }
            else
            {
                await slashCommand.RespondAsync("The command you used doesn't exist, if this happens repeatedly ask nalka", ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            await slashCommand.DeleteOriginalResponseAsync();
            await slashCommand.RespondAsync("The command you used doesn't exist, if this happens repeatedly ask nalka", ephemeral: true);
            Console.WriteLine(ex);
        }
    }

    public async Task ClientReady()
    {
        try
        {
            List<SlashCommandProperties> builtCommands = [];
            client.Ready -= ClientReady;
            await client.SetActivityAsync(new Game("/help or @nalka"));
            foreach (IDiscordCommand command in Commands.Values)
            {
                SlashCommandBuilder slashCommand = new SlashCommandBuilder().WithName(command.Name).WithDescription(command.Description);
                foreach (DiscordCommandParameter parameterDefinition in command.ParameterDefinitions ?? [])
                {
                    slashCommand.AddOption(new()
                    {
                        Name = parameterDefinition.Name,
                        Type = ApplicationCommandOptionType.String,
                        Description = parameterDefinition.Description,
                        IsRequired = parameterDefinition.IsRequired,
                        ChannelTypes = [ChannelType.Text],
                        IsAutocomplete = parameterDefinition.CanAutocomplete
                    });
                }

                builtCommands.Add(slashCommand.Build());
            }
#if DEBUG
            await client.Guilds.Single(x => x.Id == ulong.Parse(ConfigurationManager.AppSettings["TestServerId"])).BulkOverwriteApplicationCommandAsync(builtCommands.ToArray());
#else
            await client.BulkOverwriteGlobalApplicationCommandsAsync(builtCommands.ToArray());
#endif
            await Console.Out.WriteLineAsync("Bot ready");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}