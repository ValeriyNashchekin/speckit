using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Service for stamping families with Extensible Storage.
/// Writes RoleId, RoleName, ContentHash, Timestamp to Family elements.
/// </summary>
public class StampService
{
    private readonly IEsService _esService;

    public StampService() : this(new EsService())
    {
    }

    public StampService(IEsService esService)
    {
        _esService = esService;
    }

    /// <summary>
    /// Stamp multiple families in a single transaction.
    /// Returns count of successfully stamped families.
    /// </summary>
    public int StampFamilies(Document document, List<FamilyQueueItem> items)
    {
        if (document == null || items == null || items.Count == 0)
            return 0;

        var stampedCount = 0;

        // SINGLE transaction for all stamps - batch operation
        using var transaction = new Transaction(document, "Stamp Families");
        transaction.Start();

        try
        {
            foreach (var item in items)
            {
                if (StampSingleFamily(document, item))
                {
                    item.Status = QueueItemStatus.Stamped;
                    item.HasStamp = true;
                    stampedCount++;
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.RollBack();
            throw;
        }

        return stampedCount;
    }

    private bool StampSingleFamily(Document document, FamilyQueueItem item)
    {
        var element = document.GetElement(item.UniqueId);
        if (element is not Family family)
        {
            item.Status = QueueItemStatus.Failed;
            item.ErrorMessage = "Element not found or not a Family";
            return false;
        }

        // Generate stamp data
        var stampData = new EsStampData
        {
            RoleId = Guid.NewGuid(), // MVP: generate new role ID
            RoleName = item.FamilyName,
            FamilyName = item.FamilyName,
            ContentHash = string.Empty, // Will be computed on publish
            StampedAt = DateTime.UtcNow,
            StampedBy = Environment.UserName
        };

        try
        {
            _esService.WriteStamp(family, stampData);
            item.StampData = stampData;
            return true;
        }
        catch (Exception ex)
        {
            item.Status = QueueItemStatus.Failed;
            item.ErrorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Clear stamp from a family.
    /// </summary>
    public void ClearStamp(Document document, string uniqueId)
    {
        var element = document.GetElement(uniqueId);
        if (element == null) return;

        using var transaction = new Transaction(document, "Clear Stamp");
        transaction.Start();

        _esService.ClearStamp(element);

        transaction.Commit();
    }
}
