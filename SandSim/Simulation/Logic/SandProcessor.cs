namespace SandSim.Simulation.Logic;

public class SandProcessor : DotProcessor 
{
    public override void Update(World world, int x, int y, int dotType)
    {
        Span<(int, int)> targets = [(x, y + 1), (x + 1, y + 1), (x - 1, y + 1)];
        if (world.Rng.Next(0, 2) == 1)
            (targets[1], targets[2]) = (targets[2], targets[1]);

        foreach ((int tX, int tY) in targets)
        {
            if (world.IsInBounds(tX, tY) && world.IsEmpty(tX, tY))
            {
                world.MoveDot(x, y, tX, tY);
            }
        }
    }
}