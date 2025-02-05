using Microsoft.Xna.Framework;
using SandSim.Simulation.DotTypes;

namespace SandSim.Simulation.Physics;

/// <summary>
/// Iterative structure to query grid from point A (exclusive) to point B (inclusive) 
/// </summary>
public struct LinearTrace
{
    public readonly World World;
    public readonly Point Origin;
    public readonly Point Destination;
    public Point CurPosition { get; private set; }
    public int StepCount { get; private set; }
    private bool _finished = false;
    public bool Finished => Origin == Destination || _finished;

    private int _error;
    private Point _delta;
    private readonly Point _step;
    private const double Sqrt2 = 1.41421356237f;

    public Point NextStep
    {
        get
        {
            Point ret = CurPosition;
            if (Finished)
                return ret;
        
            int e2 = _error * 2;

            if (e2 > -_delta.Y)
            {
                ret = CurPosition + new Point(_step.X, 0);
            }

            if (e2 < _delta.X)
            {
                ret = CurPosition + new Point(0, _step.Y);
            }

            return ret;
        }
    }
    
    public Dot? Step()
    {
        if (Finished)
            return null;
        
        int e2 = _error * 2;
        Span<double> dist = [1, Sqrt2];

        if (e2 > -_delta.Y)
        {
            _error -= _delta.Y;
            CurPosition += new Point(_step.X, 0);
        }

        if (e2 < _delta.X)
        {
            _error += _delta.X;
            CurPosition += new Point(0, _step.Y);
        }
        
        if (CurPosition == Destination)
            _finished = true;

        StepCount++;
        return World.GetDot(CurPosition);
    }

    private static (Point delta, Point step) CalcVars(Point origin, Point destination)
    {
        Point delta = new Point(Math.Abs(destination.X - origin.X), Math.Abs(destination.Y - origin.Y));
        Point step = new Point(origin.X < destination.X ? 1 : -1, origin.Y < destination.Y ? 1 : -1);
        
        return (delta, step);
    }
    
    public LinearTrace(World world, Point origin, Point destination)
    {
        Origin = origin;
        Destination = destination;
        CurPosition = origin;
        (_delta, _step) = CalcVars(origin, destination);
        World = world;
        _error = _delta.X - _delta.Y;
    }

    public LinearTrace(World world, Point origin, double theta, int distance)
    {
        Origin = origin;
        Destination = origin + new Point((int)Math.Round(distance * Math.Cos(theta)),
            (int)Math.Round(distance * Math.Sin(theta)));
        CurPosition = origin;
        
        (_delta, _step) = CalcVars(origin, Destination);
        World = world;
        _error = _delta.X - _delta.Y;
    }
}