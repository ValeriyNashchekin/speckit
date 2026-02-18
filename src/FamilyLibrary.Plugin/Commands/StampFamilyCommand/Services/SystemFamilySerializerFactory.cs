using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FamilyLibrary.Plugin.Core.Enums;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Factory for selecting appropriate serializer based on element type.
/// Routes to correct serializer for Group A, B, C, D, E.
/// </summary>
public class SystemFamilySerializerFactory
{
    private readonly CompoundStructureSerializer _compoundStructureSerializer;
    private readonly RoutingPreferencesSerializer _routingPreferencesSerializer;
    private readonly RailingSerializer _railingSerializer;
    private readonly CurtainSerializer _curtainSerializer;
    private readonly StackedWallSerializer _stackedWallSerializer;

    public SystemFamilySerializerFactory()
        : this(
            new CompoundStructureSerializer(),
            new RoutingPreferencesSerializer(),
            new RailingSerializer(),
            new CurtainSerializer(),
            new StackedWallSerializer())
    {
    }

    public SystemFamilySerializerFactory(
        CompoundStructureSerializer compoundStructureSerializer,
        RoutingPreferencesSerializer routingPreferencesSerializer,
        RailingSerializer railingSerializer,
        CurtainSerializer curtainSerializer,
        StackedWallSerializer stackedWallSerializer)
    {
        _compoundStructureSerializer = compoundStructureSerializer
            ?? throw new ArgumentNullException(nameof(compoundStructureSerializer));
        _routingPreferencesSerializer = routingPreferencesSerializer
            ?? throw new ArgumentNullException(nameof(routingPreferencesSerializer));
        _railingSerializer = railingSerializer
            ?? throw new ArgumentNullException(nameof(railingSerializer));
        _curtainSerializer = curtainSerializer
            ?? throw new ArgumentNullException(nameof(curtainSerializer));
        _stackedWallSerializer = stackedWallSerializer
            ?? throw new ArgumentNullException(nameof(stackedWallSerializer));
    }

    /// <summary>
    /// Gets the appropriate serializer for the given element type.
    /// Returns null if no serializer is available.
    /// </summary>
    public object? GetSerializer(Element elementType)
    {
        if (elementType == null)
            return null;

        // Group A: CompoundStructure types
        if (elementType is WallType wallType)
        {
            if (wallType.Kind == WallKind.Curtain)
                return _curtainSerializer;

            if (wallType.Kind == WallKind.Stacked)
                return _stackedWallSerializer;

            return _compoundStructureSerializer;
        }

        if (elementType is FloorType || elementType is RoofType ||
            elementType is CeilingType || elementType is WallFoundationType)
        {
            return _compoundStructureSerializer;
        }

        // Group B: MEP with RoutingPreferences
        if (elementType is PipeType || elementType is DuctType)
        {
            return _routingPreferencesSerializer;
        }

        // Group C: Railings and Stairs
        if (elementType is RailingType)
        {
            return _railingSerializer;
        }

        // Group E: Simple types (no special serializer)
        return null;
    }

    /// <summary>
    /// Gets the group classification for an element type.
    /// </summary>
    public SystemFamilyGroup GetGroup(Element elementType)
    {
        if (elementType == null)
            return SystemFamilyGroup.GroupA;

        // Group A: CompoundStructure types
        if (elementType is WallType wallType)
        {
            if (wallType.Kind == WallKind.Curtain)
                return SystemFamilyGroup.GroupD;

            if (wallType.Kind == WallKind.Stacked)
                return SystemFamilyGroup.GroupD;

            return SystemFamilyGroup.GroupA;
        }

        if (elementType is FloorType || elementType is RoofType ||
            elementType is CeilingType || elementType is WallFoundationType)
        {
            return SystemFamilyGroup.GroupA;
        }

        // Group B: MEP with RoutingPreferences
        if (elementType is PipeType || elementType is DuctType)
        {
            return SystemFamilyGroup.GroupB;
        }

        // Group C: Railings and Stairs
        if (elementType is RailingType)
        {
            return SystemFamilyGroup.GroupC;
        }

        // Group D: Curtain systems
        // Additional curtain system types would go here

        // Group E: Simple types
        if (elementType is Level || elementType is Grid)
        {
            return SystemFamilyGroup.GroupE;
        }

        return SystemFamilyGroup.GroupA;
    }

    /// <summary>
    /// Serializes the element type to JSON using the appropriate serializer.
    /// </summary>
    public string? SerializeToJson(Element elementType, Document document)
    {
        if (elementType == null || document == null)
            return null;

        // Group A: CompoundStructure types
        if (elementType is WallType wallType)
        {
            if (wallType.Kind == WallKind.Curtain)
                return _curtainSerializer.SerializeToJson(wallType, document);

            if (wallType.Kind == WallKind.Stacked)
                return _stackedWallSerializer.SerializeToJson(wallType, document);

            var hostAttrs = elementType as HostObjAttributes;
            if (hostAttrs != null)
            {
                var structure = hostAttrs.GetCompoundStructure();
                return _compoundStructureSerializer.Serialize(structure, document);
            }
        }

        if (elementType is FloorType floorType)
        {
            var structure = floorType.GetCompoundStructure();
            return _compoundStructureSerializer.Serialize(structure, document);
        }

        if (elementType is RoofType roofType)
        {
            var structure = roofType.GetCompoundStructure();
            return _compoundStructureSerializer.Serialize(structure, document);
        }

        if (elementType is CeilingType ceilingType)
        {
            var structure = ceilingType.GetCompoundStructure();
            return _compoundStructureSerializer.Serialize(structure, document);
        }

        // Group B: MEP with RoutingPreferences
        if (elementType is PipeType pipeType)
        {
            return _routingPreferencesSerializer.SerializeToJson(pipeType, document);
        }

        if (elementType is DuctType ductType)
        {
            return _routingPreferencesSerializer.SerializeToJson(ductType, document);
        }

        // Group C: Railings
        if (elementType is RailingType railingType)
        {
            return _railingSerializer.SerializeToJson(railingType, document);
        }

        return null;
    }

    /// <summary>
    /// Gets the RailingSerializer for Group C operations.
    /// </summary>
    public RailingSerializer GetRailingSerializer() => _railingSerializer;

    /// <summary>
    /// Gets the CurtainSerializer for Group D operations.
    /// </summary>
    public CurtainSerializer GetCurtainSerializer() => _curtainSerializer;

    /// <summary>
    /// Gets the StackedWallSerializer for Group D operations.
    /// </summary>
    public StackedWallSerializer GetStackedWallSerializer() => _stackedWallSerializer;
}
