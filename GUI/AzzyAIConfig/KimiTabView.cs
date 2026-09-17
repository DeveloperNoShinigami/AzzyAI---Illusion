using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AzzyAIConfig
{
    /// <summary>
    /// The settings surface for one Kimi type.  It is deliberately a view over
    /// KimiConf: the values continue to have one owner and existing save,
    /// revert, import, and export code does not need a second settings model.
    /// </summary>
    public sealed class KimiTabView : UserControl
    {
        readonly KimiConf _configuration;
        readonly int _kimiTypeId;
        readonly PropertyGrid _skillsGrid;
        readonly PropertyGrid _comboGrid;
        readonly object _fallbackComboView;

        public KimiTabView(KimiConf configuration, int kimiTypeId)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (kimiTypeId < 1 || kimiTypeId > 4)
                throw new ArgumentOutOfRangeException("kimiTypeId");

            _configuration = configuration;
            _kimiTypeId = kimiTypeId;
            _skillsGrid = CreateGrid();
            _comboGrid = CreateGrid();
            _skillsGrid.SelectedObject = new FilteredSettings(configuration,
                delegate(PropertyDescriptor property) { return IsSkillOrBuffProperty(property, kimiTypeId); });
            _fallbackComboView = new FilteredSettings(configuration,
                delegate(PropertyDescriptor property) { return IsComboProperty(property); });
            _comboGrid.SelectedObject = _fallbackComboView;

            BuildLayout();
        }

        /// <summary>Numeric Kimi type used by the runtime (1..4).</summary>
        public int KimiTypeId
        {
            get { return _kimiTypeId; }
        }

        public PropertyGrid SkillsGrid
        {
            get { return _skillsGrid; }
        }

        public PropertyGrid ComboGrid
        {
            get { return _comboGrid; }
        }

        /// <summary>
        /// Replace the transitional legacy combo view with the per-type
        /// profile supplied by the owner.  The profile object is intentionally
        /// accepted as object so the profile store can evolve without making
        /// the visual control own persistence details.
        /// </summary>
        public void SetComboSettings(object comboSettings)
        {
            _comboGrid.SelectedObject = comboSettings ?? _fallbackComboView;
        }

        public static object CreateSharedSettingsView(KimiConf configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            return new FilteredSettings(configuration,
                delegate(PropertyDescriptor property) { return IsSharedProperty(property); });
        }

        public static Color GetTypeColor(int kimiTypeId)
        {
            switch (kimiTypeId)
            {
                case 1: return Color.FromArgb(181, 139, 47);   // Ward gold
                case 2: return Color.FromArgb(128, 74, 153);   // Occult purple
                case 3: return Color.FromArgb(34, 145, 145);   // Agile teal
                case 4: return Color.FromArgb(97, 139, 58);    // Raging green
                default: return SystemColors.Control;
            }
        }

        void BuildLayout()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(3);
            BackColor = GetTypeColor(_kimiTypeId);

            var sections = new TabControl();
            sections.Dock = DockStyle.Fill;
            sections.TabPages.Add(CreateSectionPage("Skills", _skillsGrid));
            sections.TabPages.Add(CreateSectionPage("Combo", _comboGrid));
            Controls.Add(sections);
        }

        static TabPage CreateSectionPage(string title, Control content)
        {
            var page = new TabPage(title);
            page.Padding = new Padding(3);
            page.UseVisualStyleBackColor = true;
            page.Controls.Add(content);
            return page;
        }

        static PropertyGrid CreateGrid()
        {
            var grid = new PropertyGrid();
            grid.Dock = DockStyle.Fill;
            grid.PropertySort = PropertySort.Categorized;
            grid.HelpVisible = true;
            grid.ToolbarVisible = false;
            return grid;
        }

        static bool IsSharedProperty(PropertyDescriptor property)
        {
            if (property == null || !property.IsBrowsable) return false;

            string category = property.Category ?? string.Empty;
            if (category.StartsWith("Kimi Skills", StringComparison.OrdinalIgnoreCase)) return false;
            if (category.StartsWith("Skill Combo Options", StringComparison.OrdinalIgnoreCase)) return false;
            if (category.StartsWith("Blueprint Combos", StringComparison.OrdinalIgnoreCase)) return false;
            if (IsComboProperty(property)) return false;
            if (IsRelatedBuffProperty(property, 1) || IsRelatedBuffProperty(property, 2) ||
                IsRelatedBuffProperty(property, 3) || IsRelatedBuffProperty(property, 4)) return false;

            // Deprecated generic heal properties have no Kimi type and remain
            // in the shared settings surface for compatibility.
            return true;
        }

        static bool IsSkillOrBuffProperty(PropertyDescriptor property, int kimiTypeId)
        {
            if (property == null || !property.IsBrowsable) return false;
            return IsSkillProperty(property, kimiTypeId) || IsRelatedBuffProperty(property, kimiTypeId) ||
                   IsComboProperty(property);
        }

        static bool IsComboProperty(PropertyDescriptor property)
        {
            if (property == null) return false;
            string category = property.Category ?? string.Empty;
            return category.StartsWith("Skill Combo Options", StringComparison.OrdinalIgnoreCase) ||
                   property.Name.StartsWith("Combo", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsSkillProperty(PropertyDescriptor property, int kimiTypeId)
        {
            string category = property.Category ?? string.Empty;
            if (!category.StartsWith("Kimi Skills", StringComparison.OrdinalIgnoreCase)) return false;

            int skillId;
            if (TryGetSkillId(property.Name, out skillId))
            {
                KimiSkillDefinition definition = KimiSkills.GetDefinition(skillId);
                return definition != null && definition.AppliesTo(ToKimiType(kimiTypeId));
            }

            // Passive skill summaries are read-only properties with no paired
            // level field. They are already grouped by type in KimiConf.
            string typeName = GetTypeName(kimiTypeId);
            return string.Equals(property.Name, typeName + "PassiveSkills", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsRelatedBuffProperty(PropertyDescriptor property, int kimiTypeId)
        {
            int skillId;
            if (!TryGetBuffSkillId(property.Name, out skillId)) return false;
            KimiSkillDefinition definition = KimiSkills.GetDefinition(skillId);
            return definition != null && definition.AppliesTo(ToKimiType(kimiTypeId));
        }

        static bool TryGetSkillId(string propertyName, out int skillId)
        {
            skillId = 0;
            if (string.IsNullOrEmpty(propertyName)) return false;
            switch (propertyName)
            {
                case "MasterSwapLevel":
                case "MasterSwapEnabled":
                    skillId = 8005; return true;
                case "IllusionOfClawsLevel":
                case "IllusionOfClawsEnabled":
                case "IllusionOfClawLevel":
                case "IllusionOfClawEnabled":
                    skillId = 8009; return true;
                case "WarmDefLevel":
                case "WarmDefEnabled":
                case "BastionRenewalLevel":
                case "BastionRenewalEnabled":
                    skillId = 8006; return true;
                case "ChaoticHealLevel":
                case "ChaoticHealEnabled":
                    skillId = 8014; return true;
                case "BodyDoubleLevel":
                case "BodyDoubleEnabled":
                    skillId = 8022; return true;
                case "IllusionOfBreathLevel":
                case "IllusionOfBreathEnabled":
                    skillId = 8024; return true;
                case "IllusionOfCrusherLevel":
                case "IllusionOfCrusherEnabled":
                case "IllusionCrusherLevel":
                case "IllusionCrusherEnabled":
                    skillId = 8031; return true;
                case "IllusionOfLightLevel":
                case "IllusionOfLightEnabled":
                    skillId = 8034; return true;
                case "WardDomainLevel":
                case "WardDomainEnabled":
                    skillId = 8033; return true;
                case "TauntLevel":
                case "TauntEnabled":
                    skillId = 8021; return true;
                case "QuickDefenseLevel":
                case "QuickDefenseEnabled":
                    skillId = 8023; return true;
                case "ChaoticSanctuaryLevel":
                case "ChaoticSanctuaryEnabled":
                    skillId = 8013; return true;
                case "ArcaneOfferingLevel":
                case "ArcaneOfferingEnabled":
                    skillId = 8015; return true;
                case "FadeAwayLevel":
                case "FadeAwayEnabled":
                    skillId = 8012; return true;
                case "MirageAssaultLevel":
                case "MirageAssaultEnabled":
                    skillId = 8036; return true;
                case "BloodSweepLevel":
                case "BloodSweepEnabled":
                    skillId = 8032; return true;
                case "OnlyAOE":
                    // OnlyAOE controls Illusion of Light's Occult build.
                    skillId = 8034; return true;
            }
            return false;
        }

        static bool TryGetBuffSkillId(string propertyName, out int skillId)
        {
            skillId = 0;
            if (string.IsNullOrEmpty(propertyName)) return false;
            if (propertyName == "UseMasterSwap" || propertyName == "MasterSwapOwnerHP" ||
                propertyName == "MasterSwapCooldown") skillId = 8005;
            else if (propertyName == "UseWarmDef" || propertyName == "WarmDefCooldown") skillId = 8006;
            else if (propertyName == "UseChaoticHeal" || propertyName == "ChaoticHealOwnerHP" ||
                     propertyName == "ChaoticHealKimiHP") skillId = 8014;
            else if (propertyName == "UseBodyDouble" || propertyName == "BodyDoubleOwnerHP") skillId = 8022;
            else return false;
            return true;
        }

        static KimiTypeOptions ToKimiType(int kimiTypeId)
        {
            switch (kimiTypeId)
            {
                case 1: return KimiTypeOptions.Ward;
                case 2: return KimiTypeOptions.Occult;
                case 3: return KimiTypeOptions.Agile;
                default: return KimiTypeOptions.Raging;
            }
        }

        static string GetTypeName(int kimiTypeId)
        {
            switch (kimiTypeId)
            {
                case 1: return "Ward";
                case 2: return "Occult";
                case 3: return "Agile";
                default: return "Raging";
            }
        }

        sealed class FilteredSettings : ICustomTypeDescriptor
        {
            readonly object _source;
            readonly Predicate<PropertyDescriptor> _include;

            public FilteredSettings(object source, Predicate<PropertyDescriptor> include)
            {
                _source = source;
                _include = include;
            }

            PropertyDescriptorCollection Filter(PropertyDescriptorCollection source)
            {
                var properties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor property in source)
                    if (_include == null || _include(property)) properties.Add(property);
                return new PropertyDescriptorCollection(properties.ToArray(), true);
            }

            public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(_source); }
            public string GetClassName() { return TypeDescriptor.GetClassName(_source); }
            public string GetComponentName() { return TypeDescriptor.GetComponentName(_source); }
            public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(_source); }
            public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(_source); }
            public PropertyDescriptor GetDefaultProperty() { return null; }
            public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(_source, editorBaseType); }
            public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(_source); }
            public EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(_source, attributes); }
            public PropertyDescriptorCollection GetProperties() { return Filter(TypeDescriptor.GetProperties(_source)); }
            public PropertyDescriptorCollection GetProperties(Attribute[] attributes) { return Filter(TypeDescriptor.GetProperties(_source, attributes)); }
            public object GetPropertyOwner(PropertyDescriptor property) { return _source; }
        }
    }
}
