using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FamilyLibrary.Plugin.Commands.LoadFamilyCommand.Services
{
    /// <summary>
    /// Parser for Revit Type Catalog TXT files.
    /// Type catalogs allow families to have multiple types with different parameter values.
    /// </summary>
    public class TypeCatalogParser
    {
        private const string FieldsPrefix = "##FIELDS##";
        private const string TypesPrefix = "##TYPES##";

        /// <summary>
        /// Parses a type catalog TXT file from file path.
        /// </summary>
        /// <param name="filePath">Path to the type catalog TXT file.</param>
        /// <returns>Parsed TypeCatalog object.</returns>
        /// <exception cref="FileNotFoundException">Thrown when file does not exist.</exception>
        public TypeCatalog Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Type catalog file not found", filePath);
            }

            var lines = File.ReadAllLines(filePath);
            return ParseLines(lines);
        }

        /// <summary>
        /// Parses type catalog from stream.
        /// </summary>
        /// <param name="stream">Stream containing type catalog data.</param>
        /// <returns>Parsed TypeCatalog object.</returns>
        public TypeCatalog Parse(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var lines = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        lines.Add(line);
                    }
                }
            }
            return ParseLines(lines.ToArray());
        }

        /// <summary>
        /// Parses type catalog from lines.
        /// </summary>
        /// <param name="lines">Array of lines from type catalog file.</param>
        /// <returns>Parsed TypeCatalog object.</returns>
        private TypeCatalog ParseLines(string[] lines)
        {
            var catalog = new TypeCatalog();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(FieldsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    catalog.Fields = ParseFields(line);
                }
                else if (line.StartsWith(TypesPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var entry = ParseType(line, catalog.Fields);
                    catalog.Types.Add(entry);
                }
            }

            return catalog;
        }

        /// <summary>
        /// Parses the ##FIELDS## line to extract field names.
        /// </summary>
        /// <param name="line">The FIELDS line from type catalog.</param>
        /// <returns>List of field names.</returns>
        private List<string> ParseFields(string line)
        {
            // ##FIELDS## TypeName##Width##Height##Length
            var content = line.Substring(FieldsPrefix.Length);
            var parts = content.Split(new[] { "##" }, StringSplitOptions.None);
            return parts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList();
        }

        /// <summary>
        /// Parses a ##TYPES## line to extract type values.
        /// </summary>
        /// <param name="line">The TYPES line from type catalog.</param>
        /// <param name="fields">List of field names for mapping values.</param>
        /// <returns>TypeCatalogEntry with mapped values.</returns>
        private TypeCatalogEntry ParseType(string line, List<string> fields)
        {
            // ##TYPES## Standard: 100##200##300
            var content = line.Substring(TypesPrefix.Length);
            var parts = content.Split(new[] { "##" }, StringSplitOptions.None);

            var entry = new TypeCatalogEntry();
            for (int i = 0; i < fields.Count && i < parts.Length; i++)
            {
                entry.Values[fields[i]] = parts[i].Trim();
            }

            return entry;
        }
    }

    /// <summary>
    /// Represents a parsed type catalog.
    /// Contains field definitions and type entries.
    /// </summary>
    public class TypeCatalog
    {
        /// <summary>
        /// List of field/parameter names defined in the catalog.
        /// First field is always TypeName.
        /// </summary>
        public List<string> Fields { get; set; } = new List<string>();

        /// <summary>
        /// List of type entries with their parameter values.
        /// </summary>
        public List<TypeCatalogEntry> Types { get; set; } = new List<TypeCatalogEntry>();
    }

    /// <summary>
    /// Represents a single type entry in the catalog.
    /// Contains values mapped to field names.
    /// </summary>
    public class TypeCatalogEntry
    {
        /// <summary>
        /// Dictionary of field name to value mappings.
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the type name from values dictionary.
        /// Returns "Unknown" if TypeName field is not present.
        /// </summary>
        public string TypeName
        {
            get
            {
                return Values.TryGetValue("TypeName", out var name) ? name : "Unknown";
            }
        }
    }
}
