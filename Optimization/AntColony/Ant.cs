using System;
using System.Collections.Generic;
using System.Linq;
using AI_labs.Network;

namespace AI_labs.Optimization.AntColony;

public class Ant
{
    private readonly AntColonyOptimization.Parameters _params;
    private readonly Network<int> _network;
    private readonly PheromoneNetwork _pheromones;
    
    private readonly Route _route;
    public static Route? BestRoute
    {
        get;
        private set;
    }

    public Ant(AntColonyOptimization.Parameters @params, Network<int> network, PheromoneNetwork pheromones)
    {
        _params = @params;
        _network = network;
        _pheromones = pheromones;
        _route = new Route(network.Size + 1,  @params.Home);
    }

    public void EmitPheromone()
    {
        var gain = _params.PheromoneGain / _route.Length;
        for (var i = 0; i < _route.Count - 1; i++)
        {
            _pheromones.AddPheromone(_route[i], _route[i + 1], gain);
        }
    }

    public void Reset()
    {
        _route.Reset();
    }

    public void GoRoute()
    {
        for (var i = 0; i < _network.Size - 1; i++)
        {
            AntGoNextNode();
        }
        AntReturnHome();
        if (BestRoute is null || _route.Length < BestRoute.Length)
        {
            BestRoute = _route.Copy; // save best route
        }
    }

    private void AntGoNextNode()
    {
        var next = SelectNextNode();
        _route.Add(next, GetLength(_route.Last(), next));
    }

    private void AntReturnHome()
    {
        _route.Add(_params.Home, GetLength(_route.Last(), _params.Home));
    }

    private int SelectNextNode()
    {
        var probs = new List<float>(_network.Size);
        for (var i = 0; i < _network.Size; i++)
        {
            var prob = CalculateProbability(i);
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

    private float CalculateProbability(int next)
    {
        if (_route.Contains(next)) return 0; // can't move to already visited nodes
        var current = _route.Last();
        var length = GetLength(current, next);
        var pheromone = _pheromones.GetValue(current, next);
        return MathF.Pow(pheromone, _params.Alpha) * MathF.Pow(1f / length, _params.Beta);
    }

    private int GetLength(int a, int b)
    {
        var length = _network.GetValue(a, b);
        return length != Network<int>.NoPath ? length : 999999; // replace NoPath with big number to avoid dead ends
    }

    public static void ResetBestRoute()
    {
        BestRoute = null;
    }
}