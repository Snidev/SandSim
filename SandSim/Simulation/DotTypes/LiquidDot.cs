using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public abstract class LiquidDot(World world) : Dot(world)
{
    public abstract int FlowRate { get; }
    public abstract float FlowChance { get; }
    public override void Update(Point position)
    {
        // Apply normal gravity first
        Point dest = position + new Point(0, 1);
        if (World.IsOpen(dest))
        {
            World.SwapDots(position, dest);
            return;
        }

        // Water physics 
        dest = position;
        Point dir = World.Random.Next(0, 2) == 1 ? new Point(1, 0) : new Point(-1, 0);
        bool didFall = false;
        if (!World.IsOpen(dest + dir))
        {
            dir *= new Point(-1, 0);
            if (!World.IsOpen(dest + dir))
                return;
        }
        
        for (int i = 0; i < FlowRate; i++)
        {
            if (World.Random.NextSingle() >= FlowChance)
                break;

            if (World.IsOpen(dest + new Point(0, 1)))
            {
                if (didFall)
                    break;

                dest += new Point(0, 1);
                didFall = true;
                continue;
            }
            
            didFall = false;
                
            
            if (World.IsOpen(dest + dir))
                dest += dir;
        }

        World.SwapDots(position, dest);
    }
}