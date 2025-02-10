using Microsoft.Xna.Framework;
using SandSim.Data;

namespace SandSim.Simulation.Physics;

public struct RadialTrace(World world, Point origin, int radius)
{
    // todo: Optimize to remove duplicate grid queries
    public readonly int Radius = radius;
    public readonly Point Origin = origin;
    public Point CurPosition { get; private set; } = origin;

    public bool Finished { get; private set; } = radius <= 0;

    private LinearTrace _trace = new LinearTrace(world, origin, origin + GetTargetPoint(0, radius));
    private int _squareStep = 0;
    private readonly int _stepMax = radius * 8;
    private readonly double _sqRadius = radius * radius;

    private static Point GetTargetPoint(int step, int radius)
    {
        int switchCond = step / (radius * 2);
        int variable = step % (radius * 2) - radius;
        return switchCond switch
        {
            0 => new Point(radius, variable),
            1 => new Point(-variable, radius),
            2 => new Point(-radius, -variable),
            3 => new Point(variable, -radius),
            _ => Point.Zero
        };
    }

    public DotType Step()
    {
        if (Finished)
            return DotType.Empty;

        double sqDist = Math.Round(Math.Pow(_trace.CurPosition.X - Origin.X, 2) + Math.Pow(_trace.CurPosition.Y - Origin.Y, 2));
        
        DotType result = _trace.Step();
        CurPosition = _trace.CurPosition;

        if (sqDist > _sqRadius || _trace.Finished)
        {
            if (_squareStep == _stepMax - 1)
                Finished = true;
            else
                _trace = new LinearTrace(world, Origin, Origin + GetTargetPoint(++_squareStep, Radius));
        }
        
        return result;
    }
}