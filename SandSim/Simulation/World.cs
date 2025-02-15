#define MCORE

using System.Reflection;
using Microsoft.Xna.Framework;
using SandSim.Data;
using SandSim.Simulation.ComponentData;
using SandSim.Simulation.System;

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

    public bool HasComponent(Point point, Components component) => HasComponent(GetDot(point), (int)component);
    
    public void AddComponent<T>(Point point, Components component, T value) =>
        AllocateComponent(GetDot(point), (int)component, value);
    
    public uint Particles { get; private set; }
    public readonly Random Random = new();
    public readonly Point Size;
    private readonly Entity[,] _grid;
    //private readonly HashSet<Point> _nonUpdate = [];
    private GridAccessLock _updateLock;
    private readonly int[] _xOrder;
    private readonly int[] _edgeOrder;
    private readonly Chunk[] _chunks;
    private readonly Point _chunkGridSize;
    private readonly SandSystem _sandUpdate;
    public readonly int ChunkSize;

    
    private ref Chunk GetChunk(Point chunk) => ref _chunks[chunk.Y * _chunkGridSize.X + chunk.X];
    private bool IsValidChunk(Point chunk) => 
        chunk is { Y: >= 0, X: >= 0 } && chunk.X < _chunkGridSize.X && chunk.Y < _chunkGridSize.Y;

    public void LockUpdates(Point dot) => _updateLock.TryLock(dot);
    
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
        return GetChunk(point).IsSleeping;
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
            
            if (!IsValidChunk(chunkPoint))
                continue;
            
            ref Chunk chunk = ref GetChunk(chunkPoint);
            chunk.IsSleeping = false;
        }
    }

    private void SleepChunk(Point point)
    {
        ref Chunk chunk = ref GetChunk(point);
        chunk.IsSleeping = true;
    }

    public bool IsChunkBoundary(Point point)
    {
        return (point.ToVector2() / ChunkSize) == point.ToVector2();
    }
    
    public void Update()
    {
        //_nonUpdate.Clear();
        _updateLock.Clear();
        Random.Shuffle(_xOrder);
        Random.Shuffle(_edgeOrder);

        int evens = _chunks.Length / 2;
        int odds = _chunks.Length - evens;
        
        #if MCORE
        Parallel.For(0, odds, i =>
        {
            i *= 2;
            Point chunkPos = new(i % _chunkGridSize.X, i / _chunkGridSize.X);
            ref Chunk chunk = ref GetChunk(chunkPos);
            
            if (!chunk.IsSleeping)
                chunk.Update();
        });

        Parallel.For(0, evens, i =>
        {
            i = i * 2 + 1;
            Point chunkPos = new(i % _chunkGridSize.X, i / _chunkGridSize.X);
            ref Chunk chunk = ref GetChunk(chunkPos);

            if (!chunk.IsSleeping)
                chunk.Update();
        });
        #else
        
        for (int i = 0; i < _chunks.Length; i++)
        {
            ref Chunk chunk = ref _chunks[i];
            if (chunk.IsSleeping)
                continue;
            
            chunk.Update();
        }
        
        #endif

        /*for (int x = 0; x < _chunkGridSize.X; x++)
        for (int y = 0; y < _chunkGridSize.Y; y++)
        {
            ref Chunk chunk = ref GetChunk(new Point(x, y));
            if (chunk.IsSleeping)
                continue;

            chunk.Update();
        }*/
    }

    public World(Point size, int chunkSize = 32)
    {
        ChunkSize = chunkSize;
        
        _xOrder = Enumerable.Range(0, ChunkSize).ToArray();
        _edgeOrder = Enumerable.Range(0, Size.X % ChunkSize).ToArray();
        
        Size = size;
        
        // Todo: Unify this code and the renderchunk code to clean things up a bit
        _grid = new Entity[size.X, size.Y];
        for (int x = 0; x < size.X; x++)
        for (int y = 0; y < size.Y; y++)
        {
            _grid[x, y] = new Entity(-1, -1);
        }
        
        int xChunks = Size.X / ChunkSize + (Size.X % ChunkSize == 0 ? 0 : 1);
        int yChunks = Size.Y / ChunkSize + (Size.Y % ChunkSize == 0 ? 0 : 1);
        _chunkGridSize = new(xChunks, yChunks);
        _chunks = new Chunk[xChunks * yChunks];
        
        for (int x = 0; x < xChunks; x++)
        for (int y = 0; y < yChunks; y++)
            _chunks[y * _chunkGridSize.X + x] = new Chunk(this, (Size.X % chunkSize != 0 && x == xChunks - 1) ? _edgeOrder : _xOrder,
                new Rectangle(x * ChunkSize, 
                    y * ChunkSize, 
                    ChunkSize - Math.Max(0, (x + 1) * ChunkSize - Size.X ),  
                    ChunkSize - Math.Max(0, (y + 1) * ChunkSize - Size.Y)));

        // Reflection code for specifying component data
        _componentStore = new IComponentStore[Enum.GetValues<Components>().Length];
        Type[] componentTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsDefined(typeof(ComponentAttribute), false)).ToArray();

        foreach (Type type in componentTypes)
        {
            ComponentAttribute attr = type.GetCustomAttribute<ComponentAttribute>()!;
            int idx = (int)attr.Component;
            if (idx >= _componentStore.Length)
                throw new ComponentException(
                    $"The enum values of {nameof(Components)} must be ordered({nameof(attr.Component)} = {idx})");

            if (_componentStore[idx] is not null)
                throw new ComponentException(
                    $"Component {nameof(attr.Component)} has a duplicate structure({_componentStore[idx].GetType()} & {type})");

            _componentStore[idx] =
                (IComponentStore)Activator.CreateInstance(typeof(ComponentStore<>).MakeGenericType(type),
                    attr.BaseAmount, attr.Default)!;
        }

        for (int i = 0; i < _componentStore.Length; i++)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            // In this instance, if the project is configured improperly there CAN be null components, but this ensures
            // there are none, and finalizes
            if (_componentStore[i] is null)
                throw new ComponentException($"Component type {(Components)i} has not been defined");
        }
        _updateLock = new GridAccessLock(Size);
    }

    private struct Chunk(World world, int[] xOrder, Rectangle bounds)
    {
        public readonly Rectangle Bounds = bounds;
        public bool IsSleeping = true;

        public void Update()
        {
            IsSleeping = true;

            for (int xIdx = 0; xIdx < Bounds.Width; xIdx++)
            {
                int x = xOrder[xIdx];
                for (int y = 0; y < Bounds.Height; y++)
                {
                    Point absPos = new Point(Bounds.X + x, Bounds.Y + y);

                    if (world._updateLock.IsLocked(absPos))
                    {
                        // In the event that an update locked here means a dot came here from another chunk, so we don't sleep
                        IsSleeping = false;
                        continue;
                    }
                    

                    DotType dot = world.GetComponentOrDefault<DotType>(absPos, Components.DotType);

                    if (dot == DotType.Sand)
                    {
                        Span<Point> targets = [new(0, 1), new(-1, 1), new(1, 1)];
                        world.Random.Shuffle(targets[1..]);

                        foreach (Point target in targets)
                        {
                            Point tgtPoint = target + absPos;
                            if (world.IsOpen(tgtPoint))
                            {
                                world.SwapDots(tgtPoint, absPos);
                                world._updateLock.TryLock(tgtPoint);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum)]
    public class ComponentAttribute(Components component, int baseAmount, object @default) : Attribute
    {
        public Components Component => component;
        public int BaseAmount => baseAmount;
        public object Default => @default;
    }
}

public class ComponentException : Exception
{
    public ComponentException()
    {
    }

    public ComponentException(string message) : base(message)
    {
    }

    public ComponentException(string message, Exception inner) : base(message, inner)
    {
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