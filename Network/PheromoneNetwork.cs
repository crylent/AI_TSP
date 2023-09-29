namespace AI_labs.Network;

public class PheromoneNetwork: MutableNetwork<float>
{
    private readonly float _evaporation;
    
    public PheromoneNetwork(int networkSize, float evaporation) : base(networkSize, 1f)
    {
        _evaporation = evaporation;
    }

    public void Evaporate()
    {
        ForEachPath((i, j) => Matrix[i][j] *= 1f - _evaporation);
    }

    public void AddPheromone(int nodeA, int nodeB, float gain)
    {
        var (node1, node2) = OrderIndices(nodeA, nodeB);
        Matrix[node1][node2] += gain;
    }
}