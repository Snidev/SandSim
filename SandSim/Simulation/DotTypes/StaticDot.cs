using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public abstract class StaticDot(World world) : Dot(world)
{
    public override void Update(Point position)
    {
    }
}