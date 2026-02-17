using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using FamilyLibrary.Plugin.Core.Interfaces;

namespace FamilyLibrary.Plugin.Infrastructure.Hashing;

/// <summary>
/// Service for computing content hash of RFA files.
/// MVP: PartAtom normalization only (per specification).
/// </summary>
public class ContentHashService : IContentHashService
{
    public string ComputeHash(string rfaFilePath)
    {
        if (!File.Exists(rfaFilePath))
            throw new FileNotFoundException("RFA file not found", rfaFilePath);

        using var fileStream = File.OpenRead(rfaFilePath);
        return ComputeHashFromStream(fileStream);
    }

    public string ComputeHash(byte[] fileContent)
    {
        using var stream = new MemoryStream(fileContent);
        return ComputeHashFromStream(stream);
    }

    private string ComputeHashFromStream(Stream rfaStream)
    {
        // RFA files are ZIP archives
        using var archive = new ZipArchive(rfaStream, ZipArchiveMode.Read, leaveOpen: false);

        // Find PartAtom XML entry
        var partAtomEntry = archive.GetEntry("PartAtom.xml");
        if (partAtomEntry == null)
        {
            // Fallback: hash entire file if PartAtom not found
            return ComputeFallbackHash(archive);
        }

        // Extract and normalize PartAtom XML
        string normalizedXml;
        using (var entryStream = partAtomEntry.Open())
        using (var reader = new StreamReader(entryStream, Encoding.UTF8))
        {
            var xmlContent = reader.ReadToEnd();
            normalizedXml = NormalizePartAtomXml(xmlContent);
        }

        // Compute SHA256 hash (.NET Framework 4.8 compatible)
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedXml));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Normalize PartAtom XML by removing non-deterministic elements.
    /// </summary>
    private string NormalizePartAtomXml(string xmlContent)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xmlContent);

        // Remove PartAtomBuild elements (timestamps)
        var buildNodes = doc.SelectNodes("//PartAtomBuild");
        if (buildNodes != null)
        {
            foreach (XmlNode node in buildNodes)
            {
                node.ParentNode?.RemoveChild(node);
            }
        }

        // Remove timestamp attributes
        RemoveTimestampAttributes(doc);

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8
        });

        doc.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return stringWriter.ToString();
    }

    private void RemoveTimestampAttributes(XmlDocument doc)
    {
        // Remove common timestamp-related attributes
        var timestampAttrs = new[] { "Timestamp", "BuildTime", "Created", "Modified" };
        foreach (var attrName in timestampAttrs)
        {
            var nodes = doc.SelectNodes($"//*[@{attrName}]");
            if (nodes == null) continue;
            
            foreach (XmlElement node in nodes)
            {
                node.RemoveAttribute(attrName);
            }
        }
    }

    private string ComputeFallbackHash(ZipArchive archive)
    {
        // Hash all entry names and sizes as fallback
        var sb = new StringBuilder();
        foreach (var entry in archive.Entries.OrderBy(e => e.FullName))
        {
            sb.Append(entry.FullName);
            sb.Append(':');
            sb.Append(entry.Length);
            sb.Append(';');
        }

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
