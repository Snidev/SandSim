namespace SandSim.Simulation.Logic;

public class FluidProcessor : DotProcessor
{
    public float Viscosity = 1f;
    
    public override void Update(World world, int x, int y, int dotType)
    {        
        if (world.IsInBounds(x, y + 1) && world.IsEmpty(x, y + 1))
        {
            world.MoveDot(x, y, x, y + 1);
            return;
        }

        if (world.Rng.NextDouble() > Viscosity)
            return;
        
        Span<int> xValues = [x + 1, x - 1];
        if (world.Rng.Next(0, 2) == 1)
            (xValues[0], xValues[1]) = (xValues[1], xValues[0]);

        foreach (int xOff in xValues)
        {
            if (world.IsInBounds(xOff, y) && world.IsEmpty(xOff, y))
            {
                int yOff = y + 1;
                if (world.IsInBounds(xOff, yOff) && world.IsEmpty(xOff, yOff))
                {
                    world.MoveDot(x, y, xOff, yOff);
                    return;
                }
                world.MoveDot(x, y, xOff, y);
                return;
            }
        }
    }
}