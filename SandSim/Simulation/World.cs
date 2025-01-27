using SandSim.Simulation.Logic;

namespace SandSim.Simulation;

public class World(int width, int height)
{
    public readonly int Width = width;
    public readonly int Height = height;
    public readonly Random Rng = new();

    // Quick and dirty solution, optimize later
    private readonly HashSet<(int, int)> _noUpdate = new();
    private readonly int[] xOrder = Enumerable.Range(0, width).ToArray();
    private readonly Random _rng = new();
    private readonly Dictionary<string, int> _dotTypeLookup = new();
    private readonly List<DotType> _dotTypes = [ new DotType("Air", null, 0)];
    
    private SandProcessor _sandProcessor = new();
    private FluidProcessor _fluidProcessor = new();
    
    private int[,] _dots = new int[width, height];

    public int GetDotId(int x, int y) => _dots[x, y];

    public DotType GetDot(int x, int y) => _dotTypes[_dots[x, y]];

    public void SetDot(int x, int y, string dotType)
    {
        int dot = LookupDotByName(dotType);
        dot = dot == -1 ? 0 : dot;

        SetDotById(x, y, dot);
    }

    private void SetDotById(int x, int y, int id) => _dots[x, y] = id;

    public int LookupDotByName(string name) => _dotTypeLookup.GetValueOrDefault(name, -1);

    public void MoveDot(int srcX, int srcY, int dstX, int dstY)
    {
        SetDotById(dstX, dstY, GetDotId(srcX, srcY));
        SetDotById(srcX, srcY, 0);
        PauseDot(dstX, dstY);
    }
    
    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    
    public bool IsEmpty(int x, int y) => _dots[x, y] == 0;
    
    public void PauseDot(int x, int y) => _noUpdate.Add((x, y));

    public void RegisterDotType(string identifier, DotProcessor processor)
    {
        int id = _dotTypes.Count;

        if (!_dotTypeLookup.TryAdd(identifier, id))
            throw new DuplicateDotTypeException();
        
        _dotTypes.Add(new DotType(identifier, processor, id));
    }
    
    public void Update()
    {
        _noUpdate.Clear();
        Rng.Shuffle(xOrder);
        
        for (int xIndex = 0; xIndex < Width; xIndex++)
        for (int y = 0; y < Height; y++)
        {
            int x = xOrder[xIndex];
            if (_noUpdate.Contains((x, y)))
                continue;
            
            int curDot = GetDotId(x, y);

            if (curDot == 1) // Sand, falls down
            {
                _sandProcessor.Update(this, x, y, curDot);
                continue;
            }

            if (curDot == 2) // Water, spreads and fills containers. Close to sand, but also tries to move horizontally when grounded
            {
                _fluidProcessor.Update(this, x, y, curDot);
            }
        }
    }
}

public class DuplicateDotTypeException : Exception
{
    public DuplicateDotTypeException()
    {
    }

    public DuplicateDotTypeException(string message) : base(message)
    {
    }

    public DuplicateDotTypeException(string message, Exception inner) : base(message, inner)
    {
    }
}