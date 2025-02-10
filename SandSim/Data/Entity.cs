namespace SandSim.Data;

public readonly struct Entity(int index, int generation)
{
    public readonly int Id = index;
    private readonly int _generation = generation;

    public static Entity Null => new(-1, -1);
    
    public static bool operator ==(Entity left, Entity right) 
        => left.Id == right.Id && left._generation == right._generation;

    public static bool operator !=(Entity left, Entity right) => !(left == right);
}