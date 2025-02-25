using Microsoft.Xna.Framework;

namespace SandSim.Simulation.System;

public interface ISimulationUpdateSystem
{
    public bool IsApplicable(Point dot);

    public bool Update(Point dot);
}