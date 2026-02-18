namespace FamilyLibrary.Plugin.Core.Enums;

/// <summary>
/// Groups for system families based on complexity.
/// </summary>
public enum SystemFamilyGroup
{
    GroupA = 0,  // CompoundStructure: Walls, Floors, Roofs, Ceilings, Foundations
    GroupB = 1,  // MEP: Pipes, Ducts (Phase 2)
    GroupC = 2,  // Railings, Stairs (Phase 3)
    GroupD = 3,  // Curtain Systems (Phase 3)
    GroupE = 4   // Simple: Levels, Grids, Ramps, Building Pads
}
