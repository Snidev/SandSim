using Microsoft.Xna.Framework;
using SandSim.Data;

namespace SandSim.Simulation.System;

public class DotTemplateSystem(World world)
{
    private Dictionary<string, Generator> _templates = [];

    public delegate void Generator(DotTemplateSystem system, Entity ent);
    
    public void AddTemplate(string name, Generator generator) => _templates.Add(name, generator);

    public void ApplyTemplate(Entity ent, string name)
    {
        Generator generator = _templates[name];
        generator.Invoke(this, ent);
    }

    public void InstantiateFromTemplate(Point pos, string name)
    {
        world.AddDot(pos);
        ApplyTemplate(world.GetDot(pos), name);
    }
}