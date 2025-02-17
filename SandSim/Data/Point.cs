namespace SandSim.Data;

public struct Point(int x, int y)
{
    public int X = x;
    public int Y = y;
    
    public Vector Vector => new Vector(X, Y);
    
    public bool Equals(Point p) => X.Equals(p.X) && Y.Equals(p.Y);
    
    public static Point Zero = new(0, 0);
    public static Point One = new(1, 1);
    public static Point Up = new(0, 1);
    public static Point Down = new(0, -1);
    public static Point Right = new(1, 0);
    public static Point Left = new(-1, 0);
    
    // Vector operations
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    public static Point operator *(Point a, Point b) => new(a.X * b.X, a.Y * b.Y);
    public static Point operator /(Point a, Point b) => new(a.X / b.X, a.Y / b.Y);
    public static bool operator ==(Point a, Point b) => a.Equals(b);
    public static bool operator !=(Point a, Point b) => !a.Equals(b);

    // Scalar operations
    public static Point operator +(Point a, int b) => new(a.X + b, a.Y + b);
    public static Point operator -(Point a, int b) => new(a.X - b, a.Y - b);
    public static Point operator *(Point a, int b) => new(a.X * b, a.Y * b);
    public static Point operator /(Point a, int b) => new(a.X / b, a.Y / b);
}