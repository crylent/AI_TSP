using AI_labs.Network;

namespace AI_labs.Optimization;

public abstract class OptimizationAlgorithm
{
    protected readonly Network<int> Network;

    protected OptimizationAlgorithm(Network<int> network)
    {
        Network = network;
    }
    
    public abstract Route Optimize();
}