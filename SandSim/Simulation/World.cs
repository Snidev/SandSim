namespace SandSim.Simulation;

public class World(int width, int height)
{
    public readonly int Width = width;
    public readonly int Height = height;
    
    private int[,] _dots = new int[width, height];

    public int GetDot(int x, int y) => _dots[x, y];
    
    public void SetDot(int x, int y, int value) => _dots[x, y] = value;
    
    public void Update()
    {
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            int curDot = GetDot(x, y);

            if (curDot == 1) // Sand, falls down
            {
                if (y >= Height - 1)
                    continue;
                
                if (GetDot(x, y+1) != 0)
                    continue;
                    
                SetDot(x, y, 0);
                SetDot(x, y+1, 1);
            }
        }
    }
}