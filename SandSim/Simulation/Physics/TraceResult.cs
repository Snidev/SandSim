
using SandSim.Data;

namespace SandSim.Simulation.Physics;

public readonly struct TraceStep
{
    private readonly Vector _hitNormal;
    
    public bool Valid { get; init; }
    public Point HitPoint { get; init; }
    public Point TracePoint { get; init; }
    public Vector HitNormal { get => _hitNormal;
        init => _hitNormal = value.Normalized;
    }
}