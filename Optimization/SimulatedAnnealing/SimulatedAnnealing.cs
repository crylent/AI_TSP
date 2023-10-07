using System;
using AI_labs.Network;

namespace AI_labs.Optimization.SimulatedAnnealing;

public class SimulatedAnnealing: OptimizationAlgorithm
{
    public class Parameters
    {
        public float InitialTemperature = 100f;
        public float CoolingRate = 0.003f;
        public float TargetTemperature = 1f;

        public int Home;
    }

    private readonly Parameters _params;
    
    private Route _bestRoute;
    private float _temp;

    public SimulatedAnnealing(Network<int> network, Parameters @params) : base(network)
    {
        _params = @params;
        _bestRoute = Route.Random(network.Size + 1, @params.Home);
        _bestRoute.RecalculateLength(network);
        _temp = @params.InitialTemperature;
    }
    
    public override Route Optimize()
    {
        Start();
        return _bestRoute;
    }

    private void Start()
    {
        var route = _bestRoute.Copy;
        while (_temp > _params.TargetTemperature)
        {
            var (a, b) = (GetRandomNode(), GetRandomNode());
            var otherRoute = route.Copy;
            otherRoute.Swap(a, b, Network);
            route = SelectRoute(route, otherRoute);
            if (route.Length < _bestRoute.Length) _bestRoute = route;
            _temp *= 1 - _params.CoolingRate;
        }
    }

    private int GetRandomNode() => Random.Shared.Next(1, Network.Size);

    private Route SelectRoute(Route newRoute, Route oldRoute)
    {
        if (newRoute.Length < oldRoute.Length)
        {
            return newRoute;
        }
        var chance = Math.Exp((oldRoute.Length - newRoute.Length) / _temp);
        //Debug.WriteLine(chance);
        return Random.Shared.NextSingle() < chance ? newRoute : oldRoute;
    }
}