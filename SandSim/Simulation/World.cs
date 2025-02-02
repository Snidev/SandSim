using Microsoft.Xna.Framework;
using SandSim.Simulation.DotTypes;

namespace SandSim.Simulation;

public class World(Point size)
{
    public readonly Random Random = new();
    public readonly Point Size = size;
    private readonly Dot?[,] _grid = new Dot?[size.X, size.Y];
    private readonly HashSet<Dot> _nonUpdate = [];
    private readonly int[] _xOrder = Enumerable.Range(0, size.X).ToArray();

    public bool IsOpen(Point point) => IsInBounds(point) && IsEmpty(point);
    
    public bool IsInBounds(Point point) => point is { X: >= 0, Y: >= 0 } && point.X < Size.X && point.Y < Size.Y;

    public bool IsEmpty(Point point)
    {
        if (!IsInBounds(point))
            throw new OutOfWorldBoundsException();

        return _grid[point.X, point.Y] is null;
    }

    public Dot? GetDot(Point point)
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

        (_grid[b.X, b.Y], _grid[a.X, a.Y]) = (_grid[a.X, a.Y], null);
    }

    public void AddDot(Dot dot, Point point)
    {
        if (!IsEmpty(point))
            throw new PlacementConflictException();

        if (dot.World != this)
            throw new WorldConflictException();

        _grid[point.X, point.Y] = dot;
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

        _grid[point.X, point.Y] = null;
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
                if (_grid[x, y] == null)
                    continue;

                Dot dot = _grid[x, y]!;

                if (_nonUpdate.Contains(dot))
                    continue;

                dot.Update(new Point(x, y));
                _nonUpdate.Add(dot);
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