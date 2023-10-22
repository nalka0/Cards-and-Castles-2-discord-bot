namespace CardsAndCastles2.DiscordBot.Helpers;
using System.Collections.Generic;
using System.Text;

public static class FormatHelper
{
    public static string BulletList(IEnumerable<string> list)
    {
        StringBuilder builder = new();
        foreach (string item in list)
        {
            builder.AppendLine($"- {item}");
        }

        return builder.ToString();
    }
}
