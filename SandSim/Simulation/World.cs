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
    
    private SandProcessor _sandProcessor = new();
    private WaterProcessor _waterProcessor = new();
    
    private int[,] _dots = new int[width, height];

    public int GetDot(int x, int y) => _dots[x, y];
    
    public void SetDot(int x, int y, int value) => _dots[x, y] = value;

    public void MoveDot(int srcX, int srcY, int dstX, int dstY)
    {
        SetDot(dstX, dstY, GetDot(srcX, srcY));
        SetDot(srcX, srcY, 0);
        PauseDot(dstX, dstY);
    }
    
    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    
    public bool IsEmpty(int x, int y) => _dots[x, y] == 0;
    
    public void PauseDot(int x, int y) => _noUpdate.Add((x, y));
    
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
            
            int curDot = GetDot(x, y);

            if (curDot == 1) // Sand, falls down
            {
                _sandProcessor.Update(this, x, y, curDot);
                continue;
            }

            if (curDot == 2) // Water, spreads and fills containers. Close to sand, but also tries to move horizontally when grounded
            {
                _waterProcessor.Update(this, x, y, curDot);
            }
        }
    }
}