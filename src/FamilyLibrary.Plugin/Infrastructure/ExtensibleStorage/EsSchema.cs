using Autodesk.Revit.DB.ExtensibleStorage;

namespace FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

/// <summary>
/// Extensible Storage schema definition for family library stamps.
/// Fields: RoleId, RoleName, FamilyName, Hash, StampedAt, StampedBy
/// </summary>
public static class EsSchema
{
    // Unique GUID for Family Library ES schema v1
    public static readonly Guid SchemaGuid = new Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D");

    private static Schema? _schema;
    private static readonly object _lock = new();

    public static Schema GetSchema()
    {
        if (_schema != null) return _schema;

        lock (_lock)
        {
            // Try to lookup existing schema
            _schema = Schema.Lookup(SchemaGuid);
            if (_schema != null) return _schema;

            // Create new schema
            var builder = new SchemaBuilder(SchemaGuid);
            builder.SetSchemaName("FamilyLibraryStamp");
            builder.SetDocumentation("Family Library stamp data for tracking family roles and versions");

            // Add fields per specification
            builder.AddSimpleField("RoleId", typeof(Guid));
            builder.AddSimpleField("RoleName", typeof(string));
            builder.AddSimpleField("FamilyName", typeof(string));
            builder.AddSimpleField("ContentHash", typeof(string));
            builder.AddSimpleField("StampedAt", typeof(DateTime));
            builder.AddSimpleField("StampedBy", typeof(string));

            // Set read/write access
            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);

            _schema = builder.Finish();
            return _schema;
        }
    }
}
