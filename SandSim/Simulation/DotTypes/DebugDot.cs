using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public class DebugDot(World world) : StaticDot(world)
{
    public override Color Color => DotColor;
    public Color DotColor = Color.Orange;
}