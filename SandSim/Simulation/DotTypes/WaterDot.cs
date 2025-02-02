using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public class WaterDot(World world) : LiquidDot(world)
{
    public override Color Color => Color.Aqua;
    public override int FlowRate => 2;
    public override float FlowChance => 1;
}