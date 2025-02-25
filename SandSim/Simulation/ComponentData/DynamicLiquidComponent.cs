using SandSim.Data;

namespace SandSim.Simulation.ComponentData;

[World.Component(Components.DynamicLiquid)]
public struct DynamicLiquidComponent
{
    public Vector Velocity;
    public float Density;
}