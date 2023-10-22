namespace CardsAndCastles2.DiscordBot.Commands;
public interface IAutocompleteDiscordCommand : IDiscordCommand
{
    IEnumerable<string> AutoCompleteParameter(string parameterName, string value);
}