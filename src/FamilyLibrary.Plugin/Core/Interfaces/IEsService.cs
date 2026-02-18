using FamilyLibrary.Plugin.Core.Entities;

namespace FamilyLibrary.Plugin.Core.Interfaces;

/// <summary>
/// Service for reading and writing Extensible Storage stamp data.
/// </summary>
public interface IEsService
{
    /// <summary>
    /// Read stamp data from a family element.
    /// </summary>
    EsStampData? ReadStamp(object element);

    /// <summary>
    /// Write stamp data to a family element.
    /// </summary>
    void WriteStamp(object element, EsStampData stampData);

    /// <summary>
    /// Check if element has a valid stamp.
    /// </summary>
    bool HasStamp(object element);

    /// <summary>
    /// Clear stamp data from element.
    /// </summary>
    void ClearStamp(object element);
}
