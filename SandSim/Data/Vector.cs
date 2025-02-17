namespace SandSim.Data;

public struct Vector(float x, float y)
{
    public float X => x; 
    public float Y => y;

    public Vector Normalized
    {
        get
        {
            float sqMag = SqrMagnitude;
            return sqMag > 0 ? this / float.Sqrt(sqMag) : Zero;
        }
    }
    public Point Point => new Point((int)X, (int)Y);
    public float Magnitude => MathF.Sqrt(X * X + Y * Y);
    public float SqrMagnitude => X * X + Y * Y;
    
    public bool Equals(Vector other) => X.Equals(other.X) && Y.Equals(other.Y);
    
    public static Vector Zero => new(0, 0);
    public static Vector One => new(1, 1);
    public static Vector Up => new(0, 1);
    public static Vector Down => new(0, -1);
    public static Vector Right => new(1, 0);
    public static Vector Left => new(-1, 0);

    // Vector operations
    public static Vector operator +(Vector v1, Vector v2) => new(v1.X + v2.X, v1.Y + v2.Y);
    public static Vector operator -(Vector v1, Vector v2) => new(v1.X - v2.X, v1.Y - v2.Y);
    public static Vector operator *(Vector v1, Vector v2) => new(v1.X * v2.X, v1.Y * v2.Y);
    public static Vector operator /(Vector v1, Vector v2) => new(v1.X / v2.X, v1.Y / v2.Y);
    public static bool operator ==(Vector v1, Vector v2) => v1.Equals(v2);
    public static bool operator !=(Vector v1, Vector v2) => !v1.Equals(v2);
    public static float Cross(Vector v1, Vector v2) => v1.X * v2.Y - v1.Y * v2.X;
    public static float Dot(Vector v1, Vector v2) => v1.X * v2.X + v1.Y * v2.Y;
    
    // Scalar operations
    public static Vector operator +(Vector v, float b) => new(v.X + b, v.Y + b);
    public static Vector operator -(Vector v, float b) => new(v.X - b, v.Y - b);
    public static Vector operator *(Vector v, float b) => new(v.X * b, v.Y * b);
    public static Vector operator /(Vector v, float b) => new(v.X / b, v.Y / b);
}