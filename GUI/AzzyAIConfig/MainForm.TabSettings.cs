using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AzzyAIConfig
{
    public partial class MainForm
    {
        ToolStripMenuItem exportTabScopesMenu;
        ToolStripMenuItem exportSelectedTabsMenu;
        readonly Dictionary<string, byte[]> pendingSupportFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        void InitializeSettingsMenus()
        {
            var fileMenu = MainMenuStrip.Items["fileToolStripMenuItem"] as ToolStripMenuItem;
            if (fileMenu == null) return;
            var importMenu = fileMenu.DropDownItems["importToolStripMenuItem"] as ToolStripMenuItem;
            var exportMenu = fileMenu.DropDownItems["exportToolStripMenuItem"] as ToolStripMenuItem;
            if (importMenu != null && importMenu.DropDownItems["importAllSettingsToolStripMenuItem"] == null)
            {
                var importAll = new ToolStripMenuItem("All Settings...")
                {
                    Name = "importAllSettingsToolStripMenuItem"
                };
                importAll.Click += new EventHandler(importAllSettingsToolStripMenuItem_Click);
                importMenu.DropDownItems.Insert(0, importAll);
            }

            if (exportMenu == null || exportTabScopesMenu != null) return;
            var exportAll = new ToolStripMenuItem("All Settings...")
            {
                Name = "exportAllSettingsToolStripMenuItem"
            };
            exportAll.Click += new EventHandler(exportAllSettingsToolStripMenuItem_Click);
            exportMenu.DropDownItems.Insert(0, exportAll);

            exportTabScopesMenu = new ToolStripMenuItem("Tabs");
            AddExportScope("shared-settings", "Kimi Settings", true);
            AddExportScope("ward", "Ward", false);
            AddExportScope("occult", "Occult", false);
            AddExportScope("agile", "Agile", false);
            AddExportScope("raging", "Raging", false);
            AddExportScope("kimi-tactics", "Kimi Tactics", false);
            AddExportScope("blueprint", "Blueprint (Shared)", false);
            AddExportScope("pvp-tactics", "PVP Tactics", false);
            AddExportScope("extra", "Extra Options", false);
            exportTabScopesMenu.DropDown.Closing += delegate(object sender, ToolStripDropDownClosingEventArgs e)
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked) e.Cancel = true;
            };
            exportMenu.DropDownItems.Insert(1, exportTabScopesMenu);

            exportSelectedTabsMenu = exportMenu.DropDownItems["kimiSettingsToolStripMenuItem1"] as ToolStripMenuItem;
            if (exportSelectedTabsMenu != null)
            {
                exportSelectedTabsMenu.Text = "Export Selected Tabs...";
                exportMenu.DropDownItems.Remove(exportSelectedTabsMenu);
                exportMenu.DropDownItems.Insert(2, exportSelectedTabsMenu);
            }
        }

        void AddExportScope(string scope, string label, bool isChecked)
        {
            var item = new ToolStripMenuItem(label) { CheckOnClick = true, Checked = isChecked, Tag = scope };
            exportTabScopesMenu.DropDownItems.Add(item);
        }

        void ExportAllSettings()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "All settings package (*.zip)|*.zip";
                dialog.DefaultExt = "zip";
                dialog.FileName = "KimiAI-Settings.zip";
                dialog.Title = "Save and export all settings";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                try
                {
                    SaveChanges();
                    SettingsBundle.Export(dialog.FileName, AppDomain.CurrentDomain.BaseDirectory);
                    buttonApply.Enabled = false;
                    applySettingsToolStripMenuItem.Enabled = false;
                    revertToolStripMenuItem.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export settings:\n\n" + ex.Message,
                                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ImportAllSettingsBundle()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "All settings package (*.zip)|*.zip";
                dialog.Title = "Import all settings";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                try
                {
                    if (TabSettingsBundle.IsTabSettingsPackage(dialog.FileName))
                        throw new InvalidDataException("This package contains selected tabs. Use Import > Settings Files... to route it by scope.");
                    ImportedTabSettings bundle = TabSettingsBundle.ReadLegacyBundle(dialog.FileName);
                    ConfigChanged(this, EventArgs.Empty);
                    ApplyTabSettingsImport(bundle);
                    RefreshKimiGrids();
                    MessageBox.Show("All settings were imported and staged. Use Apply Settings to write them.",
                                    "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import all settings:\n\n" + ex.Message,
                                    "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ImportSelectedSettingsFiles()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Settings packages and files (*.zip;*.lua;*.cbp)|*.zip;*.lua;*.cbp|All files (*.*)|*.*";
                dialog.Title = "Import Settings Files";
                dialog.Multiselect = true;
                if (dialog.ShowDialog() != DialogResult.OK) return;

                int staged = 0;
                bool stagingStarted = false;
                try
                {
                    foreach (string file in dialog.FileNames)
                    {
                        string extension = Path.GetExtension(file);
                        if (String.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            if (TabSettingsBundle.IsTabSettingsPackage(file))
                            {
                                IList<ImportedTabSettings> importedTabs = TabSettingsBundle.ReadMany(file);
                                foreach (ImportedTabSettings item in importedTabs)
                                {
                                    stagingStarted = true;
                                    ConfigChanged(this, EventArgs.Empty);
                                    ApplyTabSettingsImport(item);
                                    staged++;
                                }
                            }
                            else
                            {
                                ImportedTabSettings bundle = TabSettingsBundle.ReadLegacyBundle(file);
                                stagingStarted = true;
                                ConfigChanged(this, EventArgs.Empty);
                                ApplyTabSettingsImport(bundle);
                                staged++;
                            }
                        }
                        else
                        {
                            ImportedTabSettings item = TabSettingsBundle.ReadLegacyFile(file);
                            stagingStarted = true;
                            ConfigChanged(this, EventArgs.Empty);
                            ApplyTabSettingsImport(item);
                            staged++;
                        }
                    }

                    RefreshKimiGrids();
                    MessageBox.Show(staged + " settings file(s) imported and staged. Use Apply Settings to write them.",
                                    "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (stagingStarted) ConfigChanged(this, EventArgs.Empty);
                    RefreshKimiGrids();
                    MessageBox.Show("Could not import settings:\n\n" + ex.Message +
                                    (staged > 0 ? "\n\n" + staged + " earlier file(s) remain staged. Apply is enabled." :
                                     stagingStarted ? "\n\nThe current file may have partially staged changes. Apply is enabled." : String.Empty),
                                    "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ExportSelectedTabs()
        {
            var settings = new List<ImportedTabSettings>();
            try
            {
                if (exportTabScopesMenu != null)
                    foreach (ToolStripMenuItem choice in exportTabScopesMenu.DropDownItems)
                    {
                        string scope = choice.Tag as string;
                        if (choice.Checked && scope != null) settings.Add(BuildTabSettings(scope));
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not prepare the selected tabs:\n\n" + ex.Message,
                                "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (settings.Count == 0)
            {
                MessageBox.Show("Choose one or more tabs from Export > Tabs first.", "Export Settings",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Tab settings package (*.zip)|*.zip";
                dialog.DefaultExt = "zip";
                dialog.AddExtension = true;
                dialog.FileName = "KimiAI-Selected-Tabs.zip";
                dialog.Title = "Export selected tabs";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    TabSettingsBundle.ExportMany(dialog.FileName, settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export settings:\n\n" + ex.Message,
                                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        ImportedTabSettings BuildTabSettings(string scope)
        {
            if (IsPropertyScope(scope))
                return new ImportedTabSettings(scope, ReadViewProperties(scope),
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            files.Add(GetScopeFileName(scope), GetScopeFileContents(scope));
            return new ImportedTabSettings(scope, new Dictionary<string, string>(StringComparer.Ordinal), files);
        }

        void ApplyTabSettingsImport(ImportedTabSettings imported)
        {
            if (imported.Scope == "all-config")
            {
                string configText;
                if (!imported.Files.TryGetValue("H_Config.lua", out configText))
                    throw new InvalidDataException("The imported H_Config.lua file is missing.");
                StageHConfigText(configText);
                return;
            }
            if (imported.Scope == "all-settings")
            {
                StageLegacySettings(imported.Files);
                return;
            }

            if (IsPropertyScope(imported.Scope))
            {
                ApplyViewProperties(imported.Scope, imported.Properties);
                return;
            }

            string fileName = GetScopeFileName(imported.Scope);
            string contents;
            if (!imported.Files.TryGetValue(fileName, out contents))
                throw new InvalidDataException("The imported file does not match its tab scope.");

            switch (imported.Scope)
            {
                case "kimi-tactics": kimiTactControl1.OpenText(contents); break;
                case "pvp-tactics": pvpTactControl1.OpenText(contents); break;
                case "blueprint": comboTactControl1.ImportSettingsText(contents); break;
                case "extra": extraControl1.SetSettingsText(contents); break;
                default: throw new InvalidDataException("Unknown settings scope: " + imported.Scope);
            }
        }

        void StageHConfigText(string contents)
        {
            string path = Path.Combine(Path.GetTempPath(), "KimiAI-H_Config-" + Guid.NewGuid().ToString("N") + ".lua");
            try
            {
                File.WriteAllText(path, contents ?? String.Empty);
                _kconf.Open(path);
                _comboProfiles.Load(path);
                RefreshKimiGrids();
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        void StageLegacySettings(IDictionary<string, string> files)
        {
            string config;
            if (!files.TryGetValue("H_Config.lua", out config))
                throw new InvalidDataException("The legacy all-settings bundle is missing H_Config.lua.");
            StageHConfigText(config);

            string contents;
            if (files.TryGetValue("H_Tactics.lua", out contents)) kimiTactControl1.OpenText(contents);
            if (files.TryGetValue("H_PVP_Tact.lua", out contents)) pvpTactControl1.OpenText(contents);
            if (files.TryGetValue("H_Extra.lua", out contents)) extraControl1.SetSettingsText(contents);
            if (files.TryGetValue("combo_tactics.cbp", out contents)) comboTactControl1.ImportSettingsText(contents);

            foreach (string fileName in new[] { "H_Avoid.lua", "A_Friends.lua", "Defaults.lua", "Mob_ID.lua", "twRO.lua", "ComboTactics.lua" })
                if (files.TryGetValue(fileName, out contents))
                    pendingSupportFiles[fileName] = System.Text.Encoding.UTF8.GetBytes(contents);
        }

        void ApplyPendingSupportFiles()
        {
            foreach (KeyValuePair<string, byte[]> file in pendingSupportFiles)
                File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.Key), file.Value);
        }

        void ClearPendingSupportFiles()
        {
            pendingSupportFiles.Clear();
        }

        string GetSelectedSettingsScope()
        {
            if (kimiTabs == null || kimiTabs.SelectedTab == null) return null;
            if (kimiTabs.SelectedTab.Tag is int)
            {
                switch ((int)kimiTabs.SelectedTab.Tag)
                {
                    case 1: return "ward";
                    case 2: return "occult";
                    case 3: return "agile";
                    case 4: return "raging";
                }
            }

            switch (kimiTabs.SelectedTab.Text)
            {
                case "Kimi Settings": return "shared-settings";
                case "Kimi Tactics": return "kimi-tactics";
                case "PVP Tactics": return "pvp-tactics";
                case "Blueprint (Shared)": return "blueprint";
                case "Extra Options": return "extra";
                default: return null;
            }
        }

        static bool IsPropertyScope(string scope)
        {
            return scope == "shared-settings" || scope == "ward" || scope == "occult" ||
                   scope == "agile" || scope == "raging";
        }

        Dictionary<string, string> ReadViewProperties(string scope)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (SettingsPropertyTarget target in GetPropertyTargets(scope).Values)
            {
                object value = target.Property.GetValue(target.Owner);
                values.Add(target.Property.Name, ToInvariantValue(value, target.Property.PropertyType));
            }
            return values;
        }

        void ApplyViewProperties(string scope, IDictionary<string, string> values)
        {
            Dictionary<string, SettingsPropertyTarget> targets = GetPropertyTargets(scope);
            var staged = new List<KeyValuePair<SettingsPropertyTarget, object>>();
            foreach (KeyValuePair<string, string> value in values)
            {
                SettingsPropertyTarget target;
                if (!targets.TryGetValue(value.Key, out target))
                    throw new InvalidDataException("The package contains a setting that does not belong to " + scope + ": " + value.Key);
                // Avoid setter normalization and side effects for unchanged legacy values.
                if (ToInvariantValue(target.Property.GetValue(target.Owner), target.Property.PropertyType) == value.Value)
                    continue;
                staged.Add(new KeyValuePair<SettingsPropertyTarget, object>(target,
                    ParseInvariantValue(value.Value, target.Property.PropertyType)));
            }

            foreach (KeyValuePair<SettingsPropertyTarget, object> value in staged)
                value.Key.Property.SetValue(value.Key.Owner, value.Value);
        }

        Dictionary<string, SettingsPropertyTarget> GetPropertyTargets(string scope)
        {
            var targets = new Dictionary<string, SettingsPropertyTarget>(StringComparer.Ordinal);
            foreach (object view in GetSettingsViews(scope))
            {
                ICustomTypeDescriptor descriptor = view as ICustomTypeDescriptor;
                if (descriptor == null) throw new InvalidOperationException("The settings view does not expose its property owner.");
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(view))
                {
                    if (property.IsReadOnly) continue;
                    object owner = descriptor.GetPropertyOwner(property);
                    if (owner == null) throw new InvalidOperationException("The settings property has no owner: " + property.Name);
                    if (targets.ContainsKey(property.Name))
                        throw new InvalidDataException("The settings view contains a duplicate property: " + property.Name);
                    targets.Add(property.Name, new SettingsPropertyTarget(property, owner));
                }
            }
            return targets;
        }

        IEnumerable<object> GetSettingsViews(string scope)
        {
            if (scope == "shared-settings")
            {
                yield return new KimiSettingsView(_kconf, 0);
                yield break;
            }

            int type = scope == "ward" ? 1 : scope == "occult" ? 2 : scope == "agile" ? 3 : 4;
            yield return new KimiSettingsView(_kconf, type);
            yield return _comboProfiles.ForAutoSkills(type);
            yield return _comboProfiles.ForType(type);
        }

        static string ToInvariantValue(object value, Type propertyType)
        {
            if (propertyType == typeof(bool)) return Convert.ToBoolean(value, CultureInfo.InvariantCulture) ? "1" : "0";
            if (propertyType.IsEnum) return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        static object ParseInvariantValue(string text, Type propertyType)
        {
            if (propertyType == typeof(bool))
            {
                if (text == "1" || String.Equals(text, "true", StringComparison.OrdinalIgnoreCase)) return true;
                if (text == "0" || String.Equals(text, "false", StringComparison.OrdinalIgnoreCase)) return false;
                throw new InvalidDataException("A Boolean setting must be 0 or 1.");
            }

            if (propertyType.IsEnum)
            {
                int enumValue = ParseBoundedInteger(text);
                object value = Enum.ToObject(propertyType, enumValue);
                if (!Enum.IsDefined(propertyType, value))
                    throw new InvalidDataException("A setting contains an unsupported enum value.");
                return value;
            }

            if (propertyType == typeof(string))
            {
                if (text.Length > 4096) throw new InvalidDataException("A text setting exceeds the supported length.");
                return text;
            }

            if (propertyType == typeof(int)) return ParseBoundedInteger(text);
            if (propertyType == typeof(short) || propertyType == typeof(byte) || propertyType == typeof(long))
            {
                try { return Convert.ChangeType(text, propertyType, CultureInfo.InvariantCulture); }
                catch (Exception ex) { throw new InvalidDataException("A numeric setting is invalid.", ex); }
            }
            throw new InvalidDataException("The package contains an unsupported setting type: " + propertyType.Name);
        }

        static int ParseBoundedInteger(string text)
        {
            int value;
            if (!Int32.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                throw new InvalidDataException("A numeric setting is invalid.");
            return value;
        }

        static string GetScopeFileName(string scope)
        {
            switch (scope)
            {
                case "kimi-tactics": return "H_Tactics.lua";
                case "pvp-tactics": return "H_PVP_Tact.lua";
                case "blueprint": return "combo_tactics.cbp";
                case "extra": return "H_Extra.lua";
                default: throw new InvalidDataException("Unknown settings scope: " + scope);
            }
        }

        string GetScopeFileContents(string scope)
        {
            switch (scope)
            {
                case "kimi-tactics": return kimiTactControl1.ExportText();
                case "pvp-tactics": return pvpTactControl1.ExportText();
                case "blueprint": return comboTactControl1.ExportSettingsText();
                case "extra": return extraControl1.GetSettingsText();
                default: throw new InvalidDataException("Unknown settings scope: " + scope);
            }
        }

        sealed class SettingsPropertyTarget
        {
            internal readonly PropertyDescriptor Property;
            internal readonly object Owner;

            internal SettingsPropertyTarget(PropertyDescriptor property, object owner)
            {
                Property = property;
                Owner = owner;
            }
        }
    }
}
