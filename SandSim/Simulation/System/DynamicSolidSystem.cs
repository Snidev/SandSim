using SandSim.Data;
using SandSim.Simulation.ComponentData;

namespace SandSim.Simulation.System;

public class DynamicSolidSystem(World world) : ISimulationUpdateSystem
{
    private const int ActsPerTick = 6;
    public bool IsApplicable(Point dot) => world.HasComponent(dot, Components.DynamicSolid);

    public bool Update(Point dot)
    {
        Span<Point> tgtOffsets = [new Point(0, 1), new Point(-1, 1), new Point(1, 1)];
        world.Random.Shuffle(tgtOffsets[1..]);

        Point workingPos = dot;
        for (int i = 0; i < ActsPerTick; i++)
        {
            Point prevPos = workingPos;

            foreach (Point target in tgtOffsets)
            {
                Point tgt = target + workingPos;
                if (world.IsOpen(tgt))
                {
                    workingPos = tgt;
                    break;
                }
            }

            if (workingPos == prevPos)
                break;
        }

        if (workingPos == dot)
            return false;

        world.SwapDots(dot, workingPos);
        world.LockUpdates(workingPos);
        return true;
    }
}