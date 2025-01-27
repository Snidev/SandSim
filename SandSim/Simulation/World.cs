namespace SandSim.Simulation;

public class World(int width, int height)
{
    public readonly int Width = width;
    public readonly int Height = height;
    public readonly Random Rng = new();

    // Quick and dirty solution, optimize later
    private readonly HashSet<(int, int)> _noUpdate = new();
    private readonly Random _rng = new();
    
    private int[,] _dots = new int[width, height];

    public int GetDot(int x, int y) => _dots[x, y];
    
    public void SetDot(int x, int y, int value) => _dots[x, y] = value;
    
    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    
    public bool IsEmpty(int x, int y) => _dots[x, y] == 0;
    
    public void PauseDot(int x, int y) => _noUpdate.Add((x, y));
    
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

            else if (curDot == 2) // Water, spreads and fills containers. Close to sand, but also tries to move horizontally when grounded
            {
                if (y >= Height - 1)
                    continue;
                
                int newX = x, newY = y;
                if (GetDot(x, y + 1) == 0)
                {
                    newY = y + 1;
                }
                else
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
                SetDot(newX, newY, 2);
                _noUpdate.Add((newX, newY));
            }
        }
    }
}