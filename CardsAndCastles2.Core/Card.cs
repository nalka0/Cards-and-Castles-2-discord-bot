namespace CardsAndCastles2.Core;

using CardsAndCastles2;
using System;
using System.Collections.Generic;

public abstract class Card
{
    private static readonly CardType[] cardTypes = Enum.GetValues<CardType>();

    public string Id { get; private set; }

    public string Name { get; private set; }

    public uint Cost { get; private set; }

    public string Text { get; private set; }

    public List<CardType> Types { get; } = [];

    public Rarity Rarity { get; private set; }

    public int ShardsCost => Rarity switch
    {
        Rarity.Common => 40,
        Rarity.Rare => 100,
        Rarity.Epic => 400,
        Rarity.Legendary => 1600,
        _ => throw new NotSupportedException($"Unknown rarity : {Rarity}")
    };

    public Faction Faction { get; private set; }

    public string DedicatedCommand { get; set; }

    public abstract override string ToString();

    public static Card Create(IList<object> row, Faction faction)
    {
        Card ret;
        string cardCategory = row[3].ToString();
        if (cardCategory.Contains("Spell", StringComparison.InvariantCultureIgnoreCase))
        {
            ret = new Spell();
        }
        else if (cardCategory.Contains("Building", StringComparison.InvariantCultureIgnoreCase))
        {
            string[] stats = cardCategory.Split("/");
            ret = new Building()
            {
                Attack = int.Parse(stats[0].Split()[^1]),
                Health = int.Parse(stats[1])
            };
        }
        else
        {
            string[] stats = cardCategory.Split("/");
            ret = new Unit()
            {
                Attack = int.Parse(stats[0]),
                Health = int.Parse(stats[1])
            };
        }
        ret.Faction = faction;
        ret.Name = row[0].ToString();
        ret.Cost = uint.Parse(row[1].ToString());
        ret.Text = row[2].ToString();
        string types = row[4].ToString();
        foreach (CardType cardType in cardTypes)
        {
            string cardTypeText = cardType.ToString().Replace("_", " ");
            if (types.Contains(cardTypeText, StringComparison.InvariantCultureIgnoreCase))
            {
                ret.Types.Add(cardType);
                types = types.Replace(cardTypeText, string.Empty);
            }
        }

        types = types.Replace("-", string.Empty).Replace(",", string.Empty).Trim();
        if (!string.IsNullOrEmpty(types))
        {
            Console.WriteLine($"Unknown type : {types}");
        }

        ret.Rarity = Enum.Parse<Rarity>(row[5].ToString(), true);
        ret.Id = row[6].ToString();
        return ret;
    }
}
