namespace SandSim.Simulation;

public class World(int width, int height)
{
    public readonly int Width = width;
    public readonly int Height = height;

    // Quick and dirty solution, optimize later
    private readonly HashSet<(int, int)> _noUpdate = new();
    private readonly Random _rng = new();
    
    private int[,] _dots = new int[width, height];

    public int GetDot(int x, int y) => _dots[x, y];
    
    public void SetDot(int x, int y, int value) => _dots[x, y] = value;
    
    public void Update()
    {
        _noUpdate.Clear();
        
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            if (_noUpdate.Contains((x, y)))
                continue;
            
            int curDot = GetDot(x, y);

            if (curDot == 1) // Sand, falls down
            {
                if (y >= Height - 1)
                    continue;
                
                int newX = x, newY = y + 1;
                if (GetDot(x, newY) != 0)
                {
                    int xOffset = _rng.Next(0, 2) == 1 ? 1 : -1;
                    int xPos = x + xOffset;
                    int xNeg = x - xOffset;
                    if (xPos >= 0 && xPos < Width && GetDot(xPos, newY) == 0)
                        newX += xOffset;
                    else if (xNeg >= 0 && xNeg < Width && GetDot(xNeg, newY) == 0)
                        newX -= xOffset;
                    else
                        continue;
                }
                
                SetDot(x, y, 0);
                SetDot(newX, newY, 1);
                _noUpdate.Add((newX, newY));
            }
        }
    }
}