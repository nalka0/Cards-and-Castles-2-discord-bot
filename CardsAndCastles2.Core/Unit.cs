namespace CardsAndCastles2.Core;

using System.Text;

public class Unit : Card
{
    public int Attack { get; set; }

    public int Health { get; set; }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"Kind : {GetType().Name}");
        builder.AppendLine($"Rarity : {Rarity}");
        builder.AppendLine($"Faction : {Faction}");
        if (Types.Count > 0)
        {
            builder.AppendLine($"Type(s) : {string.Join(", ", Types)}");
        }

        builder.AppendLine($"Cost : {Cost}");
        builder.AppendLine($"Attack : {Attack}");
        builder.AppendLine($"Health : {Health}");
        builder.AppendLine(Text);
        return builder.ToString();
    }
}