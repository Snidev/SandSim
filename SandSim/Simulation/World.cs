using Microsoft.Xna.Framework;

namespace SandSim.Simulation;

public class World(Point size)
{
    public uint Particles { get; private set; }
    public readonly Random Random = new();
    public readonly Point Size = size;
    private readonly DotType[,] _grid = new DotType[size.X, size.Y];
    private readonly HashSet<Point> _nonUpdate = [];
    private readonly int[] _xOrder = Enumerable.Range(0, size.X).ToArray();

    public bool IsOpen(Point point) => IsInBounds(point) && IsEmpty(point);
    
    public bool IsInBounds(Point point) => point is { X: >= 0, Y: >= 0 } && point.X < Size.X && point.Y < Size.Y;

    public bool IsEmpty(Point point)
    {
        if (!IsInBounds(point))
            throw new OutOfWorldBoundsException();

        return _grid[point.X, point.Y] == 0;
    }

    public DotType GetDot(Point point)
    {
        if (!IsInBounds(point))
            throw new OutOfWorldBoundsException();

        return _grid[point.X, point.Y];
    }
    
    public void MoveDot(Point a, Point b)
    {
        if (!IsInBounds(a))
            throw new OutOfWorldBoundsException();
        
        if (!IsEmpty(b))
            throw new PlacementConflictException();

        (_grid[b.X, b.Y], _grid[a.X, a.Y]) = (_grid[a.X, a.Y], 0);
    }

    public void AddDot(DotType dot, Point point)
    {
        if (!IsEmpty(point))
            throw new PlacementConflictException();

        _grid[point.X, point.Y] = dot;
        Particles++;
    }

    public void SwapDots(Point a, Point b)
    {
        if (!IsInBounds(a) || !IsInBounds(b))
            throw new OutOfWorldBoundsException();

        (_grid[a.X, a.Y], _grid[b.X, b.Y]) = (_grid[b.X, b.Y], _grid[a.X, a.Y]);
    }

    public void DeleteDot(Point point)
    {
        if (!IsInBounds(point))
            throw new OutOfWorldBoundsException();

        Particles -= _grid[point.X, point.Y] != 0 ? 1u : 0u;
        
        _grid[point.X, point.Y] = 0;
    }

    public void Update()
    {
        _nonUpdate.Clear();
        Random.Shuffle(_xOrder);

        for (int xIdx = 0; xIdx < Size.X; xIdx++)
        {
            int x = _xOrder[xIdx];
            for (int y = 0; y < Size.Y; y++)
            {
                Point index = new Point(x, y);
                if (_grid[x, y] == 0)
                    continue;

                DotType dot = _grid[x, y]!;

                if (_nonUpdate.Contains(index))
                    continue;
                
                // Sand code
                if (dot == DotType.Sand)
                {
                    Span<Point> targets = [new Point(0, 1), new Point(1, 1), new Point(-1, 1)];
                    Point target = Point.Zero;
                    bool hasTarget = false;
                    Random.Shuffle(targets[1..]);

                    foreach (Point point in targets)
                    {
                        if (!IsOpen(index + point)) 
                            continue;
                        
                        hasTarget = true;
                        target = point;
                    }

                    if (hasTarget)
                    {
                        SwapDots(index, index + target);
                    }
                }
            }
        }
    }
}

public class WorldConflictException : Exception
{
    public WorldConflictException()
    {
    }

    public WorldConflictException(string message) : base(message)
    {
    }

    public WorldConflictException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class PlacementConflictException : Exception
{
    public PlacementConflictException()
    {
    }

    public PlacementConflictException(string message) : base(message)
    {
    }

    public PlacementConflictException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class OutOfWorldBoundsException : Exception
{
    public OutOfWorldBoundsException()
    {
    }

    public OutOfWorldBoundsException(string message) : base(message)
    {
    }

    public OutOfWorldBoundsException(string message, Exception inner) : base(message, inner)
    {
    }
}