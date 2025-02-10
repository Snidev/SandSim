using Microsoft.Xna.Framework;
using SandSim.Data;

namespace SandSim.Simulation;

public class World : EntityManager
{
    // ECS implementations
    private readonly IComponentStore[] _componentStore;
    protected override IComponentStore[] ComponentStore => _componentStore;

    public T? GetComponentOrDefault<T>(Point point, Components component) => 
        GetComponentOrDefault<T>(GetDot(point), (int)component);

    public void SetComponent<T>(Point point, Components component, T value) => 
        SetComponent(GetDot(point), (int)component, value);

    public bool HasComponent<T>(Point point, Components component) => HasComponent<T>(GetDot(point), (int)component);
    
    public void AddComponent<T>(Point point, Components component, T value) =>
        AllocateComponent(GetDot(point), (int)component, value);
    
    public uint Particles { get; private set; }
    public readonly Random Random = new();
    public readonly Point Size;
    private readonly Entity[,] _grid;
    private readonly HashSet<Point> _nonUpdate = [];
    private readonly int[] _xOrder;
    
    public bool IsOpen(Point point) => IsInBounds(point) && IsEmpty(point);
    
    public bool IsInBounds(Point point) => point is { X: >= 0, Y: >= 0 } && point.X < Size.X && point.Y < Size.Y;

    public bool IsEmpty(Point point)
    {
        if (!IsInBounds(point))
            throw new OutOfWorldBoundsException();

        return GetComponentOrDefault<DotType>(point, Components.DotType) == DotType.Empty;
    }

    public Entity GetDot(Point point)
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

        (_grid[b.X, b.Y], _grid[a.X, a.Y]) = (_grid[a.X, a.Y], new Entity(-1, -1));
    }

    public void AddDot(DotType dot, Point point)
    {
        if (!IsEmpty(point))
            throw new PlacementConflictException();

        Entity ent = AllocateEntity();
        AllocateComponent(ent, (int)Components.DotType, dot);
        
        _grid[point.X, point.Y] = ent;
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


        Entity ent = _grid[point.X, point.Y];
        if (FreeEntity(ent))
            Particles--;
        
        _grid[point.X, point.Y] = new Entity(-1, -1);
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
                DotType dt = GetComponentOrDefault<DotType>(index, Components.DotType);
                if (dt == DotType.Empty)
                    continue;

                if (_nonUpdate.Contains(index))
                    continue;
                
                // Sand code
                if (dt == DotType.Sand)
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
                        _nonUpdate.Add(index + target);
                    }
                }
            }
        }
    }

    public World(Point size)
    {
        const int BaseSize = 1024;
        Size = size;
        _grid = new Entity[size.X, size.Y];
        for (int x = 0; x < size.X; x++)
        for (int y = 0; y < size.Y; y++)
        {
            _grid[x, y] = new Entity(-1, -1);
        }
        _xOrder = Enumerable.Range(0, size.X).ToArray();
        _componentStore = new IComponentStore[Enum.GetValues(typeof(Components)).Length];
        
        _componentStore[(int)Components.DotType] = new ComponentStore<DotType>(BaseSize, DotType.Empty);
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