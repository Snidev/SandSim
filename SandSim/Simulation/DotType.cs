using SandSim.Simulation.Logic;

namespace SandSim.Simulation;

public readonly struct DotType
{
    public readonly string Name;
    public readonly DotProcessor? Processor;
    public readonly int InternalId;

    public DotType(string name, DotProcessor? processor, int id)
    {
        Name = name;
        Processor = processor;
        InternalId = id;
    }
}