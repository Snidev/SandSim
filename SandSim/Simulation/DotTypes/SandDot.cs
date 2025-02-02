using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public class SandDot : Dot
{
    public override Color Color { get; }
    public override void Update(Point position)
    {
        Span<Point> targets = [position + new Point(0, 1), position + new Point(1, 1), position + new Point(-1, 1)];
        if (World.Random.Next(2) == 1)
            (targets[1], targets[2]) = (targets[2], targets[1]);
        
        foreach (Point t in targets)
        {
            if (!World.IsInBounds(t)) 
                continue;

            Dot? tDot = World.GetDot(t);
            
            if (tDot is LiquidDot)
            {
                Span<Point> tDst = [t + new Point(1, 0), t - new Point(1, 0)];
                if (World.Random.Next(2) == 1)
                    (tDst[0], tDst[1]) = (tDst[1], tDst[0]);

                foreach (Point dst in tDst)
                {
                    if (World.IsOpen(dst))
                    {
                        World.SwapDots(t, dst);
                        return;
                    }
                }
                
                World.SwapDots(position, t);
                return;
            }
            
            if (tDot is not null)
                continue;
            
            World.MoveDot(position, t);   
            return;
        }
    }

    public SandDot(World world) : base(world)
    {
        Span<Color> colors = [new Color(204, 204, 0), new Color(230, 230, 0), new Color(179, 179, 0)];
        Color = colors[World.Random.Next(0, colors.Length)];
    }
}