using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AzzyAIConfig
{
    internal sealed class ImportedTabSettings
    {
        internal readonly string Scope;
        internal readonly Dictionary<string, string> Properties;
        internal readonly Dictionary<string, string> Files;

        internal ImportedTabSettings(string scope, Dictionary<string, string> properties, Dictionary<string, string> files)
        {
            Scope = scope;
            Properties = properties;
            Files = files;
        }
    }

    /// <summary>Reads and writes scoped settings packages without extracting files to the runtime directory.</summary>
    internal static class TabSettingsBundle
    {
        const string ManifestName = "tab-settings.xml";
        const string PropertiesName = "properties.xml";
        const string FormatName = "KimiTabSettings";

        static readonly HashSet<string> PropertyScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "shared-settings", "ward", "occult", "agile", "raging"
        };

        static readonly Dictionary<string, string> FileScopes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "kimi-tactics", "H_Tactics.lua" },
            { "pvp-tactics", "H_PVP_Tact.lua" },
            { "blueprint", "combo_tactics.cbp" },
            { "extra", "H_Extra.lua" }
        };

        internal static void Export(string path, string scope, IDictionary<string, string> properties,
                                    IDictionary<string, string> files)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentException("An export path is required.", "path");
            string normalizedScope = NormalizeScope(scope);
            bool isPropertyScope = PropertyScopes.Contains(normalizedScope);
            string expectedFile = null;
            if (!isPropertyScope && !FileScopes.TryGetValue(normalizedScope, out expectedFile))
                throw new InvalidDataException("Unknown settings scope: " + normalizedScope);

            if (isPropertyScope)
            {
                if (properties == null || files == null || files.Count != 0)
                    throw new InvalidDataException("A property settings package contains unexpected file data.");
            }
            else if (properties == null || properties.Count != 0 || files == null || files.Count != 1 ||
                     !files.ContainsKey(expectedFile))
            {
                throw new InvalidDataException("The package contents do not match the selected settings tab.");
            }

            string archivePath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(archivePath);
            if (String.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                throw new DirectoryNotFoundException("The settings package directory does not exist: " + directory);

            string temporaryPath = archivePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    WriteEntry(archive, ManifestName, CreateManifest(normalizedScope));
                    if (isPropertyScope)
                        WriteEntry(archive, PropertiesName, CreateProperties(properties));
                    else
                        WriteEntry(archive, expectedFile, Encoding.UTF8.GetBytes(files[expectedFile] ?? String.Empty));
                }

                if (File.Exists(archivePath)) File.Replace(temporaryPath, archivePath, null);
                else File.Move(temporaryPath, archivePath);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        internal static void ExportMany(string path, IEnumerable<ImportedTabSettings> settings)
        {
            var items = settings == null ? new List<ImportedTabSettings>() : settings.ToList();
            if (items.Count == 0) throw new InvalidDataException("Select at least one tab to export.");

            var seenScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var manifestScopes = new XElement("Scopes");
            var payloads = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (ImportedTabSettings item in items)
            {
                string scope = NormalizeScope(item.Scope);
                if (!seenScopes.Add(scope)) throw new InvalidDataException("A tab was selected more than once: " + scope);
                bool propertiesScope = PropertyScopes.Contains(scope);
                string expectedFile = null;
                string entryName;
                if (propertiesScope)
                {
                    if (item.Files == null || item.Files.Count != 0 || item.Properties == null)
                        throw new InvalidDataException("The package contents do not match the selected settings tab.");
                    entryName = scope + ".properties.xml";
                    payloads.Add(entryName, CreateProperties(item.Properties));
                }
                else
                {
                    if (!FileScopes.TryGetValue(scope, out expectedFile) || item.Properties == null || item.Properties.Count != 0 ||
                        item.Files == null || item.Files.Count != 1 || !item.Files.ContainsKey(expectedFile))
                        throw new InvalidDataException("The package contents do not match the selected settings tab.");
                    entryName = expectedFile;
                    payloads.Add(entryName, Encoding.UTF8.GetBytes(item.Files[expectedFile] ?? String.Empty));
                }
                manifestScopes.Add(new XElement("Scope", new XAttribute("id", scope), new XAttribute("entry", entryName)));
            }

            var manifest = new XDocument(new XElement(FormatName, new XAttribute("version", "2"), manifestScopes));
            string archivePath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(archivePath);
            if (String.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                throw new DirectoryNotFoundException("The settings package directory does not exist: " + directory);

            string temporaryPath = archivePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    WriteEntry(archive, ManifestName, Encoding.UTF8.GetBytes(manifest.ToString(SaveOptions.DisableFormatting)));
                    foreach (KeyValuePair<string, byte[]> payload in payloads) WriteEntry(archive, payload.Key, payload.Value);
                }
                if (File.Exists(archivePath)) File.Replace(temporaryPath, archivePath, null);
                else File.Move(temporaryPath, archivePath);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        internal static bool IsTabSettingsPackage(string path)
        {
            using (FileStream stream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                return archive.Entries.Any(entry => String.Equals(entry.FullName, ManifestName, StringComparison.OrdinalIgnoreCase));
        }

        internal static IList<ImportedTabSettings> ReadMany(string path)
        {
            if (!IsTabSettingsPackage(path))
                throw new InvalidDataException("This settings package has no tab scope manifest.");

            var entries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (FileStream stream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string name = entry.FullName;
                    if (String.IsNullOrEmpty(name) || name.IndexOf('/') >= 0 || name.IndexOf('\\') >= 0 ||
                        name == "." || name == ".." || entries.ContainsKey(name))
                        throw new InvalidDataException("The settings package contains an unsafe or duplicate entry: " + name);
                    using (Stream input = entry.Open())
                    using (MemoryStream buffer = new MemoryStream()) { input.CopyTo(buffer); entries.Add(name, buffer.ToArray()); }
                }
            }

            byte[] manifestBytes;
            if (!entries.TryGetValue(ManifestName, out manifestBytes))
                throw new InvalidDataException("The settings package manifest is missing.");
            XDocument document;
            try { document = XDocument.Parse(Encoding.UTF8.GetString(manifestBytes)); }
            catch (Exception ex) { throw new InvalidDataException("The settings package manifest is invalid.", ex); }
            XElement root = document.Root;
            if (root == null || root.Name.LocalName != FormatName)
                throw new InvalidDataException("The settings package format is not supported.");

            string version = (string)root.Attribute("version");
            if (version == "1") return new[] { Read(path) };
            if (version != "2") throw new InvalidDataException("The settings package format is not supported.");

            XElement scopes = root.Element("Scopes");
            if (scopes == null) throw new InvalidDataException("The settings package has no tab scopes.");
            var imported = new List<ImportedTabSettings>();
            var seenScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ManifestName };
            foreach (XElement scopeElement in scopes.Elements("Scope"))
            {
                string scope = NormalizeScope((string)scopeElement.Attribute("id"));
                if (!seenScopes.Add(scope)) throw new InvalidDataException("The package lists a tab more than once: " + scope);
                string entryName = (string)scopeElement.Attribute("entry");
                string expectedFile = null;
                bool propertiesScope = PropertyScopes.Contains(scope);
                string expectedEntry = propertiesScope ? scope + ".properties.xml" :
                    (FileScopes.TryGetValue(scope, out expectedFile) ? expectedFile : null);
                if (entryName == null || !String.Equals(entryName, expectedEntry, StringComparison.OrdinalIgnoreCase) ||
                    !seenEntries.Add(entryName))
                    throw new InvalidDataException("The package entry does not match the declared tab scope: " + scope);

                byte[] contents;
                if (!entries.TryGetValue(entryName, out contents))
                    throw new InvalidDataException("The package is missing a tab payload: " + entryName);
                if (propertiesScope)
                    imported.Add(new ImportedTabSettings(scope, ReadProperties(contents), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)));
                else
                    imported.Add(new ImportedTabSettings(scope, new Dictionary<string, string>(StringComparer.Ordinal),
                        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { expectedFile, Encoding.UTF8.GetString(contents) } }));
            }

            if (imported.Count == 0 || entries.Count != seenEntries.Count)
                throw new InvalidDataException("The settings package contains missing or unreferenced tab entries.");
            return imported;
        }

        internal static ImportedTabSettings Read(string path)
        {
            string archivePath = Path.GetFullPath(path);
            var entries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (FileStream stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string name = entry.FullName;
                    if (String.IsNullOrEmpty(name) || name.IndexOf('/') >= 0 || name.IndexOf('\\') >= 0 ||
                        name == "." || name == "..")
                        throw new InvalidDataException("The settings package contains an unsafe entry name: " + name);
                    if (entries.ContainsKey(name))
                        throw new InvalidDataException("The settings package contains a duplicate entry: " + name);
                    if (name != ManifestName && name != PropertiesName &&
                        !FileScopes.Values.Contains(name, StringComparer.OrdinalIgnoreCase))
                        throw new InvalidDataException("Unexpected entry in the single-tab settings package: " + name);

                    using (Stream input = entry.Open())
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        input.CopyTo(buffer);
                        entries.Add(name, buffer.ToArray());
                    }
                }
            }

            byte[] manifestBytes;
            if (!entries.TryGetValue(ManifestName, out manifestBytes))
                throw new InvalidDataException("The tab-specific settings manifest is missing.");

            string scope = ReadManifest(manifestBytes);
            bool isPropertyScope = PropertyScopes.Contains(scope);
            var properties = new Dictionary<string, string>(StringComparer.Ordinal);
            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (isPropertyScope)
            {
                if (entries.Count != 2 || !entries.ContainsKey(PropertiesName))
                    throw new InvalidDataException("The package contents do not match its settings scope.");
                properties = ReadProperties(entries[PropertiesName]);
            }
            else
            {
                string expectedFile;
                if (!FileScopes.TryGetValue(scope, out expectedFile) || entries.Count != 2 || !entries.ContainsKey(expectedFile))
                    throw new InvalidDataException("The package contents do not match its settings scope.");
                files.Add(expectedFile, Encoding.UTF8.GetString(entries[expectedFile]));
            }

            return new ImportedTabSettings(scope, properties, files);
        }

        internal static ImportedTabSettings ReadLegacyFile(string path)
        {
            string fileName = Path.GetFileName(path);
            string contents = File.ReadAllText(path);
            if (String.Equals(fileName, "H_Config.lua", StringComparison.OrdinalIgnoreCase) || LooksLikeHConfig(contents))
                return new ImportedTabSettings("all-config", new Dictionary<string, string>(StringComparer.Ordinal),
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "H_Config.lua", contents } });
            string scope = null;
            foreach (KeyValuePair<string, string> pair in FileScopes)
                if (String.Equals(pair.Value, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    scope = pair.Key;
                    break;
                }

            if (scope == null)
                throw new InvalidDataException("This file cannot be safely routed to one settings tab. Choose a tab-specific package or a named tactics, blueprint, or extra file.");

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            files.Add(fileName, contents);
            return new ImportedTabSettings(scope, new Dictionary<string, string>(StringComparer.Ordinal), files);
        }

        static bool LooksLikeHConfig(string contents)
        {
            if (String.IsNullOrEmpty(contents)) return false;
            bool hasCoreSettings = System.Text.RegularExpressions.Regex.IsMatch(contents, @"(?m)^\s*AggroHP\s*=") &&
                                   System.Text.RegularExpressions.Regex.IsMatch(contents, @"(?m)^\s*(OldHomunType|UseAttackSkill)\s*=");
            bool hasComboSettings = System.Text.RegularExpressions.Regex.IsMatch(contents, @"(?m)^\s*ComboSlot1_SkillID\s*=") ||
                                    contents.IndexOf("KimiComboProfiles", StringComparison.Ordinal) >= 0;
            return hasCoreSettings && hasComboSettings;
        }

        internal static ImportedTabSettings ReadLegacyBundle(string path)
        {
            string directory = Path.Combine(Path.GetTempPath(), "KimiAI-Settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                // Reuse the legacy bundle's allowlist and required-file checks, but
                // point it at an isolated temporary directory instead of USER_AI.
                SettingsBundle.Import(path, directory);
                var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string fileName in new[] { "H_Config.lua", "H_Tactics.lua", "H_PVP_Tact.lua", "H_Extra.lua",
                                                     "H_Avoid.lua", "A_Friends.lua", "Defaults.lua", "Mob_ID.lua",
                                                     "twRO.lua", "combo_tactics.cbp", "ComboTactics.lua" })
                {
                    string filePath = Path.Combine(directory, fileName);
                    if (File.Exists(filePath)) files.Add(fileName, File.ReadAllText(filePath));
                }
                return new ImportedTabSettings("all-settings", new Dictionary<string, string>(StringComparer.Ordinal), files);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        static string NormalizeScope(string scope)
        {
            if (String.IsNullOrEmpty(scope)) throw new ArgumentException("A settings scope is required.", "scope");
            string normalized = scope.Trim().ToLowerInvariant();
            if (!PropertyScopes.Contains(normalized) && !FileScopes.ContainsKey(normalized))
                throw new InvalidDataException("Unknown settings scope: " + scope);
            return normalized;
        }

        static byte[] CreateManifest(string scope)
        {
            var document = new XDocument(new XElement(FormatName,
                new XAttribute("version", "1"),
                new XAttribute("scope", scope)));
            return Encoding.UTF8.GetBytes(document.ToString(SaveOptions.DisableFormatting));
        }

        static string ReadManifest(byte[] contents)
        {
            XDocument document;
            try { document = XDocument.Parse(Encoding.UTF8.GetString(contents)); }
            catch (Exception ex) { throw new InvalidDataException("The settings package manifest is invalid.", ex); }

            XElement root = document.Root;
            if (root == null || root.Name.LocalName != FormatName || (string)root.Attribute("version") != "1")
                throw new InvalidDataException("The settings package format is not supported.");
            return NormalizeScope((string)root.Attribute("scope"));
        }

        static byte[] CreateProperties(IDictionary<string, string> properties)
        {
            var root = new XElement("Properties");
            foreach (KeyValuePair<string, string> property in properties.OrderBy(p => p.Key, StringComparer.Ordinal))
                root.Add(new XElement("Property", new XAttribute("name", property.Key), new XAttribute("value", property.Value ?? String.Empty)));
            return Encoding.UTF8.GetBytes(new XDocument(root).ToString(SaveOptions.DisableFormatting));
        }

        static Dictionary<string, string> ReadProperties(byte[] contents)
        {
            XDocument document;
            try { document = XDocument.Parse(Encoding.UTF8.GetString(contents)); }
            catch (Exception ex) { throw new InvalidDataException("The property settings are invalid.", ex); }

            if (document.Root == null || document.Root.Name.LocalName != "Properties")
                throw new InvalidDataException("The property settings are invalid.");

            var properties = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (XElement element in document.Root.Elements("Property"))
            {
                string name = (string)element.Attribute("name");
                string value = (string)element.Attribute("value");
                if (String.IsNullOrEmpty(name) || value == null || properties.ContainsKey(name))
                    throw new InvalidDataException("The property settings contain a missing or duplicate property.");
                properties.Add(name, value);
            }
            return properties;
        }

        static void WriteEntry(ZipArchive archive, string name, byte[] contents)
        {
            ZipArchiveEntry entry = archive.CreateEntry(name);
            using (Stream output = entry.Open()) output.Write(contents, 0, contents.Length);
        }
    }
}
