using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AzzyAIConfig
{
    internal sealed class KimiComboProfiles
    {
        readonly KimiConf config;
        readonly Dictionary<int, Dictionary<string, object>> values = new Dictionary<int, Dictionary<string, object>>();
        readonly List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
        const string BlockPattern = @"-- BEGIN KIMI COMBO PROFILES[\s\S]*?-- END KIMI COMBO PROFILES";

        public KimiComboProfiles(KimiConf config, string path)
        {
            this.config = config;
            foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(config))
                if (p.Name.StartsWith("Combo", StringComparison.Ordinal) || IsAutoOption(p)) properties.Add(p);
            Load(path);
        }

        internal static bool IsAutoOption(PropertyDescriptor p)
        {
            return p.Category == "AutoSkill Options" || p.Category == "Autobuff Options" || p.Name == "UseAttackSkill" || p.Name == "OnlyAOE" || p.Name == "UseChaoticHeal";
        }
        internal static bool Applies(PropertyDescriptor p, int type)
        {
            // Keep legacy healing fields in saved files, but expose only Chaotic Heal controls.
            if (p.Name == "UseWarmDef" || p.Name == "WarmDefCooldown" || p.Name == "UseBodyDouble" || p.Name == "UseMasterSwap") return false;
            if (p.Name == "BastionRenewalCooldown") return type == 1;
            if (p.Name == "HealOwnerHP" || p.Name == "HealSelfHP") return false;
            if (p.Name == "UseAutoHeal") return type == 1 || type == 2;
            if (p.Name == "UseChaoticHeal") return false;
            if (p.Name == "OnlyAOE") return type == 1 || type == 2 || type == 4;
            if (p.Name.Contains("Heal")) return type == 2;
            if (p.Name.Contains("WarmDef")) return type == 1;
            if (p.Name.Contains("MasterSwap")) return type != 4;
            return true;
        }
        public void Reset()
        {
            values.Clear();
            for (int type = 1; type <= 4; type++)
            {
                var profile = new Dictionary<string, object>();
                foreach (var p in properties)
                {
                    var def = p.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
                    profile[p.Name] = def != null ? def.Value : (p.PropertyType == typeof(bool) ? (object)false : 0);
                }
                values[type] = profile;
            }
        }

        public void Load(string path)
        {
            string text = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            Reset();
            // Existing automatic settings applied to every Kimi before profiles existed.
            for (int type = 1; type <= 4; type++)
                foreach (var p in properties) if (IsAutoOption(p)) values[type][p.Name] = p.GetValue(config);
            // Older files contain one combo: retain it only for their selected Kimi.
            int selected = Convert.ToInt32(config.OldHomunType);
            if (values.ContainsKey(selected))
                foreach (var p in properties) values[selected][p.Name] = p.GetValue(config);
            foreach (Match profile in Regex.Matches(text, @"KimiComboProfiles\[(?<type>[1-4])\]\s*=\s*\{(?<body>[^}]*)\}"))
            {
                int type = int.Parse(profile.Groups["type"].Value);
                foreach (var p in properties)
                {
                    var match = Regex.Match(profile.Groups["body"].Value, @"\b" + Regex.Escape(p.Name) + @"\s*=\s*(-?\d+)");
                    int number;
                    if (match.Success && int.TryParse(match.Groups[1].Value, out number))
                        values[type][p.Name] = p.PropertyType == typeof(bool) ? (object)(number != 0) : p.PropertyType.IsEnum ? Enum.ToObject(p.PropertyType, number) : (object)number;
                }
                if (!Regex.IsMatch(profile.Groups["body"].Value, @"\bUseAutoBD\s*="))
                    values[type]["UseAutoBD"] = values[type]["UseBodyDouble"];
            }
        }

        public void ApplySelected()
        {
            int selected = Convert.ToInt32(config.OldHomunType);
            if (!values.ContainsKey(selected)) return;
            foreach (var p in properties) if (p.Name != "UseBodyDouble") p.SetValue(config, values[selected][p.Name]);
        }

        public void Save(string path)
        {
            var block = new StringBuilder("-- BEGIN KIMI COMBO PROFILES\nKimiComboProfiles = {}\n");
            for (int type = 1; type <= 4; type++)
            {
                block.Append("KimiComboProfiles[").Append(type).Append("] = {\n");
                foreach (var p in properties)
                    block.Append("  ").Append(p.Name).Append(" = ").Append(Convert.ToInt32(values[type][p.Name == "UseBodyDouble" ? "UseAutoBD" : p.Name])).Append(",\n");
                block.Append("}\n");
            }
            block.Append("-- END KIMI COMBO PROFILES");
            string text = File.ReadAllText(path);
            text = Regex.IsMatch(text, BlockPattern) ? Regex.Replace(text, BlockPattern, m => block.ToString()) : text + "\n" + block;
            File.WriteAllText(path, text);
        }

        public object ForType(int type) { return new ProfileView(this, type, false); }
        public object ForAutoSkills(int type) { return new ProfileView(this, type, true); }

        sealed class ProfileView : CustomTypeDescriptor
        {
            internal readonly KimiComboProfiles owner;
            internal readonly int type;
            readonly bool autoSkills;
            public ProfileView(KimiComboProfiles owner, int type, bool autoSkills) { this.owner = owner; this.type = type; this.autoSkills = autoSkills; }
            public override PropertyDescriptorCollection GetProperties() { return GetProperties(null); }
            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var result = new List<PropertyDescriptor>();
                foreach (var p in owner.properties)
                    if (IsAutoOption(p) == autoSkills && Applies(p, type)) result.Add(new ProfileProperty(p, type));
                return new PropertyDescriptorCollection(result.ToArray());
            }
            public override object GetPropertyOwner(PropertyDescriptor pd) { return this; }
        }

        sealed class ProfileSkillConverter : TypeConverter
        {
            readonly TypeConverter original;
            readonly int type;
            public ProfileSkillConverter(TypeConverter original, int type) { this.original = original; this.type = type; }
            public override bool GetStandardValuesSupported(ITypeDescriptorContext c) { return true; }
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext c) { return true; }
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext c)
            {
                var ids = new List<int> { 0, -1 };
                foreach (var skill in KimiSkills.GetActiveDefinitions()) if (skill.AppliesTo((KimiTypeOptions)type)) ids.Add(skill.Id);
                return new StandardValuesCollection(ids);
            }
            public override bool CanConvertFrom(ITypeDescriptorContext c, Type t) { return original.CanConvertFrom(c,t); }
            public override bool CanConvertTo(ITypeDescriptorContext c, Type t) { return original.CanConvertTo(c,t); }
            public override object ConvertFrom(ITypeDescriptorContext c, System.Globalization.CultureInfo culture, object value) { return original.ConvertFrom(c,culture,value); }
            public override object ConvertTo(ITypeDescriptorContext c, System.Globalization.CultureInfo culture, object value, Type t) { return original.ConvertTo(c,culture,value,t); }
        }
        sealed class ProfileProperty : PropertyDescriptor
        {
            readonly PropertyDescriptor original;
            readonly int kimiType;
            public ProfileProperty(PropertyDescriptor original, int type) : base(original) { this.original = original; kimiType = type; }
            public override Type ComponentType { get { return typeof(ProfileView); } }
            public override Type PropertyType { get { return original.PropertyType; } }
            public override bool IsReadOnly { get { return false; } }
            public override TypeConverter Converter { get { return Name.EndsWith("_SkillID") ? new ProfileSkillConverter(original.Converter, kimiType) : original.Converter; } }
            public override object GetValue(object component)
            { var view = (ProfileView)component; return view.owner.values[view.type][Name]; }
            public override void SetValue(object component, object value)
            {
                var view = (ProfileView)component;
                // Reuse the existing property's clamping without changing the active profile.
                object previous = original.GetValue(view.owner.config);
                try { original.SetValue(view.owner.config, value); view.owner.values[view.type][Name] = original.GetValue(view.owner.config); }
                finally { original.SetValue(view.owner.config, previous); }
                OnValueChanged(component, EventArgs.Empty);
            }
            public override bool CanResetValue(object component) { return false; }
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) { return true; }
        }
    }
}
