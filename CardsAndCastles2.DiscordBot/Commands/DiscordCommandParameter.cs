namespace CardsAndCastles2.DiscordBot.Commands;
public class DiscordCommandParameter
{
    public string Name { get; init; }

    public bool CanAutocomplete { get; init; }

    public bool IsRequired { get; init; }

    public string Description { get; init; }
}