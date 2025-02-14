using Microsoft.Xna.Framework;

namespace SandSim.Simulation.Physics;

public readonly struct TraceStep
{
    private readonly Vector2 _hitNormal;
    
    public bool Valid { get; init; }
    public Point HitPoint { get; init; }
    public Point TracePoint { get; init; }
    public Vector2 HitNormal { get => _hitNormal;
        init
        {
            value.Normalize(); 
            _hitNormal = value;
        }
    }
}