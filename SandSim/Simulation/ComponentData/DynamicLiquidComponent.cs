using Microsoft.Xna.Framework;

namespace SandSim.Simulation.ComponentData;

[World.Component(Components.DynamicLiquid)]
public struct DynamicLiquidComponent
{
    public Vector2 Velocity;
    public float Density;
}