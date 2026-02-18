using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Interfaces;

namespace FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

/// <summary>
/// Implementation of Extensible Storage service for reading/writing stamp data.
/// </summary>
public class EsService : IEsService
{
    public EsStampData? ReadStamp(object element)
    {
        if (element is not Element revitElement) return null;

        var schema = EsSchema.GetSchema();
        var entity = revitElement.GetEntity(schema);

        if (entity == null || !entity.IsValid()) return null;

        try
        {
            return new EsStampData
            {
                RoleId = entity.Get<Guid>("RoleId"),
                RoleName = entity.Get<string>("RoleName") ?? string.Empty,
                FamilyName = entity.Get<string>("FamilyName") ?? string.Empty,
                ContentHash = entity.Get<string>("ContentHash") ?? string.Empty,
                StampedAt = entity.Get<DateTime>("StampedAt"),
                StampedBy = entity.Get<string>("StampedBy") ?? string.Empty
            };
        }
        catch
        {
            return null;
        }
    }

    public void WriteStamp(object element, EsStampData stampData)
    {
        if (element is not Element revitElement) return;

        var schema = EsSchema.GetSchema();
        var entity = new Entity(schema);

        entity.Set("RoleId", stampData.RoleId);
        entity.Set("RoleName", stampData.RoleName ?? string.Empty);
        entity.Set("FamilyName", stampData.FamilyName ?? string.Empty);
        entity.Set("ContentHash", stampData.ContentHash ?? string.Empty);
        entity.Set("StampedAt", stampData.StampedAt);
        entity.Set("StampedBy", stampData.StampedBy ?? string.Empty);

        revitElement.SetEntity(entity);
    }

    public bool HasStamp(object element)
    {
        return ReadStamp(element)?.IsValid == true;
    }

    public void ClearStamp(object element)
    {
        if (element is not Element revitElement) return;

        var schema = EsSchema.GetSchema();
        revitElement.DeleteEntity(schema);
    }
}
