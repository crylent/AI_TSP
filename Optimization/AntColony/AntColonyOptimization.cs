using System.Collections.Generic;
using System.Linq;
using AI_labs.Network;

namespace AI_labs.Optimization.AntColony;

public class AntColonyOptimization: OptimizationAlgorithm
{
    private PheromoneNetwork _pheromone = null!;

    public class Parameters
    {
        public float Alpha = 1f;
        public float Beta = 3f;
        public float Evaporation = 0.5f;
        public float PheromoneGain = 100f;
        public int AntsNumber = 20;
        public int ColonyLifetime = 100;

        public int Home; // start point
    }

    private readonly Parameters _params;

    private List<Ant> _ants = null!;

    public AntColonyOptimization(Network<int> network, Parameters @params) : base(network)
    {
        _params = @params;
    }

    public override Route Optimize()
    {
        Init();
        Start();
        return Ant.BestRoute!;
    }

    private void Init()
    {
        _pheromone = new PheromoneNetwork(Network.Size, _params.Evaporation);
        _ants = Enumerable.Repeat(new Ant(_params, Network, _pheromone), _params.AntsNumber).ToList();
        Ant.ResetBestRoute();
    }

    private void Start()
    {
        for (var t = 0; t < _params.ColonyLifetime; t++)
        {
            foreach (var ant in _ants)
            {
                ant.GoRoute();
                ant.EmitPheromone();
                ant.Reset();
            }
            _pheromone.Evaporate();
        }
    }
}