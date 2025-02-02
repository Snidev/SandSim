using Microsoft.Xna.Framework;

namespace SandSim.Simulation.DotTypes;

public abstract class Dot(World world)
{
    public readonly World World = world;
    private bool _isInWorld = false;
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