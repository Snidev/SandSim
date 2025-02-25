using SandSim.Simulation.ComponentData;
using SandSim.Data;

namespace SandSim.Simulation.System;

public class LiquidSystem(World world) : ISimulationUpdateSystem
{
    private const int ActsPerTick = 50;
    
    public bool IsApplicable(Point dot) => 
        world.HasComponent(dot, Components.DynamicLiquid);

    public bool Update(Point dot)
    {
        world.GetComponentOrDefault<DynamicLiquidComponent>(dot, Components.DynamicLiquid, out DynamicLiquidComponent dlc);
        
        
        
        if (dlc.Velocity == Vector.Zero) 
            return false;

        return false;
    }
}