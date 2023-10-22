namespace CardsAndCastles2.DiscordBot.Helpers;

public readonly struct NodeSet<TNode> : IEquatable<NodeSet<TNode>> where TNode : notnull, IEquatable<TNode>
{
    private NodeSet(TNode[] nodes)
    {
        Nodes = nodes;
        HashCode hashCode = new();
        foreach (TNode item in nodes)
        {
            hashCode.Add(item);
        }

        _hashCode = hashCode.ToHashCode();
    }

    public bool Equals(NodeSet<TNode> other)
    {
        if (_hashCode != other._hashCode || Nodes.Length != other.Nodes.Length)
        {
            return false;
        }

        for (int i = 0; i < Nodes.Length; ++i)
        {
            if (!Nodes[i].Equals(other.Nodes[i]))
            {
                return false;
            }
        }

        return true;
    }

    public TNode[] Nodes { get; }

    public override bool Equals(object obj) => obj is NodeSet<TNode> other && Equals(other);

    public static bool operator ==(NodeSet<TNode> left, NodeSet<TNode> right) => left.Equals(right);

    public static bool operator !=(NodeSet<TNode> left, NodeSet<TNode> right) => !(left == right);

    public override int GetHashCode() => _hashCode;

    private readonly int _hashCode;

    public class Builder
    {
        public Builder()
        {
            _nodesArrayPool = new();
        }

        public NodeSet<TNode> CreateFrom(ReadOnlySpan<TNode> nodes)
        {
            if (!_nodesArrayPool.TryPop(out TNode[] newNodes))
            {
                newNodes = new TNode[nodes.Length];
            }

            nodes.CopyTo(newNodes);
            return new(newNodes);
        }

        public void Return(NodeSet<TNode> nodeSet) => _nodesArrayPool.Push(nodeSet.Nodes);

        private readonly Stack<TNode[]> _nodesArrayPool;
    }
}