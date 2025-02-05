using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public abstract class Dot
{
    public static int Allocated { get; private set; }
    public readonly World World;
    private bool _isInWorld = false;

    protected Dot(World world)
    {
        World = world;
        Allocated++;
    }

    ~Dot()
    {
        Allocated--;
    }

    public abstract Color Color { get; }

    public abstract void Update(Point position);

    public void MarkAdded()
    {
        if (_isInWorld)
            throw new DuplicateInstantiationException();

        _isInWorld = true;
    }
}

public class DuplicateInstantiationException : Exception
{
    public DuplicateInstantiationException()
    {
    }

    public DuplicateInstantiationException(string message) : base(message)
    {
    }

    public DuplicateInstantiationException(string message, Exception inner) : base(message, inner)
    {
    }
}