namespace CardsAndCastles2.DiscordBot.Helpers;

using CardsAndCastles2.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("Design", "CA1067:Override Object.Equals(object) when implementing IEquatable<T>", Justification = "It uses ReferenceEquals, just like object.Equals does")]
public class Node : IEquatable<Node>
{
    public Node(Unit unit)
    {
        Unit = unit;
    }

    public Unit Unit { get; }

    public List<Node> NextNodes { get; set; }

    public bool Equals(Node other)
    {
        return ReferenceEquals(this, other);
    }

    public override string ToString()
    {
        return Unit.ToString();
    }
}
