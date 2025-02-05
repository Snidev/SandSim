using Microsoft.Xna.Framework;
using SandSim.Simulation.Physics;

namespace SandSim.Simulation.DotTypes;

public class GasDot(World world) : Dot(world)
{
    public override Color Color => Color.DeepPink;

    public override void Update(Point position)
    {
        Span<Point> points =
        [
            position + new Point(1, 0),
            position + new Point(0, 1),
            position + new Point(-1, 0),
            position + new Point(0, -1),
        ];
        world.Random.Shuffle(points);
        
        foreach (Point point in points)
            if (world.IsOpen(point))
                world.SwapDots(position, point);
    }
}