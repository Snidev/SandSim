using Microsoft.Xna.Framework;

namespace SandSim.Simulation.ComponentData;

[World.Component(Components.ColorData, 1024)]
public record struct ColorData(Color Color);