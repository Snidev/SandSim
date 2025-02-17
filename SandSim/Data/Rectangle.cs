namespace SandSim.Data;

public struct Rectangle(int x, int y, int w, int h)
{
    private int _x = x;
    private int _y = y;
    private int _w = w;
    private int _h = h;
    private int _bottom = y + h;
    private int _right = x + w;
    
    public Point Location => new(_x, _y);
    public Point Max => new(_bottom, _right);
    public Point Size => new(_w, _h);

    public int X
    {
        get => _x;
        set
        {
            _x = value;
            _right = _x + _w;
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            _y = value;
            _bottom = _y + _h;
        }
    }

    public int Width
    {
        get => _w;
        set
        {
            _w = value;
            _right = _x + _w;
        }
    }

    public int Height
    {
        get => _h;
        set
        {
            _h = value;
            _bottom = _y + _h;
        }
    }

    public int Right
    {
        get => _right;
        set
        {
            _right = value;
            X = _right - _w;
        }
    }

    public int Bottom
    {
        get => _bottom;
        set
        {
            _bottom = value;
            Y = _bottom - _h;
        }
    }
}