using System;
using System.Collections.Generic;
using System.Linq;

namespace AI_labs;

public class AntColonyOptimization: IOptimizationAlgorithm
{
    private readonly Network<int> _network;
    private PheromoneNetwork _pheromone = null!;

    public float Alpha = 1f;
    public float Beta = 3f;
    public float Evaporation = 0.5f;
    public float PheromoneGain = 100f;
    public int AntsNumber = 20;
    public int ColonyLifetime = 100;

    public int Home; // start point

    public class Ant
    {
        public int RouteLength;
        public List<int> Route = new();

        public int CurrentNode => Route.Last();

        public Ant Copy() => new()
        {
            Route = new List<int>(Route), 
            RouteLength = RouteLength
        };
    }

    private List<Ant> _ants = null!;
    private Ant _bestRoute = null!;

    public AntColonyOptimization(Network<int> network)
    {
        _network = network;
    }

    public Ant Optimize()
    {
        Init();
        Start();
        return _bestRoute;
    }

    private void Init()
    {
        _pheromone = new PheromoneNetwork(_network.Size, Evaporation);
        _ants = Enumerable.Repeat(new Ant
        {
            Route = new List<int>(_network.Size) { Home }
        }, AntsNumber).ToList();
    }

    private void Start()
    {
        for (var t = 0; t < ColonyLifetime; t++)
        {
            foreach (var ant in _ants)
            {
                AntGoRoute(ant);
                EmitPheromone(ant);
                AntReset(ant);
            }
            _pheromone.Evaporate();
        }
    }

    private void EmitPheromone(Ant ant)
    {
        var gain = PheromoneGain / ant.RouteLength;
        for (var i = 0; i < ant.Route.Count - 1; i++)
        {
            _pheromone.AddPheromone(ant.Route[i], ant.Route[i + 1], gain);
        }
    }

    private void AntReset(Ant ant)
    {
        ant.Route.Clear();
        ant.Route.Add(Home);
        ant.RouteLength = 0;
    }

    private void AntGoRoute(Ant ant)
    {
        for (var i = 0; i < _network.Size - 1; i++)
        {
            AntGoNextNode(ant);
        }
        AntReturnHome(ant);
        if (_bestRoute == null! || ant.RouteLength < _bestRoute.RouteLength)
        {
            _bestRoute = ant.Copy(); // save best route
        }
    }

    private void AntGoNextNode(Ant ant)
    {
        var next = SelectNextNode(ant);
        ant.RouteLength += GetLength(ant.CurrentNode, next);
        ant.Route.Add(next);
    }

    private void AntReturnHome(Ant ant)
    {
        ant.RouteLength += GetLength(ant.CurrentNode, Home);
        ant.Route.Add(Home);
    }

    private int SelectNextNode(Ant ant)
    {
        var probs = new List<float>(_network.Size);
        for (var i = 0; i < _network.Size; i++)
        {
            var prob = CalculateProbability(ant, i);
            probs.Add(prob);
        }
        var rand = Random.Shared.NextSingle() * probs.Sum();
        var sum = 0f;
        var next = -1;
        while (sum < rand)
        {
            next++;
            sum += probs[next];
        }
        return next;
    }

    private float CalculateProbability(Ant ant, int next)
    {
        if (ant.Route.Contains(next)) return 0; // can't move to already visited nodes
        var length = GetLength(ant.CurrentNode, next);
        var pheromone = _pheromone.GetLength(ant.CurrentNode, next);
        return MathF.Pow(pheromone, Alpha) * MathF.Pow(1f / length, Beta);
    }

    private int GetLength(int a, int b)
    {
        var length = _network.GetLength(a, b);
        return length != Network<int>.NoPath ? length : 999999; // replace NoPath with big number to avoid dead ends
    }
}