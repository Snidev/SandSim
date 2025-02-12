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
    private readonly int[] _edgeOrder;
    private readonly bool[,] _chunkUpdate;
    public readonly int ChunkSize;
    
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
        Wake(a);
        Wake(b);
    }

    public void AddDot(DotType dot, Point point)
    {
        if (!IsEmpty(point))
            throw new PlacementConflictException();

        Entity ent = AllocateEntity();
        AllocateComponent(ent, (int)Components.DotType, dot);
        
        _grid[point.X, point.Y] = ent;
        Particles++;
        Wake(point);
    }

    public void SwapDots(Point a, Point b)
    {
        if (!IsInBounds(a) || !IsInBounds(b))
            throw new OutOfWorldBoundsException();

        (_grid[a.X, a.Y], _grid[b.X, b.Y]) = (_grid[b.X, b.Y], _grid[a.X, a.Y]);
        Wake(a);
        Wake(b);
    }

    public void DeleteDot(Point point)
    {
        if (!IsInBounds(point))
            throw new OutOfWorldBoundsException();


        Entity ent = _grid[point.X, point.Y];
        if (FreeEntity(ent))
        {
            Particles--;
            Wake(point);
        }

        _grid[point.X, point.Y] = new Entity(-1, -1);
    }

    public bool IsPointSleeping(Point point)
    {
        Point chunkPoint = new Point(point.X / ChunkSize, point.Y / ChunkSize);
        return IsChunkSleeping(chunkPoint);
    }

    public bool IsChunkSleeping(Point point)
    {
        return !_chunkUpdate[point.X, point.Y];
    }

    public void Wake(Point point)
    {
        Span<Point> offsets =
        [
            new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(1, -1), new Point(0, -1), new Point(-1, -1),
            new Point(-1, 0), new Point(-1, 1)
        ];
        
        foreach (Point offset in offsets)
        {
            Point curPoint = point + offset;
            Point chunkPoint = new Point(curPoint.X / ChunkSize, curPoint.Y / ChunkSize);
            _chunkUpdate[chunkPoint.X, chunkPoint.Y] = true;
        }
    }

    private void Sleep(Point point)
    {
        _chunkUpdate[point.X, point.Y] = false;
    }

    public bool IsChunkBoundary(Point point)
    {
        return (point.ToVector2() / ChunkSize) == point.ToVector2();
    }
    
    public void Update()
    {
        _nonUpdate.Clear();
        Random.Shuffle(_xOrder);
        Random.Shuffle(_edgeOrder);
        int xChunks = Size.X / ChunkSize + 1;
        int yChunks = Size.Y / ChunkSize + 1;
        
        for (int xChunk = 0; xChunk < xChunks; xChunk++)
        for (int yChunk = 0; yChunk < yChunks; yChunk++)
        {
            Point chunk = new(xChunk, yChunk);
            if (IsChunkSleeping(chunk))
                continue;

            Sleep(chunk);
            int xLim = ChunkSize - Math.Max(0, (xChunk + 1) * ChunkSize - Size.X);
            int yLim = ChunkSize - Math.Max(0, (yChunk + 1) * ChunkSize - Size.Y);
            for (int xRel = 0; xRel < xLim; xRel++)
            for (int yRel = 0; yRel < yLim; yRel++)
            {
                Point position = new Point(xChunk * ChunkSize + (xChunk == xChunks - 1 ? _edgeOrder[xRel] : _xOrder[xRel]), yChunk * ChunkSize + yRel);
                
                if (_nonUpdate.Contains(position))
                    continue;
                
                DotType dot = GetComponentOrDefault<DotType>(position, Components.DotType);

                if (dot == DotType.Sand)
                {
                    Span<Point> targets = [new(0, 1), new(-1, 1), new(1, 1)];
                    Random.Shuffle(targets[1..]);

                    foreach (Point target in targets)
                    {
                        Point tgtPoint = target + position;
                        if (IsOpen(tgtPoint))
                        {
                            SwapDots(tgtPoint, position);
                            _nonUpdate.Add(tgtPoint);
                            break;
                        }
                    }
                }
            }
        }
    }

    public World(Point size, int chunkSize = 32)
    {
        const int BaseSize = 1024;
        Size = size;
        ChunkSize = chunkSize;
        _grid = new Entity[size.X, size.Y];
        for (int x = 0; x < size.X; x++)
        for (int y = 0; y < size.Y; y++)
        {
            _grid[x, y] = new Entity(-1, -1);
        }
        _xOrder = Enumerable.Range(0, ChunkSize).ToArray();
        _edgeOrder = Enumerable.Range(0, Size.X % ChunkSize).ToArray();
        
        _chunkUpdate = new bool[size.X / chunkSize + 1, size.Y / chunkSize + 1];
        
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