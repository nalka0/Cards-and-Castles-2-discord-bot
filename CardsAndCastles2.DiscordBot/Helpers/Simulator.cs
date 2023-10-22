namespace CardsAndCastles2.DiscordBot.Helpers;

public abstract class Simulator<TNode> where TNode : notnull, IEquatable<TNode>
{
    public Dictionary<NodeSet<TNode>, int> ExecuteSerial(IReadOnlyList<TNode> initialNodes, int maxDepth)
    {
        Stack<IterationDepthState> depthStateStack = new(maxDepth);
        TNode[] currentNodes = [.. initialNodes];
        NodeSet<TNode>.Builder nodeSetBuilder = new();
        Dictionary<NodeSet<TNode>, int> results = [];
        int nodeIndex = 0;
        IReadOnlyList<TNode> nextNodes = null;
        int nextNodeIndex = 0;

        while (true)
        {
            if (nextNodes is null)
            {
                nextNodes = GetNextNodes(currentNodes[nodeIndex]);
                nextNodeIndex = 0;
            }

            if (nextNodeIndex >= nextNodes.Count)
            {
                goto NEXT_CURRENT_NODE;
            }

            TNode replacedNode = currentNodes[nodeIndex];
            currentNodes[nodeIndex] = nextNodes[nextNodeIndex];

            if (depthStateStack.Count == maxDepth - 1)
            {
                NodeSet<TNode> nodeSet = nodeSetBuilder.CreateFrom(currentNodes);

                if (!results.TryAdd(nodeSet, 1))
                {
                    results[nodeSet] += 1;
                    nodeSetBuilder.Return(nodeSet);
                }
            }
            else
            {
                depthStateStack.Push(new()
                {
                    NextNodeIndex = nextNodeIndex,
                    NextNodes = nextNodes,
                    NodeIndex = nodeIndex,
                    ReplacedNode = replacedNode
                });

                nodeIndex = 0;
                nextNodes = null;
                nextNodeIndex = 0;
                continue;
            }

            DEPTH_RETURN:
            currentNodes[nodeIndex] = replacedNode;

            ++nextNodeIndex;
            if (nextNodeIndex < nextNodes.Count)
            {
                continue;
            }

            NEXT_CURRENT_NODE:
            nextNodes = null;
            nextNodeIndex = 0;

            ++nodeIndex;
            if (nodeIndex < currentNodes.Length)
            {
                continue;
            }

            if (depthStateStack.TryPop(out IterationDepthState state))
            {
                nextNodeIndex = state.NextNodeIndex;
                nextNodes = state.NextNodes;
                nodeIndex = state.NodeIndex;
                replacedNode = state.ReplacedNode;
                goto DEPTH_RETURN;
            }
            else
            {
                break;
            }
        }

        return results;
    }

    protected abstract IReadOnlyList<TNode> GetNextNodes(TNode node);

    private readonly struct IterationDepthState
    {
        public required int NextNodeIndex { get; init; }

        public required IReadOnlyList<TNode> NextNodes { get; init; }

        public required int NodeIndex { get; init; }

        public required TNode ReplacedNode { get; init; }
    }
}

public sealed class EvolutionSimulator : Simulator<Node>
{
    protected override IReadOnlyList<Node> GetNextNodes(Node node) => node.NextNodes;
}
