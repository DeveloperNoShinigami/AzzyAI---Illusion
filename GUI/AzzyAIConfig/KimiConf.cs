// KimiConf.cs

//

// Programmed by Machiavellian of iRO Chaos

//

// Description:

// This file contains the class KimiConf, which is used as a proxy to hold

// the data for H_Config, and enumerations for UseSkillOnlyOptions,

// UseAutoPushbackOptions, and UsePierceSizeOptions to restrict certain

// variables to specific values.



using System;

using System.IO;

using System.Windows.Forms;

using System.ComponentModel;

using System.Collections.Generic;





namespace AzzyAIConfig

{

    enum UseSkillOnlyOptions : sbyte

    {

        Attacking  = 0,

        Chasing = -1,

        SkillOnly = 1

    }

    enum UseAutoMagOptions : sbyte

    {

        Never = 0,

        Idle = 1,

        Chase = -1,

        Idle_Low = -2,

        Berserk = 2,

        ASAP = 3

    }

    enum UseAutoPushbackOptions : sbyte

    {

        Off = 0,

        Self = 1,

        All = 2

    }



	enum KimiTypeOptions : sbyte

	{

		Ward = 1,

		Occult = 2,

		Agile = 3,

		Raging = 4

	}

    enum UseAutoHealOptions : sbyte

	{

		Never = 0,

		Always = 1,

		Idle = 2,

		Idle_low= 3

	}





	enum AutoMobModeOptions : sbyte

	{

		Disabled = 0,

		Aggressive = 1,

		All = 2

	}

	enum AutoComboModeOptions : sbyte

	{

		Never = 0,

		Tactics = 1,

		Always = 2

	}



	enum UseIdleWalkOptions : sbyte

	{

		None = 0,

		Circle = 1,

		Cross = 2,

		Square = 3,

		Random = 4,

		Route_Linear= 5,

		Route_Circle= 6

	}



    enum StickyStandbyOptions

    {

        Disabled=0,

        Enabled=1,

        Enabled_Relog=2

    }



    // The single GUI catalog for Refuge Kimi skills. Runtime behavior and

    // timings remain in the Lua runtime; this metadata only drives the editor.

    internal sealed class KimiSkillDefinition

    {

        public int Id { get; private set; }

        public string Name { get; private set; }

        public int MaxLevel { get; private set; }

        public string Type { get; private set; }

        public string InternalName { get; private set; }

        public bool IsPassive { get; private set; }

        public bool IsSupport { get; private set; }

        public string DefaultTarget { get; private set; }

        public KimiTypeOptions[] KimiTypes { get; private set; }



        public KimiSkillDefinition(int id, string name, int maxLevel, string type,

                                   string internalName, bool isPassive, bool isSupport,

                                   string defaultTarget, params KimiTypeOptions[] kimiTypes)

        {

            Id = id;

            Name = name;

            MaxLevel = maxLevel;

            Type = type;

            InternalName = internalName;

            IsPassive = isPassive;

            IsSupport = isSupport;

            DefaultTarget = defaultTarget;

            KimiTypes = kimiTypes ?? new KimiTypeOptions[0];

        }



        public bool AppliesTo(KimiTypeOptions kimiType)

        {

            for (int i = 0; i < KimiTypes.Length; i++)

                if (KimiTypes[i] == kimiType) return true;

            return false;

        }

    }



    // Skill name mapping and level metadata used by both KimiConf and combo

    // nodes. Passive definitions intentionally never appear in SkillIds.

    static class KimiSkills

    {

        public static readonly KimiSkillDefinition[] Definitions =

        {

            new KimiSkillDefinition(8005, "Master Swap", 5, "Active / Utility", "HAMI_CASTLE", false, true, "Owner", KimiTypeOptions.Ward, KimiTypeOptions.Occult, KimiTypeOptions.Agile),

            new KimiSkillDefinition(8006, "Bastion Renewal", 5, "Active / Self Buff", "HAMI_DEFENCE", false, true, "Self", KimiTypeOptions.Ward),

            new KimiSkillDefinition(8007, "Living Bulwark", 5, "Passive", "HAMI_SKIN", true, true, "", KimiTypeOptions.Ward),

            new KimiSkillDefinition(8033, "Ward's Domain", 5, "Active / Protection Aura", "MH_STEINWAND", false, true, "Self", KimiTypeOptions.Ward),

            new KimiSkillDefinition(8039, "Retaliation", 10, "Passive / Counter AoE", "MH_MAGMA_FLOW", true, false, "", KimiTypeOptions.Ward),

            new KimiSkillDefinition(8021, "Taunt", 5, "Active / Self AoE", "MH_PAIN_KILLER", false, true, "Self", KimiTypeOptions.Ward),

            new KimiSkillDefinition(8023, "Quick Defense", 5, "Active / Self Buff", "MH_OVERED_BOOST", false, true, "Self", KimiTypeOptions.Ward, KimiTypeOptions.Occult, KimiTypeOptions.Agile, KimiTypeOptions.Raging),

            new KimiSkillDefinition(8022, "Body Double", 5, "Active / Self Buff", "MH_LIGHT_OF_REGENE", false, true, "Self", KimiTypeOptions.Ward, KimiTypeOptions.Occult, KimiTypeOptions.Agile, KimiTypeOptions.Raging),



            new KimiSkillDefinition(8014, "Chaotic Heal", 5, "Active / Recovery", "HVAN_CHAOTIC", false, true, "Self", KimiTypeOptions.Occult),

            new KimiSkillDefinition(8024, "Illusion of Breath", 10, "Active / Neutral Magic", "MH_ERASER_CUTTER", false, false, "Enemy", KimiTypeOptions.Occult),

            new KimiSkillDefinition(8034, "Illusion of Light", 5, "Active / Neutral Magic AoE", "MH_HEILIGE_STANGE", false, false, "Enemy", KimiTypeOptions.Occult),

            new KimiSkillDefinition(8013, "Chaotic Sanctuary", 5, "Active / Ground Support", "HVAN_CAPRICE", false, true, "Ground", KimiTypeOptions.Occult),

            new KimiSkillDefinition(8015, "Arcane Offering", 5, "Active / Support", "HVAN_INSTRUCT", false, true, "Self", KimiTypeOptions.Occult),



            new KimiSkillDefinition(8009, "Illusion of Claw", 10, "Active / Melee", "HFLI_MOON", false, false, "Enemy", KimiTypeOptions.Agile),

            new KimiSkillDefinition(8010, "Enhanced Reflexes", 5, "Passive", "HFLI_FLEET", true, true, "", KimiTypeOptions.Agile),

            new KimiSkillDefinition(8011, "Second Wind", 10, "Passive", "HFLI_SPEED", true, true, "", KimiTypeOptions.Agile),

            new KimiSkillDefinition(8012, "Fade Away", 1, "Active / Utility", "HFLI_SBR44", false, true, "Self", KimiTypeOptions.Agile),

            new KimiSkillDefinition(8036, "Mirage Assault", 5, "Active / Targeted Melee", "MH_TINDER_BREAKER", false, false, "Enemy", KimiTypeOptions.Agile),



            new KimiSkillDefinition(8031, "Illusion Crusher", 5, "Active / Physical", "MH_STAHL_HORN", false, false, "Enemy", KimiTypeOptions.Raging),

            new KimiSkillDefinition(8032, "Blood Sweep", 10, "Active / Melee AoE", "MH_GOLDENE_FERSE", false, false, "Enemy", KimiTypeOptions.Raging),

            new KimiSkillDefinition(8035, "Blood Hunger", 10, "Passive / Blood Sweep Modifier", "MH_ANGRIFFS_MODUS", true, false, "", KimiTypeOptions.Raging),

            new KimiSkillDefinition(8037, "Predator's Focus", 1, "Passive", "MH_CBC", true, true, "", KimiTypeOptions.Raging),

            new KimiSkillDefinition(8038, "Blood Frenzy", 5, "Passive", "MH_EQC", true, true, "", KimiTypeOptions.Raging),

            new KimiSkillDefinition(8040, "Ironblood", 5, "Passive", "MH_GRANITIC_ARMOR", true, true, "", KimiTypeOptions.Raging)

        };



        public static readonly int[] SkillIds = BuildSkillIds();

        public static readonly string[] SkillNames = BuildSkillNames();



        static int[] BuildSkillIds()

        {

            var ids = new List<int> { 0, -1 };

            foreach (var skill in Definitions)

                if (!skill.IsPassive) ids.Add(skill.Id);

            return ids.ToArray();

        }



        static string[] BuildSkillNames()

        {

            var names = new List<string> { "(None)", "Basic Attack / Auto-Attack (-1)" };

            foreach (var skill in Definitions)

                if (!skill.IsPassive) names.Add(skill.Name + " (" + skill.Id + ")");

            return names.ToArray();

        }



        public static IEnumerable<KimiSkillDefinition> GetActiveDefinitions()

        {

            foreach (var skill in Definitions)

                if (!skill.IsPassive) yield return skill;

        }



        public static IEnumerable<KimiSkillDefinition> GetPassiveDefinitions(KimiTypeOptions kimiType)

        {

            foreach (var skill in Definitions)

                if (skill.IsPassive && skill.AppliesTo(kimiType)) yield return skill;

        }



        public static KimiSkillDefinition GetDefinition(int skillId)

        {

            foreach (var skill in Definitions)

                if (skill.Id == skillId) return skill;

            return null;

        }



        public static int GetMaxLevel(int skillId)

        {

            var skill = GetDefinition(skillId);

            return skill == null ? 1 : skill.MaxLevel;

        }



        public static int ClampLevel(int skillId, int level)

        {

            int max = GetMaxLevel(skillId);

            if (level < 1) return 1;

            if (level > max) return max;

            return level;

        }



        public static string GetDefaultTarget(int skillId)

        {

            var skill = GetDefinition(skillId);

            return skill == null || string.IsNullOrEmpty(skill.DefaultTarget) ? "Enemy" : skill.DefaultTarget;

        }



        public static string GetSkillName(int skillId)

        {

            for (int i = 0; i < SkillIds.Length; i++)

                if (SkillIds[i] == skillId) return SkillNames[i];

            var skill = GetDefinition(skillId);

            return skill == null ? "(Unknown)" : skill.Name + " (" + skill.Id + ")";

        }



        public static int GetSkillId(string skillName)

        {

            if (string.IsNullOrEmpty(skillName)) return 0;

            for (int i = 0; i < SkillNames.Length; i++)

                if (string.Equals(SkillNames[i], skillName, StringComparison.OrdinalIgnoreCase)) return SkillIds[i];



            // Accept the old generated names and title-only node names when

            // opening an existing blueprint.

            string title = skillName;

            int suffix = title.IndexOf(" (", StringComparison.Ordinal);

            if (suffix > 0) title = title.Substring(0, suffix);

            if (string.Equals(title, "Illusion of Claws", StringComparison.OrdinalIgnoreCase)) title = "Illusion of Claw";

            if (string.Equals(title, "Warm Def", StringComparison.OrdinalIgnoreCase)) title = "Bastion Renewal";

            foreach (var skill in Definitions)

                if (string.Equals(skill.Name, title, StringComparison.OrdinalIgnoreCase)) return skill.Id;

            if (string.Equals(skillName, "Basic Attack / Auto-Attack", StringComparison.OrdinalIgnoreCase)) return -1;

            return 0;

        }

    }



    // TypeConverter for skill dropdown selection

    class SkillIdConverter : TypeConverter

    {

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)

        {

            return true;

        }



        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)

        {

            return true;

        }



        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)

        {

            // Return the actual skill IDs as the standard values (not names)

            return new StandardValuesCollection(KimiSkills.SkillIds);

        }



        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)

        {

            if (destinationType == typeof(string) && value is int)

            {

                return KimiSkills.GetSkillName((int)value);

            }

            return base.ConvertTo(context, culture, value, destinationType);

        }



        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)

        {

            if (value is string)

            {

                return KimiSkills.GetSkillId((string)value);

            }

            return base.ConvertFrom(context, culture, value);

        }



        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)

        {

            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);

        }



        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)

        {

            if (destinationType == typeof(string)) return true;

            return base.CanConvertTo(context, destinationType);

        }

    }



    class KimiConf

    {

        // The default file

        string _file = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "H_Config.lua");

        // Preserve the complete Lua source used to populate this object so

        // settings this GUI does not expose survive Apply and named exports.

        string _configTemplate = string.Empty;

        readonly Dictionary<int, int> _kimiSkillLevels = new Dictionary<int, int>();

        readonly Dictionary<int, bool> _kimiSkillEnabled = new Dictionary<int, bool>();



        void ResetKimiSkillSettings()

        {

            _kimiSkillLevels.Clear();

            _kimiSkillEnabled.Clear();

            foreach (var skill in KimiSkills.GetActiveDefinitions())

            {

                _kimiSkillLevels[skill.Id] = 1;

                _kimiSkillEnabled[skill.Id] = false;

            }

        }



        void InitKimiSkillSettings()

        {

            ResetKimiSkillSettings();

            foreach (var skill in KimiSkills.GetActiveDefinitions())

            {

                _kimiSkillLevels[skill.Id] = KimiSkills.ClampLevel(skill.Id, H_Config.GetKimiSkillLevel(skill.Id));

                _kimiSkillEnabled[skill.Id] = H_Config.GetKimiSkillEnabled(skill.Id);

            }

        }



        int GetKimiSkillLevel(int skillId)

        {

            int level;

            return _kimiSkillLevels.TryGetValue(skillId, out level) ?

                KimiSkills.ClampLevel(skillId, level) : 1;

        }



        bool GetKimiSkillEnabled(int skillId)

        {

            bool enabled;

            return _kimiSkillEnabled.TryGetValue(skillId, out enabled) && enabled;

        }



        void SetKimiSkillLevel(int skillId, int level)

        {

            if (KimiSkills.GetDefinition(skillId) == null || KimiSkills.GetDefinition(skillId).IsPassive) return;

            _kimiSkillLevels[skillId] = KimiSkills.ClampLevel(skillId, level);

        }



        void SetKimiSkillEnabled(int skillId, bool enabled)

        {

            if (KimiSkills.GetDefinition(skillId) == null || KimiSkills.GetDefinition(skillId).IsPassive) return;

            _kimiSkillEnabled[skillId] = enabled;

        }



        int GetLegacyKimiLevel(int skillId)

        {

            return GetKimiSkillEnabled(skillId) ? GetKimiSkillLevel(skillId) : 0;

        }



        void SyncLegacyKimiLevels()

        {

            _IllusionOfClawsLevel = GetLegacyKimiLevel(8009);

            _IllusionOfBreathLevel = GetLegacyKimiLevel(8024);

            _IllusionOfCrusherLevel = GetLegacyKimiLevel(8031);

            _IllusionOfLightLevel = GetLegacyKimiLevel(8034);

            _ChaoticHealLevel = GetLegacyKimiLevel(8014);

            _BodyDoubleLevel = GetLegacyKimiLevel(8022);

            _WarmDefLevel = GetLegacyKimiLevel(8006);

        }



        void SaveKimiSkillSettingsToConfig()

        {

            SyncLegacyKimiLevels();

            H_Config.SetKimiSkillSettings(_kimiSkillLevels, _kimiSkillEnabled);

        }



        string FormatPassiveSkills(KimiTypeOptions kimiType)

        {

            var names = new List<string>();

            foreach (var skill in KimiSkills.GetPassiveDefinitions(kimiType))

                names.Add(skill.Name + " (" + skill.Id + ") - Passive, max level " + skill.MaxLevel);

            return names.Count == 0 ? "None" : string.Join(", ", names.ToArray());

        }



        public KimiConf()

        {

            // Check if the file does not exist

            if (!File.Exists(_file))

            {

                // Create default H_Config with hardcoded defaults, then save it

                H_Config.SetDefaults();

                H_Config.Save(_file);

            }

            else

            {

                // Load configurations from the file

                H_Config.Load(_file);

            }



            _configTemplate = File.ReadAllText(_file);



            // Initialize this object's values

            InitValues();

        }



        public KimiConf(string file)

        {

            // Set the file path

            _file = file;

            

            // Check if the file does not exist

            if (!File.Exists(file))

            {

                // Create default H_Config with hardcoded defaults, then save it

                H_Config.SetDefaults();

                H_Config.Save(_file);

            }

            else

            {

                // Load configurations from the file

                H_Config.Load(_file);

            }



            _configTemplate = File.ReadAllText(_file);



            // Initialize the values for this object

            InitValues();

        }



        public void Revert()

        {

            // Reload the configurations from the file

            H_Config.Load(_file);

            _configTemplate = File.ReadAllText(_file);



            // Reinitialize the values for this object

            InitValues();

        }



        public void SetDefaults()

        {

            // Basic Options

            _AggroHP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AggroHP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AggroSP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AggroSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _KiteMonsters = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteMonsters"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _SuperPassive = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["SuperPassive"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseAttackSkill = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseAttackSkill"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AssumeHomun = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AssumeHomun"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _DoNotChase = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["DoNotChase"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseDanceAttack = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseDanceAttack"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseAvoid = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseAvoid"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _TankMonsterLimit = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["TankMonsterLimit"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _RescueOwnerLowHP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["RescueOwnerLowHP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _StationaryAggroDist = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["StationaryAggroDist"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _MobileAggroDist = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["MobileAggroDist"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _OldHomunType = (KimiTypeOptions)((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["OldHomunType"].Attributes[typeof(DefaultValueAttribute)]).Value;

            _OpportunisticTargeting = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["OpportunisticTargeting"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AttackLastFullSP = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AttackLastFullSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _DanceMinSP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["DanceMinSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AttackTimeLimit = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AttackTimeLimit"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _LagReduction = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["LagReduction"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _DoNotAttackMoving = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["DoNotAttackMoving"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _LiveMobID = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["LiveMobID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            EnableDebugLogging = false;

            NavigationMap = string.Empty;

            



            // AutoSkill Options

            _AttackSkillReserveSP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AttackSkillReserveSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseSkillOnly = (UseSkillOnlyOptions)((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseSkillOnly"].Attributes[typeof(DefaultValueAttribute)]).Value;

            _AutoSkillDelay = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AutoSkillDelay"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AutoMobMode = (AutoMobModeOptions)((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AutoMobMode"].Attributes[typeof(DefaultValueAttribute)]).Value;

            _AutoMobCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AutoMobCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AoEReserveSP = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AoEReserveSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _AoEFixedLevel = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["AoEFixedLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _IllusionOfClawsLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["IllusionOfClawsLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _IllusionOfBreathLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["IllusionOfBreathLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _IllusionOfCrusherLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["IllusionOfCrusherLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _IllusionOfLightLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["IllusionOfLightLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ChaoticHealLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ChaoticHealLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _BodyDoubleLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["BodyDoubleLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            WarmDefHP = 100;

            _WarmDefLevel = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["WarmDefLevel"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _OnlyAOE = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["OnlyAOE"].Attributes[typeof(DefaultValueAttribute)]).Value);



            // Walk/Follow Options

            _FollowStayBack = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["FollowStayBack"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _RestXOff = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["RestXOff"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _RestYOff = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["RestYOff"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _DoNotUseRest = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["DoNotUseRest"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _SpawnDelay = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["SpawnDelay"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _MoveSticky = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["MoveSticky"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _MoveStickyFight = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["MoveStickyFight"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseIdleWalk = (UseIdleWalkOptions)((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseIdleWalk"].Attributes[typeof(DefaultValueAttribute)]).Value;

            _IdleWalkSP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["IdleWalkSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseCastleRoute = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseCastleRoute"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _RelativeRoute = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["RelativeRoute"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _IdleWalkDistance = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["IdleWalkDistance"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ChaseSPPause = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ChaseSPPause"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ChaseSPPauseSP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ChaseSPPauseSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ChaseSPPauseTime = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ChaseSPPauseTime"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _StationaryMoveBounds = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["StationaryMoveBounds"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _MobileMoveBounds = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["MobileMoveBounds"].Attributes[typeof(DefaultValueAttribute)]).Value);



            // Autobuff Options (Kimi-only)

            _HealOwnerHP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["HealOwnerHP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _HealSelfHP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["HealSelfHP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseAutoHeal = (UseAutoHealOptions)((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseAutoHeal"].Attributes[typeof(DefaultValueAttribute)]).Value;

            // Kiting Options

            _KiteParanoid = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteParanoid"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _KiteStep = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteStep"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _KiteParanoidStep = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteParanoidStep"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _KiteThreshold = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteThreshold"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _KiteParanoidThreshold = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteParanoidThreshold"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _KiteBounds = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["KiteBounds"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ForceKite = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ForceKite"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _FleeHP = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["FleeHP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            

            // Friending Options

            _StandbyFriending = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["StandbyFriending"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _MirAIFriending = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["MirAIFriending"].Attributes[typeof(DefaultValueAttribute)]).Value);



            // Standby Options

            _DefendStandby = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["DefendStandby"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _StickyStandby = (StickyStandbyOptions)((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["StickyStandby"].Attributes[typeof(DefaultValueAttribute)]).Value;

            // Berserk Options

            _UseBerserkMobbed = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseBerserkMobbed"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseBerserkSkill = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseBerserkSkill"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _UseBerserkAttack = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["UseBerserkAttack"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _Berserk_SkillAlways = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["Berserk_SkillAlways"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _Berserk_Dance = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["Berserk_Dance"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _Berserk_IgnoreMinSP = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["Berserk_IgnoreMinSP"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _Berserk_ComboAlways = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["Berserk_ComboAlways"].Attributes[typeof(DefaultValueAttribute)]).Value);



            // Skill Combo Options

            _ComboEnabled = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboEnabled"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _BlueprintComboEnabled = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["BlueprintComboEnabled"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboRunDuringChase = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboRunDuringChase"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboRunDuringAttack = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboRunDuringAttack"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboRunDuringIdle = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboRunDuringIdle"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboResetOnTargetChange = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboResetOnTargetChange"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboAutoAttackDelay = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboAutoAttackDelay"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot1_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot1_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot1_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot1_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot2_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot2_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot2_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot2_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot3_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot3_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot3_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot3_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot4_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot4_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot4_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot4_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot5_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot5_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot5_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot5_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot6_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot6_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot6_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot6_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot7_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot7_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot7_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot7_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot8_SkillID = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot8_SkillID"].Attributes[typeof(DefaultValueAttribute)]).Value);

            _ComboSlot8_ComboCount = Convert.ToInt32(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["ComboSlot8_ComboCount"].Attributes[typeof(DefaultValueAttribute)]).Value);



            // PVP Options

            _PVPmode = Convert.ToBoolean(((DefaultValueAttribute)TypeDescriptor.GetProperties(this)["PVPmode"].Attributes[typeof(DefaultValueAttribute)]).Value);



            // All active skills use an explicit enabled flag. New skills start

            // at level 1 but remain disabled until the user enables them.

            ResetKimiSkillSettings();

        }



        public void Save()

        {

            // Save the file

            Save(_file);

        }



        public void Save(string file)

        {

            // Ensure target file exists and is writable; close handle immediately to avoid lock

            if (!File.Exists(file))

            {

                using (var writer = File.CreateText(file))

                {

                    writer.Write(string.Empty);

                }

            }



            SaveKimiSkillSettingsToConfig();



                    H_Config.FriendAssistDamageHPThresholdOwner = Convert.ToInt32(_FriendAssistDamageHPThresholdOwner);



            // Basic Options

            H_Config.AggroHP = _AggroHP;

            H_Config.AggroSP = _AggroSP;

            H_Config.KiteMonsters = Convert.ToInt32(_KiteMonsters);

            H_Config.SuperPassive = Convert.ToInt32(_SuperPassive);

            H_Config.UseAttackSkill = Convert.ToInt32(_UseAttackSkill);

            H_Config.AssumeHomun = Convert.ToInt32(_AssumeHomun);

            H_Config.DoNotChase = Convert.ToInt32(_DoNotChase);

            H_Config.UseDanceAttack = Convert.ToInt32(_UseDanceAttack);

            H_Config.UseAvoid = Convert.ToInt32(_UseAvoid);

            H_Config.TankMonsterLimit = _TankMonsterLimit;

            H_Config.RescueOwnerLowHP = _RescueOwnerLowHP;

            H_Config.StationaryAggroDist = _StationaryAggroDist;

            H_Config.MobileAggroDist = _MobileAggroDist;

            H_Config.OldHomunType = Convert.ToInt32(_OldHomunType);

            H_Config.OpportunisticTargeting = Convert.ToInt32(_OpportunisticTargeting);

            H_Config.AttackLastFullSP = Convert.ToInt32(_AttackLastFullSP);

            H_Config.DanceMinSP = _DanceMinSP;

            H_Config.AttackTimeLimit = _AttackTimeLimit;

            H_Config.LagReduction = Convert.ToInt32(_LagReduction);

            H_Config.DoNotAttackMoving = Convert.ToInt32(_DoNotAttackMoving);

            H_Config.LiveMobID = Convert.ToInt32(_LiveMobID);

            H_Config.EnableDebugLogging = EnableDebugLogging ? 1 : 0;

            H_Config.NavigationMap = _NavigationMap;





            // AutoSkill Options

            H_Config.AttackSkillReserveSP = _AttackSkillReserveSP;

            H_Config.UseSkillOnly = Convert.ToInt32(_UseSkillOnly);

            H_Config.AutoSkillDelay = _AutoSkillDelay;

            H_Config.AutoMobMode = Convert.ToInt32(_AutoMobMode);

            H_Config.AutoMobCount = _AutoMobCount;

            H_Config.AoEReserveSP = Convert.ToInt32(_AoEReserveSP);

            H_Config.AoEFixedLevel = Convert.ToInt32(_AoEFixedLevel);

            H_Config.illusionOfClawsLevel = _IllusionOfClawsLevel;

            H_Config.illusionOfBreathLevel = _IllusionOfBreathLevel;

            H_Config.illusionOfCrusherLevel = _IllusionOfCrusherLevel;

            H_Config.illusionOfLightLevel = _IllusionOfLightLevel;

            H_Config.chaoticHealLevel = _ChaoticHealLevel;

            H_Config.bodyDoubleLevel = _BodyDoubleLevel;

            H_Config.warmDefLevel = _WarmDefLevel;

            H_Config.onlyAOE = Convert.ToInt32(_OnlyAOE);



            // Walk/Follow Options

            H_Config.FollowStayBack = _FollowStayBack;

            H_Config.RestXOff = _RestXOff;

            H_Config.RestYOff = _RestYOff;

            H_Config.DoNotUseRest = Convert.ToInt32(_DoNotUseRest);

            H_Config.SpawnDelay = _SpawnDelay;

            H_Config.MoveSticky = Convert.ToInt32(_MoveSticky);

            H_Config.MoveStickyFight = Convert.ToInt32(_MoveStickyFight);

            H_Config.UseIdleWalk = Convert.ToInt32(_UseIdleWalk);

            H_Config.IdleWalkSP = _IdleWalkSP;

            H_Config.UseCastleRoute = Convert.ToInt32(_UseCastleRoute);

            H_Config.RelativeRoute = Convert.ToInt32(_RelativeRoute);

            H_Config.IdleWalkDistance = _IdleWalkDistance;

            H_Config.ChaseSPPause = Convert.ToInt32(_ChaseSPPause);

            H_Config.ChaseSPPauseSP = _ChaseSPPauseSP;

            H_Config.ChaseSPPauseTime = _ChaseSPPauseTime;

            H_Config.StationaryMoveBounds = _StationaryMoveBounds;

            H_Config.MobileMoveBounds = _MobileMoveBounds;



            // Autobuff Options (Kimi-only)

            // Deprecated generic heal thresholds (HealSelfHP/HealOwnerHP/UseAutoHeal) are no longer saved.

            H_Config.UseChaoticHeal = _UseChaoticHeal;

            H_Config.ChaoticHealOwnerHP = _ChaoticHealOwnerHP;

            H_Config.ChaoticHealKimiHP = _ChaoticHealKimiHP;

            H_Config.UseBodyDouble = _UseBodyDouble;

            H_Config.BodyDoubleCooldown = BodyDoubleCooldown;

            H_Config.BastionRenewalCooldown = BastionRenewalCooldown;

            H_Config.BodyDoubleOwnerHP = _BodyDoubleOwnerHP;

            H_Config.UseWarmDef = _UseWarmDef;

            H_Config.WarmDefCooldown = _WarmDefCooldown;

            H_Config.WarmDefHP = WarmDefHP;

            H_Config.UseMasterSwap = _UseMasterSwap;

            H_Config.MasterSwapOwnerHP = _MasterSwapOwnerHP;

            H_Config.MasterSwapCooldown = _MasterSwapCooldown;

            // Kiting Options

            H_Config.KiteParanoid = Convert.ToInt32(_KiteParanoid);

            H_Config.KiteStep = _KiteStep;

            H_Config.KiteParanoidStep = _KiteParanoidStep;

            H_Config.KiteThreshold = _KiteThreshold;

            H_Config.KiteParanoidThreshold = _KiteParanoidThreshold;

            H_Config.KiteBounds = _KiteBounds;

            H_Config.ForceKite = Convert.ToInt32(_ForceKite);

            H_Config.FleeHP = _FleeHP;



            // Friending Options

            H_Config.StandbyFriending = Convert.ToInt32(_StandbyFriending);

            H_Config.MirAIFriending = Convert.ToInt32(_MirAIFriending);



            // Standby Options

            H_Config.DefendStandby = Convert.ToInt32(_DefendStandby);

            H_Config.StickyStandby = Convert.ToInt32(_StickyStandby);



            // Berserk Options

            H_Config.UseBerserkMobbed = _UseBerserkMobbed;

            H_Config.UseBerserkSkill = Convert.ToInt32(_UseBerserkSkill);

            H_Config.UseBerserkAttack = Convert.ToInt32(_UseBerserkAttack);

            H_Config.Berserk_SkillAlways = Convert.ToInt32(_Berserk_SkillAlways);

            H_Config.Berserk_Dance = Convert.ToInt32(_Berserk_Dance);

            H_Config.Berserk_IgnoreMinSP = Convert.ToInt32(_Berserk_IgnoreMinSP);

            H_Config.Berserk_ComboAlways = Convert.ToInt32(_Berserk_ComboAlways);

            

            // Skill Combo Options

            H_Config.ComboEnabled = Convert.ToInt32(_ComboEnabled);

            H_Config.BlueprintComboEnabled = Convert.ToInt32(_BlueprintComboEnabled);

            H_Config.ComboRunDuringChase = Convert.ToInt32(_ComboRunDuringChase);

            H_Config.ComboRunDuringAttack = Convert.ToInt32(_ComboRunDuringAttack);

            H_Config.ComboRunDuringIdle = Convert.ToInt32(_ComboRunDuringIdle);

            H_Config.ComboResetOnTargetChange = Convert.ToInt32(_ComboResetOnTargetChange);

            H_Config.ComboAutoAttackDelay = _ComboAutoAttackDelay;

            H_Config.ComboSlot1_SkillID = _ComboSlot1_SkillID;

            H_Config.ComboSlot1_ComboCount = _ComboSlot1_ComboCount;

            H_Config.ComboSlot2_SkillID = _ComboSlot2_SkillID;

            H_Config.ComboSlot2_ComboCount = _ComboSlot2_ComboCount;

            H_Config.ComboSlot3_SkillID = _ComboSlot3_SkillID;

            H_Config.ComboSlot3_ComboCount = _ComboSlot3_ComboCount;

            H_Config.ComboSlot4_SkillID = _ComboSlot4_SkillID;

            H_Config.ComboSlot4_ComboCount = _ComboSlot4_ComboCount;

            H_Config.ComboSlot5_SkillID = _ComboSlot5_SkillID;

            H_Config.ComboSlot5_ComboCount = _ComboSlot5_ComboCount;

            H_Config.ComboSlot6_SkillID = _ComboSlot6_SkillID;

            H_Config.ComboSlot6_ComboCount = _ComboSlot6_ComboCount;

            H_Config.ComboSlot7_SkillID = _ComboSlot7_SkillID;

            H_Config.ComboSlot7_ComboCount = _ComboSlot7_ComboCount;

            H_Config.ComboSlot8_SkillID = _ComboSlot8_SkillID;

            H_Config.ComboSlot8_ComboCount = _ComboSlot8_ComboCount;

            

            // PVP Options

            H_Config.PVPmode = Convert.ToInt32(_PVPmode);



            // Save the file

            H_Config.Save(file, _configTemplate);



            // Named exports must not change the active file or its source

            // template. Refresh the template only after an Apply save.

            if (string.Equals(System.IO.Path.GetFullPath(file),

                              System.IO.Path.GetFullPath(_file),

                              StringComparison.OrdinalIgnoreCase))

            {

                _configTemplate = File.ReadAllText(_file);

            }

        }



        public void Open(string file)

        {

            // Check if the file does not exist

            if (!File.Exists(file))

            {

                // Throw a new file not found exception

                throw new FileNotFoundException("The specified file could not be found.", _file);

            }



            // Load configurations from the file

            string source = File.ReadAllText(file);

            H_Config.Load(file);

            _configTemplate = source;



            // Initialize the values for this object

            InitValues();

        }



        void InitValues()

        {

            // Basic Options

            _AggroHP = H_Config.AggroHP;

            _AggroSP = H_Config.AggroSP; 

            _KiteMonsters = Convert.ToBoolean(H_Config.KiteMonsters);

            _SuperPassive = Convert.ToBoolean(H_Config.SuperPassive);

            _UseAttackSkill = Convert.ToBoolean(H_Config.UseAttackSkill);

            _AssumeHomun = Convert.ToBoolean(H_Config.AssumeHomun);

            _DoNotChase = Convert.ToBoolean(H_Config.DoNotChase);

            _UseDanceAttack = Convert.ToBoolean(H_Config.UseDanceAttack);

            _UseAvoid = Convert.ToBoolean(H_Config.UseAvoid);

            _TankMonsterLimit = H_Config.TankMonsterLimit;

            _RescueOwnerLowHP = H_Config.RescueOwnerLowHP;

            _StationaryAggroDist = H_Config.StationaryAggroDist;

            _MobileAggroDist = H_Config.MobileAggroDist;

            _OldHomunType = (KimiTypeOptions)H_Config.OldHomunType;

            _OpportunisticTargeting = Convert.ToBoolean(H_Config.OpportunisticTargeting);

            _AttackLastFullSP = Convert.ToBoolean(H_Config.AttackLastFullSP);

            _DanceMinSP = H_Config.DanceMinSP;

            _AttackTimeLimit = H_Config.AttackTimeLimit;

            _LagReduction = H_Config.LagReduction;

            _DoNotAttackMoving = Convert.ToBoolean(H_Config.DoNotAttackMoving);

            _LiveMobID = Convert.ToBoolean(H_Config.LiveMobID);

            EnableDebugLogging = H_Config.EnableDebugLogging == 1;

            NavigationMap = H_Config.NavigationMap;



            // AutoSkill Options

            _AttackSkillReserveSP = H_Config.AttackSkillReserveSP;

            _FriendAssistDamageHPThresholdOwner = Convert.ToInt32(H_Config.FriendAssistDamageHPThresholdOwner);

            _AutoSkillDelay = H_Config.AutoSkillDelay;

            _AutoMobMode = (AutoMobModeOptions)H_Config.AutoMobMode;

            _AoEReserveSP = Convert.ToBoolean(H_Config.AoEReserveSP);

            _AoEFixedLevel = Convert.ToBoolean(H_Config.AoEFixedLevel);

            _AutoMobCount = H_Config.AutoMobCount;

            _IllusionOfClawsLevel = H_Config.illusionOfClawsLevel;

            _IllusionOfBreathLevel = H_Config.illusionOfBreathLevel;

            _IllusionOfCrusherLevel = H_Config.illusionOfCrusherLevel;

            _IllusionOfLightLevel = H_Config.illusionOfLightLevel;

            _ChaoticHealLevel = H_Config.chaoticHealLevel;

            _BodyDoubleLevel = H_Config.bodyDoubleLevel;

            _WarmDefLevel = H_Config.warmDefLevel;

            _OnlyAOE = Convert.ToBoolean(H_Config.onlyAOE);

            InitKimiSkillSettings();



            // Walk/Follow Options

            _FollowStayBack = H_Config.FollowStayBack;

            _RestXOff = H_Config.RestXOff;

            _RestYOff = H_Config.RestYOff;

            _DoNotUseRest = Convert.ToBoolean(H_Config.DoNotUseRest);

            _SpawnDelay = H_Config.SpawnDelay;

            _MoveSticky = Convert.ToBoolean(H_Config.MoveSticky);

            _MoveStickyFight = Convert.ToBoolean(H_Config.MoveStickyFight);

            _UseIdleWalk = (UseIdleWalkOptions)H_Config.UseIdleWalk;

            _UseCastleRoute = Convert.ToBoolean(H_Config.UseCastleRoute);

            _RelativeRoute = Convert.ToBoolean(H_Config.RelativeRoute);

            _IdleWalkDistance = H_Config.IdleWalkDistance;

            _ChaseSPPause = Convert.ToBoolean(H_Config.ChaseSPPause);

            _ChaseSPPauseSP = H_Config.ChaseSPPauseSP;

            _ChaseSPPauseTime = H_Config.ChaseSPPauseTime;

            _StationaryMoveBounds = H_Config.StationaryMoveBounds;

            _MobileMoveBounds = H_Config.MobileMoveBounds;

            _IdleWalkSP = H_Config.IdleWalkSP;



            // Autobuff Options (Kimi-only)

            _HealOwnerHP = H_Config.HealOwnerHP;

            _HealSelfHP = H_Config.HealSelfHP;

            _UseAutoHeal = (UseAutoHealOptions)H_Config.UseAutoHeal;

            _UseChaoticHeal = H_Config.UseChaoticHeal;

            _ChaoticHealOwnerHP = H_Config.ChaoticHealOwnerHP;

            _ChaoticHealKimiHP = H_Config.ChaoticHealKimiHP;

            _UseBodyDouble = H_Config.UseBodyDouble;

            BodyDoubleCooldown = H_Config.BodyDoubleCooldown;

            BastionRenewalCooldown = H_Config.BastionRenewalCooldown;

            _BodyDoubleOwnerHP = H_Config.BodyDoubleOwnerHP;

            _UseWarmDef = H_Config.UseWarmDef;

            _WarmDefCooldown = H_Config.WarmDefCooldown;

            WarmDefHP = H_Config.WarmDefHP;

            _UseMasterSwap = H_Config.UseMasterSwap;

            _MasterSwapOwnerHP = H_Config.MasterSwapOwnerHP;

            _MasterSwapCooldown = H_Config.MasterSwapCooldown;

            // Kiting Options

            _KiteParanoid = Convert.ToBoolean(H_Config.KiteParanoid);

            _KiteStep = H_Config.KiteStep;

            _KiteParanoidStep = H_Config.KiteParanoidStep;

            _KiteThreshold = H_Config.KiteThreshold;

            _KiteParanoidThreshold = H_Config.KiteParanoidThreshold;

            _KiteBounds = H_Config.KiteBounds;

            _ForceKite = Convert.ToBoolean(H_Config.ForceKite);

            _FleeHP = H_Config.FleeHP;



            // Friending Options

            _StandbyFriending = Convert.ToBoolean(H_Config.StandbyFriending);

            _MirAIFriending = Convert.ToBoolean(H_Config.MirAIFriending);



            // Standby Options

            _DefendStandby = Convert.ToBoolean(H_Config.DefendStandby);

            _StickyStandby = (StickyStandbyOptions)H_Config.StickyStandby;



            // Berserk Options

            _UseBerserkMobbed = H_Config.UseBerserkMobbed;

            _UseBerserkSkill = Convert.ToBoolean(H_Config.UseBerserkSkill);

            _UseBerserkAttack = Convert.ToBoolean(H_Config.UseBerserkAttack);

            _Berserk_SkillAlways = Convert.ToBoolean(H_Config.Berserk_SkillAlways);

            _Berserk_Dance = Convert.ToBoolean(H_Config.Berserk_Dance);

            _Berserk_IgnoreMinSP = Convert.ToBoolean(H_Config.Berserk_IgnoreMinSP);

            _Berserk_ComboAlways = Convert.ToBoolean(H_Config.Berserk_ComboAlways);

            

            // Skill Combo Options

            _ComboEnabled = Convert.ToBoolean(H_Config.ComboEnabled);

            _BlueprintComboEnabled = Convert.ToBoolean(H_Config.BlueprintComboEnabled);

            _ComboRunDuringChase = Convert.ToBoolean(H_Config.ComboRunDuringChase);

            _ComboRunDuringAttack = Convert.ToBoolean(H_Config.ComboRunDuringAttack);

            _ComboRunDuringIdle = Convert.ToBoolean(H_Config.ComboRunDuringIdle);

            _ComboResetOnTargetChange = Convert.ToBoolean(H_Config.ComboResetOnTargetChange);

            _ComboAutoAttackDelay = H_Config.ComboAutoAttackDelay;

            _ComboSlot1_SkillID = H_Config.ComboSlot1_SkillID;

            _ComboSlot1_ComboCount = H_Config.ComboSlot1_ComboCount;

            _ComboSlot2_SkillID = H_Config.ComboSlot2_SkillID;

            _ComboSlot2_ComboCount = H_Config.ComboSlot2_ComboCount;

            _ComboSlot3_SkillID = H_Config.ComboSlot3_SkillID;

            _ComboSlot3_ComboCount = H_Config.ComboSlot3_ComboCount;

            _ComboSlot4_SkillID = H_Config.ComboSlot4_SkillID;

            _ComboSlot4_ComboCount = H_Config.ComboSlot4_ComboCount;

            _ComboSlot5_SkillID = H_Config.ComboSlot5_SkillID;

            _ComboSlot5_ComboCount = H_Config.ComboSlot5_ComboCount;

            _ComboSlot6_SkillID = H_Config.ComboSlot6_SkillID;

            _ComboSlot6_ComboCount = H_Config.ComboSlot6_ComboCount;

            _ComboSlot7_SkillID = H_Config.ComboSlot7_SkillID;

            _ComboSlot7_ComboCount = H_Config.ComboSlot7_ComboCount;

            _ComboSlot8_SkillID = H_Config.ComboSlot8_SkillID;

            _ComboSlot8_ComboCount = H_Config.ComboSlot8_ComboCount;

            

            // PVP Options

            _PVPmode = Convert.ToBoolean(H_Config.PVPmode);

        }



        #region Basic Options

        [Category("Debugging"), DisplayName("Enable Debug Logging"), DefaultValue(false),

        Description("Enable detailed AI trace and category logs for troubleshooting. Leave False for normal play. Apply and reload the AI after changing this setting.")]

        public bool EnableDebugLogging { get; set; }



        string _NavigationMap = string.Empty;

        [Category("Navigation Options"),

        DisplayName("Navigation Map"),

        Description(

            "Select the current map for terrain-aware navigation. Empty (Disabled / Unknown) disables navigation. " +

            "Apply settings, then reload the AI for a map change to take effect."),

        DefaultValue("")]

        [TypeConverter(typeof(NavigationMapConverter))]

        public string NavigationMap

        {

            get { return _NavigationMap; }

            set { _NavigationMap = NavigationMapNames.Normalize(value); }

        }



        int _AggroHP = 0;

        [Category("Basic Options"),

        Description(

            "Your Kimi will seek out and attack monsters whenever its " +

            "HP percent (as percent of maximum HP; a number from 0-100) is " +

            "greater than this value. When it lacks HP, it will only fight " +

            "monsters if it is attacked.\n\nSet this value to 100 if you do " +

            "not want the Kimi to attack unless it, the owner, or a " +

            "friend is attacked."

            ),

        DefaultValue(0)]

        public int AggroHP

        {

            get { return _AggroHP; }

            set

            {

                if (value < 0)

                {

                    _AggroHP = 0;

                }

                else if (value > 100)

                {

                    _AggroHP = 100;

                }

                else

                {

                    _AggroHP = value;

                }

            }

        }





        int _AggroSP = 0;

        [Category("Basic Options"),

        Description(

            "Your Kimi will seek out and attack monsters whenever its " +

            "SP percent (as percent of maximum SP; a number from 0-100) is " +

            "greater than this value. When it lacks SP, it will only fight " +

            "monsters if it is attacked."

            ),

        DefaultValue(0)]

        public int AggroSP

        {

            get { return _AggroSP; }

            set

            {

                if (value < 0)

                {

                    _AggroSP = 0;

                }

                else if (value > 100)

                {

                    _AggroSP = 100;

                }

                else

                {

                    _AggroSP = value;

                }

            }

        }





        bool _KiteMonsters = false;

        [Category("Basic Options"),

        Description(

            "Set this to true if you want your Kimi to keep its " +

            "distance from monsters while attacking."

            ),

        DefaultValue(false)]

        public bool KiteMonsters

        {

            get { return _KiteMonsters; }

            set { _KiteMonsters = value; }

        }



        bool _SuperPassive = true;

        [Category("Basic Options"),

        Description(

            "If you want your homunculus to not fight or do anything other " +

            "than watch (and kite, if set to do so), set this value to " +

            "true. This is generally useless for a homunculus."

            ),

        DefaultValue(true)]

        public bool SuperPassive

        {

            get { return _SuperPassive; }

            set { _SuperPassive = value; }

        }





        bool _UseAttackSkill = true;

        [Category("Basic Options"),

        Description(

            "Enable autoskills outside of combos (single-target + AoE when allowed).\n" +

            "Set to false to use only combos/auto-attacks and buffs/heals."

            ),

        DefaultValue(true)]

        public bool UseAttackSkill

        {

            get { return _UseAttackSkill; }

            set { _UseAttackSkill = value; }

        }





        bool _AssumeHomun = true;

        [Category("Basic Options"),

        Description(

            "If you plan to have both a mercenary and homunculus out at the " +

            "same time, set this value to true.\n\nThe default is true."

            ),

        DefaultValue(true)]

        public bool AssumeHomun

        {

            get { return _AssumeHomun; }

            set { _AssumeHomun = value; }

        }





        bool _DoNotChase = false;

        [Category("Basic Options"),

        Description(

            "If you want your homunculus to stand still and only use ranged " +

            "skills or attacks, set this value to true."

            ),

        DefaultValue(false)]

        public bool DoNotChase

        {

            get { return _DoNotChase; }

            set { _DoNotChase = value; }

        }





        bool _UseDanceAttack = false;

        [Category("Basic Options"),

        Description(

            "Set this to true if you want your homunculus to bypass ASPD by " +

            "dancing while attacking. Only recommended if your homun has no" +

            "offensive skills"

            ),

        DefaultValue(false)]

        public bool UseDanceAttack

        {

            get { return _UseDanceAttack; }

            set { _UseDanceAttack = value; }

        }



        bool _UseAvoid = false;

        [Category("Basic Options"),

        Description(

            "Set this to true if you want your homunculus to exit the client " +

            "if it sees a monster listed in H_Avoid.lua"

            ),

        DefaultValue(false)]

        public bool UseAvoid

        {

            get { return _UseAvoid; }

            set { _UseAvoid = value; }

        }



        int _TankMonsterLimit = 30;
        [Category("Basic Options"),

        Description(

            "When set to tank monsters for others to kill, gather this many " +
            "monsters (up to 30) before waiting for them to be killed. "
            ),

        DefaultValue(30)]
        public int TankMonsterLimit

        {

            get { return _TankMonsterLimit; }

            set

            {

                if (value < 1)

                {

                    _TankMonsterLimit = 1;

                }

                else if (value > 30)
                {

                    _TankMonsterLimit = 30;
                }

                else

                {

                    _TankMonsterLimit = value;

                }

            }

        }

        int _RescueOwnerLowHP = 0;

        [Category("Basic Options"),

        Description(

            "Positive: force owner rescue below this HP percentage, regardless of the monster rescue tactic. " +

            "The tactic can still allow rescue above that threshold. Negative: block owner rescue above the absolute percentage. " +

            "For threshold-only rescue, choose Rescue = Owner and use a negative threshold (for example, -80). " +

            "Zero uses the monster rescue tactic. Follow state blocks rescue."

            ),

        DefaultValue(0)]



        public int RescueOwnerLowHP

        {

            get { return _RescueOwnerLowHP; }

            set

            {

                if (value < -100)

                {

                    _RescueOwnerLowHP = -100;

                }   

                else if (value > 100)

                {

                    _RescueOwnerLowHP = 100;

                }

                else

                {

                    _RescueOwnerLowHP = value;

                }

            }

        }



        int _StationaryAggroDist = 10;

        [Category("Basic Options"),

        Description(

            "When the owner is is not moving, attack monsters within this " +

            "distance of the owner. "

            ),

        DefaultValue(10)]

        public int StationaryAggroDist

        {

            get { return _StationaryAggroDist; }

            set

            {

                if (value < 1)

                {

                    _StationaryAggroDist = 1;

                }

                else if (value > 100)

                {

                    _StationaryAggroDist = 100;

                }

                else

                {

                    _StationaryAggroDist = value;

                }

            }

        }



        int _MobileAggroDist = 5;

        [Category("Basic Options"),

        Description(

            "When the owner is is moving, attack monsters within this " +

            "distance of the owner. See also StationaryAggroDist. " +

            "This should probably be relatively low so that the homun " +

            "does not get left behind after chasing something while the " +

            "owner is moving"

            ),

        DefaultValue(5)]

        public int MobileAggroDist

        {

            get { return _MobileAggroDist; }

            set

            {

                if (value < 1)

                {

                    _MobileAggroDist = 1;

                }

                else if (value > 100)

                {

                    _MobileAggroDist = 100;

                }

                else

                {

                    _MobileAggroDist = value;

                }

            }

        }



        KimiTypeOptions _OldHomunType = KimiTypeOptions.Occult;

        [Category("Basic Options"),

        Description(

            "Ward Kimi (1 - Passive/Tank), Occult Kimi (2 - Passive/Magic), Agile Kimi (3 - Aggressive/Physical), Raging Kimi (4 - Aggressive/Balanced)"

            ),

        DefaultValue(KimiTypeOptions.Occult)]

        public KimiTypeOptions OldHomunType

        {

            get { return _OldHomunType; }

            set { _OldHomunType = value; }

        }



        bool _OpportunisticTargeting = false;

        [Category("Basic Options"),

        Description(

            "Choose the nearest eligible ordinary target during chase and combat. Rescue remains separate, and active casts are not interrupted."

            ),

        DefaultValue(true)]

        public bool OpportunisticTargeting

        {

            get { return _OpportunisticTargeting; }

            set { _OpportunisticTargeting = value; }

        }



        bool _AttackLastFullSP = false;

        [Category("Basic Options"),

        Description(

            "If enabled, targets with the ATTACK_LAST tactic will only be" +

            "attacked if the homun is at full SP"

            ),

        DefaultValue(false)]

        public bool AttackLastFullSP

        {

            get { return _AttackLastFullSP; }

            set { _AttackLastFullSP = value; }

        }



        int _DanceMinSP = 0;

        [Category("Basic Options"),

        Description(

            "If UseDanceAttack is enabled, the homun will only 'dance' if " +

            "it has at least this much SP as a percent"

            ),

        DefaultValue(0)]

        public int DanceMinSP

        {

            get { return _DanceMinSP; }

            set

            {

                if (value < -100)

                {

                    _DanceMinSP = -100;

                }

                else if (value > 6000)

                {

                    _DanceMinSP = 6000;

                }

                else

                {

                    _DanceMinSP = value;

                }

            }

        }

        int _AttackTimeLimit = 10000;

        [Category("Basic Options"),

        Description(

            "This is the longest time, in milliseconds, that the " +

            "homunculus will spend trying to attack a target which it has already attacked - this is to prevent homun from getting hung up on posbugged or inaccessible monsters. As of 1.54, this timer is reset if we see the monster flinching while nothing else is targeting it - that should mean that we're successfully attacking it, and should keep going"

            ),

        DefaultValue(10000)]

        public int AttackTimeLimit

        {

            get { return _AttackTimeLimit; }

            set

            {

                if (value < 1000)

                {

                    _AttackTimeLimit = 1000;

                }

                else if (value > 60000)

                {

                    _AttackTimeLimit = 60000;

                }

                else

                {

                    _AttackTimeLimit = value;

                }

            }

        }

        int _LagReduction = 0;

        [Category("Basic Options"),

        Description(

            "If you experience lag that appears only with homunculus out on some maps, try setting this to 1. This will reduce responsiveness on maps that do not lag." +

            "In severe cases, this can be set to 2, or even higher. This will greatly slow the homunculus' reaction time, and should be used only if truly necessary"

            ),

        DefaultValue(0)]

        public int LagReduction

        {

            get { return _LagReduction; }

            set {

                if (value < 0)

                {

                    _LagReduction = 0;

                }

                else if (value > 5)

                {

                    _LagReduction = 5;

                }

                else

                {

                    _LagReduction = value;

                }

            }

        }

        bool _DoNotAttackMoving = false;

        [Category("Basic Options"),

        Description(

            "If set to true, homun will not attack monsters that are currently moving"

            ),

        DefaultValue(false)]

        public bool DoNotAttackMoving

        {

            get { return _DoNotAttackMoving; }

            set { _DoNotAttackMoving = value; }

        }

        bool _LiveMobID = false;

        [Category("Basic Options"),

        Description(

            "If enabled, and owner has merc out, and merc has LiveMobID enabled as well, the mercenary will be able to identify monsters on screen as long as the homun is alive. " +

            "See the documentation for more details and caveats. This may cause performance problems on some systems. "

            ),

        DefaultValue(false)]

        public bool LiveMobID

        {

            get { return _LiveMobID; }

            set { _LiveMobID = value; }

        }

        #endregion



        #region AutoSkill Options

        int _AttackSkillReserveSP = 0;

        [Category("AutoSkill Options"),

        Description(

            "To control SP use, you may not want your Kimi to use " +

            "skills unless there would be enough SP left to recast some " +

            "sort of buff. Set this value to this minimum SP value to " +

            "control this."

            ),

        DefaultValue(0)]

        public int AttackSkillReserveSP

        {

            get { return _AttackSkillReserveSP; }

            set

            {

                if (value < 0)

                {

                    _AttackSkillReserveSP = 0;

                }

                else if (value > 6000)

                {

                    _AttackSkillReserveSP = 6000;

                }

                else

                {

                    _AttackSkillReserveSP = value;

                }

            }

        }





        AutoMobModeOptions _AutoMobMode = AutoMobModeOptions.All;

        [Category("AutoSkill Options"),

        Description(

            "CRITICAL: Controls automatic target selection.\n" +

            "Disabled (0) = Only attacks manually selected targets.\n" +

            "Aggressive (1) = Auto-attacks aggressive mobs.\n" +

            "All (2) = Auto-attacks all monsters in range (RECOMMENDED)."

            ),

        DefaultValue(AutoMobModeOptions.All)]

        public AutoMobModeOptions AutoMobMode

        {

            get { return _AutoMobMode; }

            set { _AutoMobMode = value; }

        }



        int _AutoMobCount = 2;

        [Category("AutoSkill Options"),

        Description(

            "If Kimi has an AoE skill, the skill will be " +

            "automatically used if the number of monsters in close " +

            "proximity to Kimi or owner is equal to or greater " +

            "than this value."

            ),

        DefaultValue(2)]

        public int AutoMobCount

        {

            get { return _AutoMobCount; }

            set

            {

                if (value < 1)

                {

                    _AutoMobCount = 1;

                }

                else if (value > 20)

                {

                    _AutoMobCount = 20;

                }

                else

                {

                    _AutoMobCount = value;

                }

            }

        }





        UseSkillOnlyOptions _UseSkillOnly = UseSkillOnlyOptions.Chasing;

        [Category("AutoSkill Options"),

        Description(

            "If you want your Kimi to use skills while attacking and " +

            "while chasing if their skill has a longer range than their " +

            "normal attack, set this to Chasing. If you want it to use skills " +

            "only while in melee range, set this to Attacking. If you want it to only use " +

            "skills and not attack normally, set this to SkillOnly."

            ),

        DefaultValue(UseSkillOnlyOptions.Chasing)]

        public UseSkillOnlyOptions UseSkillOnly

        {

            get { return _UseSkillOnly; }

            set { _UseSkillOnly = value; }

        }





		int _AutoSkillDelay = 400;

        [Category("AutoSkill Options"),

        Description(

            "This is the extra delay (in ms) added between automatic skill casts. " +

            "Actual casts always include the skill's own cast timing, cooldowns, and SP checks. " +

            "Use 0 to keep only runtime cast timing; negative values are treated as 0 in the control."

            ),

        DefaultValue(400)]

		public int AutoSkillDelay

	{

		get { return _AutoSkillDelay; }

		set {

			if (value < 0)

			{

				_AutoSkillDelay=0;

			}

			else if (value > 600){

				_AutoSkillDelay=600;

			}

			else{

				_AutoSkillDelay= value;

			}

		}

	}





        bool _AoEReserveSP = false;

        [Category("AutoSkill Options"),

        Description(

            "Enable this to not use non-AoE skill attacks unless doing so " +

            "would leave enough SP to cast Kimi's AoE attack (Illusion of Light). "

            ),

        DefaultValue(false)]

        public bool AoEReserveSP

        {

            get { return _AoEReserveSP; }

            set { _AoEReserveSP = value; }

        }



        bool _AoEFixedLevel = true;

        [Category("AutoSkill Options"),

        Description(

            "Enable this to ignore skill level tactics when using " +

            "AoE attacks. "

            ),

        DefaultValue(true)]

        public bool AoEFixedLevel

        {

            get { return _AoEFixedLevel; }

            set { _AoEFixedLevel = value; }

        }



        int _IllusionOfClawsLevel = 0;

        [Category("Kimi Skills - Agile"),

        Description("Editable level for Illusion of Claw (8009). Enable it separately; valid levels are 1-10."),

        DefaultValue(1)]

        public int IllusionOfClawsLevel

        {

            get { return GetKimiSkillLevel(8009); }

            set { SetKimiSkillLevel(8009, value); }

        }



        [Category("Kimi Skills - Agile"),

        Description("Enable Illusion of Claw (8009) for Agile Kimi."),

        DefaultValue(false)]

        public bool IllusionOfClawsEnabled

        {

            get { return GetKimiSkillEnabled(8009); }

            set { SetKimiSkillEnabled(8009, value); }

        }



        int _IllusionOfBreathLevel = 0;

        [Category("Kimi Skills - Occult"),

        Description("Editable level for Illusion of Breath (8024). Enable it separately; valid levels are 1-10."),

        DefaultValue(1)]

        public int IllusionOfBreathLevel

        {

            get { return GetKimiSkillLevel(8024); }

            set { SetKimiSkillLevel(8024, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Enable Illusion of Breath (8024) for Occult Kimi."),

        DefaultValue(false)]

        public bool IllusionOfBreathEnabled

        {

            get { return GetKimiSkillEnabled(8024); }

            set { SetKimiSkillEnabled(8024, value); }

        }



        int _IllusionOfCrusherLevel = 0;

        [Category("Kimi Skills - Raging"),

        Description("Editable level for Illusion Crusher (8031). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        public int IllusionOfCrusherLevel

        {

            get { return GetKimiSkillLevel(8031); }

            set { SetKimiSkillLevel(8031, value); }

        }



        [Category("Kimi Skills - Raging"),

        Description("Enable Illusion Crusher (8031) for Raging Kimi."),

        DefaultValue(false)]

        public bool IllusionOfCrusherEnabled

        {

            get { return GetKimiSkillEnabled(8031); }

            set { SetKimiSkillEnabled(8031, value); }

        }



        int _IllusionOfLightLevel = 0;

        [Category("Kimi Skills - Occult"),

        Description("Editable level for Illusion of Light (8034). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        public int IllusionOfLightLevel

        {

            get { return GetKimiSkillLevel(8034); }

            set { SetKimiSkillLevel(8034, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Enable Illusion of Light (8034) for Occult Kimi."),

        DefaultValue(false)]

        public bool IllusionOfLightEnabled

        {

            get { return GetKimiSkillEnabled(8034); }

            set { SetKimiSkillEnabled(8034, value); }

        }



        int _ChaoticHealLevel = 0;

        [Category("Kimi Skills - Occult"),

        Description("Editable level for Chaotic Heal (8014). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        public int ChaoticHealLevel

        {

            get { return GetKimiSkillLevel(8014); }

            set { SetKimiSkillLevel(8014, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Enable Chaotic Heal (8014) for Occult Kimi."),

        DefaultValue(false)]

        public bool ChaoticHealEnabled

        {

            get { return GetKimiSkillEnabled(8014); }

            set { SetKimiSkillEnabled(8014, value); }

        }



        int _BodyDoubleLevel = 0;

        [Category("Kimi Skills - Common"),

        Description("Editable level for Body Double (8022). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        public int BodyDoubleLevel

        {

            get { return GetKimiSkillLevel(8022); }

            set { SetKimiSkillLevel(8022, value); }

        }



        [Category("Kimi Skills - Common"),

        Description("Enable Body Double (8022) for Ward, Occult, Agile, or Raging Kimi."),

        DefaultValue(false)]

        public bool BodyDoubleEnabled

        {

            get { return GetKimiSkillEnabled(8022); }

            set { SetKimiSkillEnabled(8022, value); }

        }



        int _WarmDefLevel = 0;

        [Category("Kimi Skills - Ward"),

        Description("Editable level for Bastion Renewal (8006). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        [DisplayName("Bastion Renewal Level")]

        public int WarmDefLevel

        {

            get { return GetKimiSkillLevel(8006); }

            set { SetKimiSkillLevel(8006, value); }

        }



        [Category("Kimi Skills - Ward"),

        Description("Enable Bastion Renewal (8006) for Ward Kimi."),

        DefaultValue(false)]

        [DisplayName("Bastion Renewal Enabled")]

        public bool WarmDefEnabled

        {

            get { return GetKimiSkillEnabled(8006); }

            set { SetKimiSkillEnabled(8006, value); }

        }



        [Category("Kimi Skills - Ward"),

        Description("Editable level for Ward's Domain (8033), a self centered protection aura; valid levels are 1-5."),

        DefaultValue(1)]

        public int WardDomainLevel

        {

            get { return GetKimiSkillLevel(8033); }

            set { SetKimiSkillLevel(8033, value); }

        }



        [Category("Kimi Skills - Ward"),

        Description("Enable Ward's Domain (8033) for Ward Kimi."),

        DefaultValue(false)]

        public bool WardDomainEnabled

        {

            get { return GetKimiSkillEnabled(8033); }

            set { SetKimiSkillEnabled(8033, value); }

        }



        [Category("Kimi Skills - Ward"),

        Description("Editable level for Taunt (8021), a self centered area skill; valid levels are 1-5."),

        DefaultValue(1)]

        public int TauntLevel

        {

            get { return GetKimiSkillLevel(8021); }

            set { SetKimiSkillLevel(8021, value); }

        }



        [Category("Kimi Skills - Ward"),

        Description("Enable Taunt (8021) for Ward Kimi."),

        DefaultValue(false)]

        public bool TauntEnabled

        {

            get { return GetKimiSkillEnabled(8021); }

            set { SetKimiSkillEnabled(8021, value); }

        }



        [Category("Kimi Skills - Common"),

        Description("Editable level for Quick Defense (8023). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        public int QuickDefenseLevel

        {

            get { return GetKimiSkillLevel(8023); }

            set { SetKimiSkillLevel(8023, value); }

        }



        [Category("Kimi Skills - Common"),

        Description("Enable Quick Defense (8023) for Ward, Occult, Agile, or Raging Kimi."),

        DefaultValue(false)]

        public bool QuickDefenseEnabled

        {

            get { return GetKimiSkillEnabled(8023); }

            set { SetKimiSkillEnabled(8023, value); }

        }



        [Category("Kimi Skills - Common"),

        Description("Editable level for Master Swap (8005). This utility targets the owner; valid levels are 1-5."),

        DefaultValue(1)]

        public int MasterSwapLevel

        {

            get { return GetKimiSkillLevel(8005); }

            set { SetKimiSkillLevel(8005, value); }

        }



        [Category("Kimi Skills - Common"),

        Description("Enable Master Swap (8005) for Ward, Occult, or Agile Kimi."),

        DefaultValue(false)]

        public bool MasterSwapEnabled

        {

            get { return GetKimiSkillEnabled(8005); }

            set { SetKimiSkillEnabled(8005, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Editable level for Chaotic Sanctuary (8013), a ground support skill; valid levels are 1-5."),

        DefaultValue(1)]

        public int ChaoticSanctuaryLevel

        {

            get { return GetKimiSkillLevel(8013); }

            set { SetKimiSkillLevel(8013, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Enable Chaotic Sanctuary (8013) for Occult Kimi."),

        DefaultValue(false)]

        public bool ChaoticSanctuaryEnabled

        {

            get { return GetKimiSkillEnabled(8013); }

            set { SetKimiSkillEnabled(8013, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Editable level for Arcane Offering (8015), a self support skill; valid levels are 1-5."),

        DefaultValue(1)]

        public int ArcaneOfferingLevel

        {

            get { return GetKimiSkillLevel(8015); }

            set { SetKimiSkillLevel(8015, value); }

        }



        [Category("Kimi Skills - Occult"),

        Description("Enable Arcane Offering (8015) for Occult Kimi."),

        DefaultValue(false)]

        public bool ArcaneOfferingEnabled

        {

            get { return GetKimiSkillEnabled(8015); }

            set { SetKimiSkillEnabled(8015, value); }

        }



        [Category("Kimi Skills - Agile"),

        Description("Editable level for Fade Away (8012). Enable it separately; valid level is 1."),

        DefaultValue(1)]

        public int FadeAwayLevel

        {

            get { return GetKimiSkillLevel(8012); }

            set { SetKimiSkillLevel(8012, value); }

        }



        [Category("Kimi Skills - Agile"),

        Description("Enable Fade Away (8012) for Agile Kimi."),

        DefaultValue(false)]

        public bool FadeAwayEnabled

        {

            get { return GetKimiSkillEnabled(8012); }

            set { SetKimiSkillEnabled(8012, value); }

        }



        [Category("Kimi Skills - Agile"),

        Description("Editable level for Mirage Assault (8036). Enable it separately; valid levels are 1-5."),

        DefaultValue(1)]

        public int MirageAssaultLevel

        {

            get { return GetKimiSkillLevel(8036); }

            set { SetKimiSkillLevel(8036, value); }

        }



        [Category("Kimi Skills - Agile"),

        Description("Enable Mirage Assault (8036) for Agile Kimi."),

        DefaultValue(false)]

        public bool MirageAssaultEnabled

        {

            get { return GetKimiSkillEnabled(8036); }

            set { SetKimiSkillEnabled(8036, value); }

        }



        [Category("Kimi Skills - Raging"),

        Description("Editable level for Blood Sweep (8032). Enable it separately; valid levels are 1-10."),

        DefaultValue(1)]

        public int BloodSweepLevel

        {

            get { return GetKimiSkillLevel(8032); }

            set { SetKimiSkillLevel(8032, value); }

        }



        [Category("Kimi Skills - Raging"),

        Description("Enable Blood Sweep (8032) for Raging Kimi."),

        DefaultValue(false)]

        public bool BloodSweepEnabled

        {

            get { return GetKimiSkillEnabled(8032); }

            set { SetKimiSkillEnabled(8032, value); }

        }



        // Canonical aliases keep property names aligned with KIMI_SKILLS.txt

        // while legacy names remain available to existing callers.

        [Browsable(false)]

        public int BastionRenewalLevel { get { return WarmDefLevel; } set { WarmDefLevel = value; } }



        [Browsable(false)]

        public bool BastionRenewalEnabled { get { return WarmDefEnabled; } set { WarmDefEnabled = value; } }



        [Browsable(false)]

        public int IllusionOfClawLevel { get { return IllusionOfClawsLevel; } set { IllusionOfClawsLevel = value; } }



        [Browsable(false)]

        public bool IllusionOfClawEnabled { get { return IllusionOfClawsEnabled; } set { IllusionOfClawsEnabled = value; } }



        [Browsable(false)]

        public int IllusionCrusherLevel { get { return IllusionOfCrusherLevel; } set { IllusionOfCrusherLevel = value; } }



        [Browsable(false)]

        public bool IllusionCrusherEnabled { get { return IllusionOfCrusherEnabled; } set { IllusionOfCrusherEnabled = value; } }



        [Category("Kimi Skills - Ward"),

        DisplayName("Passive Skills (read-only)"),

        Description("Passive Ward skills are always available to the runtime and cannot be selected as cast skills."),

        ReadOnly(true), Browsable(true)]

        public string WardPassiveSkills

        {

            get { return FormatPassiveSkills(KimiTypeOptions.Ward); }

        }



        [Category("Kimi Skills - Occult"),

        DisplayName("Passive Skills (read-only)"),

        Description("Passive Occult skills are always available to the runtime and cannot be selected as cast skills."),

        ReadOnly(true), Browsable(true)]

        public string OccultPassiveSkills

        {

            get { return FormatPassiveSkills(KimiTypeOptions.Occult); }

        }



        [Category("Kimi Skills - Agile"),

        DisplayName("Passive Skills (read-only)"),

        Description("Passive Agile skills are always available to the runtime and cannot be selected as cast skills."),

        ReadOnly(true), Browsable(true)]

        public string AgilePassiveSkills

        {

            get { return FormatPassiveSkills(KimiTypeOptions.Agile); }

        }



        [Category("Kimi Skills - Raging"),

        DisplayName("Passive Skills (read-only)"),

        Description("Passive Raging skills are always available to the runtime and cannot be selected as cast skills."),

        ReadOnly(true), Browsable(true)]

        public string RagingPassiveSkills

        {

            get { return FormatPassiveSkills(KimiTypeOptions.Raging); }

        }



        bool _OnlyAOE = false;

        [Category("Kimi Skills"),

        Description(

            "If enabled, Kimi will only use Illusion of Light and ignore other offensive skills. " +

            "Use this for pure AoE-focused Occult Kimi builds. Requires that IllusionOfLightLevel > 0."

            ),

        DefaultValue(false)]

        public bool OnlyAOE

        {

            get { return _OnlyAOE; }

            set { _OnlyAOE = value; }

        }

        

        #endregion





        #region Skill Combo Options

        bool _ComboEnabled = false;

        [Category("Skill Combo Options"),

        Description(

            "Enable skill combo system. When enabled, Kimi will execute a pre-configured sequence of skills (1-4 rotation) based on context (Chase/Attack/Idle). " +

            "Combo resets when target changes (if ComboResetOnTargetChange=true)."

            ),

        DefaultValue(false)]

        public bool ComboEnabled

        {

            get { return _ComboEnabled; }

            set { _ComboEnabled = value; }

        }



        bool _BlueprintComboEnabled = false;

        [Category("Skill Combo Options"),

        Description(

            "Enable Blueprint combo system. When enabled, Kimi executes custom combos built in the Blueprint Combo Tactics editor. " +

            "These are visual node-based combos exported to Lua with complete property control (delays, repeat counts, target modes, etc.)."

            ),

        DefaultValue(false)]

        public bool BlueprintComboEnabled

        {

            get { return _BlueprintComboEnabled; }

            set { _BlueprintComboEnabled = value; }

        }



        bool _ComboRunDuringChase = true;

        [Category("Skill Combo Options"),

        Description("Enable combo execution while chasing targets."),

        DefaultValue(true)]

        public bool ComboRunDuringChase

        {

            get { return _ComboRunDuringChase; }

            set { _ComboRunDuringChase = value; }

        }



        bool _ComboRunDuringAttack = true;

        [Category("Skill Combo Options"),

        Description("Enable combo execution while attacking targets."),

        DefaultValue(true)]

        public bool ComboRunDuringAttack

        {

            get { return _ComboRunDuringAttack; }

            set { _ComboRunDuringAttack = value; }

        }



        bool _ComboRunDuringIdle = false;

        [Category("Skill Combo Options"),

        Description("Enable combo execution while idle (not in combat)."),

        DefaultValue(false)]

        public bool ComboRunDuringIdle

        {

            get { return _ComboRunDuringIdle; }

            set { _ComboRunDuringIdle = value; }

        }



        bool _ComboResetOnTargetChange = true;

        [Category("Skill Combo Options"),

        Description("Reset combo slot to 1 when the target changes. Prevents combo continuation across targets."),

        DefaultValue(true)]

        public bool ComboResetOnTargetChange

        {

            get { return _ComboResetOnTargetChange; }

            set { _ComboResetOnTargetChange = value; }

        }



        int _ComboAutoAttackDelay = 200;

        [Category("Skill Combo Options"),

        Description("Delay (ms) after issuing a combo auto-attack before advancing. Counts represent attack commands; continuous attacks may continue while a skill is unavailable."),

        DefaultValue(200)]

        public int ComboAutoAttackDelay

        {

            get { return _ComboAutoAttackDelay; }

            set { _ComboAutoAttackDelay = (value < 0) ? 0 : value; }

        }



        int _ComboSlot1_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 1"),

        Description("Combo Slot 1: Select a skill for the first slot in the combo rotation."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot1_SkillID

        {

            get { return _ComboSlot1_SkillID; }

            set { _ComboSlot1_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot1_ComboCount = 1;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 1 - Combo Count"),

        Description("Number of times to cast Combo Slot 1 skill before advancing to Slot 2 (1-10)."),

        DefaultValue(1)]

        public int ComboSlot1_ComboCount

        {

            get { return _ComboSlot1_ComboCount; }

            set { _ComboSlot1_ComboCount = (value < 1) ? 1 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot2_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 2"),

        Description("Combo Slot 2: Select a skill for the second slot in the combo rotation."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot2_SkillID

        {

            get { return _ComboSlot2_SkillID; }

            set { _ComboSlot2_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot2_ComboCount = 1;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 2 - Combo Count"),

        Description("Number of times to cast Combo Slot 2 skill before advancing to Slot 3 (1-10)."),

        DefaultValue(1)]

        public int ComboSlot2_ComboCount

        {

            get { return _ComboSlot2_ComboCount; }

            set { _ComboSlot2_ComboCount = (value < 1) ? 1 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot3_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 3"),

        Description("Combo Slot 3: Select a skill for the third slot in the combo rotation."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot3_SkillID

        {

            get { return _ComboSlot3_SkillID; }

            set { _ComboSlot3_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot3_ComboCount = 1;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 3 - Combo Count"),

        Description("Number of times to cast Combo Slot 3 skill before advancing to Slot 4 (1-10)."),

        DefaultValue(1)]

        public int ComboSlot3_ComboCount

        {

            get { return _ComboSlot3_ComboCount; }

            set { _ComboSlot3_ComboCount = (value < 1) ? 1 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot4_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 4"),

        Description("Combo Slot 4: Select a skill for the fourth slot in the combo rotation."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot4_SkillID

        {

            get { return _ComboSlot4_SkillID; }

            set { _ComboSlot4_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot4_ComboCount = 1;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 4 - Combo Count"),

        Description("Number of times to cast Combo Slot 4 skill before advancing to Slot 5 (1-10). Set to 0 to disable slot."),

        DefaultValue(1)]

        public int ComboSlot4_ComboCount

        {

            get { return _ComboSlot4_ComboCount; }

            set { _ComboSlot4_ComboCount = (value < 0) ? 0 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot5_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 5"),

        Description("Combo Slot 5: Select a skill for the fifth slot in the combo rotation. Use -1 for auto-attack."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot5_SkillID

        {

            get { return _ComboSlot5_SkillID; }

            set { _ComboSlot5_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot5_ComboCount = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 5 - Combo Count"),

        Description("Number of times to use Combo Slot 5 before advancing to Slot 6 (0-10). Set to 0 to disable slot."),

        DefaultValue(0)]

        public int ComboSlot5_ComboCount

        {

            get { return _ComboSlot5_ComboCount; }

            set { _ComboSlot5_ComboCount = (value < 0) ? 0 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot6_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 6"),

        Description("Combo Slot 6: Select a skill for the sixth slot in the combo rotation. Use -1 for auto-attack."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot6_SkillID

        {

            get { return _ComboSlot6_SkillID; }

            set { _ComboSlot6_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot6_ComboCount = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 6 - Combo Count"),

        Description("Number of times to use Combo Slot 6 before advancing to Slot 7 (0-10). Set to 0 to disable slot."),

        DefaultValue(0)]

        public int ComboSlot6_ComboCount

        {

            get { return _ComboSlot6_ComboCount; }

            set { _ComboSlot6_ComboCount = (value < 0) ? 0 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot7_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 7"),

        Description("Combo Slot 7: Select a skill for the seventh slot in the combo rotation. Use -1 for auto-attack."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot7_SkillID

        {

            get { return _ComboSlot7_SkillID; }

            set { _ComboSlot7_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot7_ComboCount = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 7 - Combo Count"),

        Description("Number of times to use Combo Slot 7 before advancing to Slot 8 (0-10). Set to 0 to disable slot."),

        DefaultValue(0)]

        public int ComboSlot7_ComboCount

        {

            get { return _ComboSlot7_ComboCount; }

            set { _ComboSlot7_ComboCount = (value < 0) ? 0 : (value > 10) ? 10 : value; }

        }



        int _ComboSlot8_SkillID = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 8"),

        Description("Combo Slot 8: Select a skill for the eighth slot in the combo rotation. Use -1 for auto-attack."),

        DefaultValue(0),

        TypeConverter(typeof(SkillIdConverter))]

        public int ComboSlot8_SkillID

        {

            get { return _ComboSlot8_SkillID; }

            set { _ComboSlot8_SkillID = (value < -1) ? -1 : value; }

        }



        int _ComboSlot8_ComboCount = 0;

        [Category("Skill Combo Options"),

        DisplayName("Combo Slot 8 - Combo Count"),

        Description("Number of times to use Combo Slot 8 before restarting combo (0-10). Set to 0 to disable slot."),

        DefaultValue(0)]

        public int ComboSlot8_ComboCount

        {

            get { return _ComboSlot8_ComboCount; }

            set { _ComboSlot8_ComboCount = (value < 0) ? 0 : (value > 10) ? 10 : value; }

        }

        

        #endregion





        #region Walk/Follow Options

        int _FollowStayBack = 2;

        [Category("Walk/Follow Options"),

        Description(

            "Your homunculus will stay this many cells behind you when " +

            "following you."

            ),

        DefaultValue(2)]

        public int FollowStayBack

        {

            get { return _FollowStayBack; }

            set

            {

                if (value < 0)

                {

                    _FollowStayBack = 0;

                }

                else if (value > 10)

                {

                    _FollowStayBack = 10;

                }

                else

                {

                    _FollowStayBack = value;

                }

            }

        }





        





        int _RestXOff = -2;

        [Category("Walk/Follow Options"),

        Description(

            "If set to rest, the homunculus will move this many cells east " +

            "of you when you sit.\n\nSetting this to a negative number will " +

            "cause the homunculus to move west instead."

            ),

        DefaultValue(-2)]

        public int RestXOff

        {

            get { return _RestXOff; }

            set

            {

                if (value < -8)

                {

                    _RestXOff = -8;

                }

                else if (value > 8)

                {

                    _RestXOff = 8;

                }

                else

                {

                    _RestXOff = value;

                }

            }

        }





        int _RestYOff = 0;

        [Category("Walk/Follow Options"),

        Description(

            "If set to rest, the homunculus will move this many cells north " +

            "of you when you sit.\n\nSetting this to a negative number will " +

            "cause the homunculus to move south instead."

            ),

        DefaultValue(0)]

        public int RestYOff

        {

            get { return _RestYOff; }

            set

            {

                if (value < -8)

                {

                    _RestYOff = -8;

                }

                else if (value > 8)

                {

                    _RestYOff = 8;

                }

                else

                {

                    _RestYOff = value;

                }

            }

        }





        bool _DoNotUseRest = false;

        [Category("Walk/Follow Options"),

        Description(

            "Set this to false if when you sit down, you want your homunculus to become passive, and, when it becomes idle, will move close to you."

            ),

        DefaultValue(false)]

        public bool DoNotUseRest

        {

            get { return _DoNotUseRest; }

            set { _DoNotUseRest = value; }

        }





        int _SpawnDelay = 1000;

        [Category("Walk/Follow Options"),

        Description(

            "Upon spawning, the homunculus will wait for this many " +

            "miliseconds before taking any actions. This prevents it from " +

            "wasting its immunity time and also prevents it from KSing " +

            "after teleporting or changing maps.\n\nSetting this value to " +

            "1000 (1 second) is a good idea."

            ),

        DefaultValue(1000)]

        public int SpawnDelay

        {

            get { return _SpawnDelay; }

            set

            {

                if (value < 100)

                {

                    _SpawnDelay = 100;

                }

                else if (value > 2000)

                {

                    _SpawnDelay = 2000;

                }

                else

                {

                    _SpawnDelay = value;

                }

            }

        }







        bool _MoveSticky = false;

        [Category("Walk/Follow Options"),

        Description(

            "Set this to true if you want your homunculus to stay put when " +

            "told to go somewhere."

            ),

        DefaultValue(false)]

        public bool MoveSticky

        {

            get { return _MoveSticky; }

            set { _MoveSticky = value; }

        }





        bool _MoveStickyFight = false;

        [Category("Walk/Follow Options"),

        Description(

            "Set this to true if you want your homunculus to fight normally " +

            "if MoveSticky is set to true."

            ),

        DefaultValue(false)]

        public bool MoveStickyFight // Set to 1 to fight normally in above mode

        {

            get { return _MoveStickyFight; }

            set { _MoveStickyFight = value; }

        }

        UseIdleWalkOptions _UseIdleWalk = UseIdleWalkOptions.None;

        [Category("Walk/Follow Options"),

        Description(

            "When at full hp with nothing better to do, set this option to " +

            "the pattern that it should walk in. See documentation for" +

            "more information before using Route walk"

            ),

        DefaultValue(UseIdleWalkOptions.None)]

        public UseIdleWalkOptions UseIdleWalk

        {

            get { return _UseIdleWalk; }

            set { _UseIdleWalk = value; }

        }





        int _IdleWalkSP = 80;

        [Category("Walk/Follow Options"),

        Description(

            "Only use IdleWalk when SP is above this, as a %" +

            "                                                              "

            ),

        DefaultValue(80)]

        public int IdleWalkSP

        {

            get { return _IdleWalkSP; }

            set

            {

                if (value < 0)

                {

                    _IdleWalkSP = 0;

                }

                else if (value > 100)

                {

                    _IdleWalkSP = 100;

                }

                else

                {

                    _IdleWalkSP = value;

                }

            }

        }



        bool _UseCastleRoute = false;

        [Category("Walk/Follow Options"),

        Description(

            "Enable this to use castling for route walk. See documentation." +

            "                                                              "

            ),

        DefaultValue(false)]

        public bool UseCastleRoute

        {

            get { return _UseCastleRoute; }

            set { _UseCastleRoute = value; }

        }



        bool _RelativeRoute = false;

        [Category("Walk/Follow Options"),

        Description(

            "Enable this to use relative routes. See documentation" +

            "                                                              "

            ),

        DefaultValue(false)]

        public bool RelativeRoute

        {

            get { return _RelativeRoute; }

            set { _RelativeRoute = value; }

        }



        int _IdleWalkDistance = 4;

        [Category("Walk/Follow Options"),

        Description(

            "When walking while idle, keep this distance from owner." +

            "                                                              "

            ),

        DefaultValue(4)]

        public int IdleWalkDistance

        {

            get { return _IdleWalkDistance; }

            set

            {

                if (value < 0)

                {

                    _IdleWalkDistance = 0;

                }

                else if (value > 14)

                {

                    _IdleWalkDistance = 14;

                }

                else

                {

                    _IdleWalkDistance = value;

                }

            }

        }





        bool _ChaseSPPause = false;

        [Category("Walk/Follow Options"),

        Description(

            "Enable this to make the homun delay moving to a new target when it is below a specified SP level, AND is expecting an SP-regen tick in the near future. " +

            "This is an extreme measure to deal with SP problems resulting from the homun never staying still long enough to regen any SP (homuns do not regen sp while moving). " +

            "WARNING: This will make your homun pause after each kill, and this may be undesirable. Use this only if you understand this option. See documentation for details."

            ),

        DefaultValue(false)]

        public bool ChaseSPPause

        {

            get { return _ChaseSPPause; }

            set { _ChaseSPPause = value; }

        }



        int _ChaseSPPauseSP = 0;

        [Category("Walk/Follow Options"),

        Description(

            "When ChaseSPPause is enabled, and SP is below this threshold, the homun may pause if near an SP tick." +

            "If set to a negative number, this is treated as a percentage. Otherwise, it is simply the number of SP."

            ),

        DefaultValue(-20)]

        public int ChaseSPPauseSP

        {

            get { return _ChaseSPPauseSP; }

            set

            {

                if (value < -100)

                {

                    _ChaseSPPauseSP = -100;

                }

                else if (value > 6000)

                {

                    _ChaseSPPauseSP = 6000;

                }

                else

                {

                    _ChaseSPPauseSP = value;

                }

            }

        }



        int _ChaseSPPauseTime = 2000;

        [Category("Walk/Follow Options"),

        Description(

            "if ChaseSPPause is enabled, and SP is below the specified threshold, the homun will pause if it is expecting an SP tick within this length of time." +

            "This is specified in milliseconds, ie, 2000 = 2 seconds. The homun SP tick is every 8 seconds"

            ),

        DefaultValue(2000)]

        public int ChaseSPPauseTime

        {

            get { return _ChaseSPPauseTime; }

            set

            {

                if (value < 100)

                {

                    _ChaseSPPauseTime = 100;

                }

                else if (value > 6000)

                {

                    _ChaseSPPauseTime = 6000;

                }

                else

                {

                    _ChaseSPPauseTime = value;

                }

            }

        }



        int _StationaryMoveBounds = 14;

        [Category("Walk/Follow Options"),

        Description(

            "This is the farthest from the owner that the homun will be allowed " +

            "to get before dropping all targets and moving back to the owner. This " +

            "value is used while the owner is NOT moving (see also MobileMoveBounds) " +

            "To control aggro range of homun, use the AggroDist options, not MoveBounds. " +

            "To set distance from owner that homun should return to after killing " +

            "use FollowStayBack, not MoveBounds. "



            ),

        DefaultValue(14)]

        public int StationaryMoveBounds

        {

            get { return _StationaryMoveBounds; }

            set

            {

                if (value < 1 )

                {

                    _StationaryMoveBounds = 1;

                }

                else if (value > 100)

                {

                    _StationaryMoveBounds = 100;

                }

                else

                {

                    _StationaryMoveBounds = value;

                }

            }

        }



        int _MobileMoveBounds = 9;

        [Category("Walk/Follow Options"),

        Description(



            "This is the farthest from the owner that the homun will be allowed " +

            "to get before dropping all targets and moving back to the owner. This " +

            "value is used while the owner IS moving (see also StationaryMoveBounds) " +

            "To control aggro range of homun, use the AggroDist options, not MoveBounds. " +

            "This should probably be lower than StationaryMoveBounds to help keep homun " +

            "from being left behind."

            ),

        DefaultValue(5)]

        public int MobileMoveBounds

        {

            get { return _MobileMoveBounds; }

            set

            {

                if (value < 1)

                {

                    _MobileMoveBounds = 1;

                }

                else if (value > 100)

                {

                    _MobileMoveBounds = 100;

                }

                else

                {

                    _MobileMoveBounds = value;

                }

            }

        }



        #endregion





        #region Autobuff Options



        // OLD HOMUNCULUS AUTOBUFFS - REMOVED FOR KIMI-ONLY VERSION













        int _HealOwnerHP = 40;

        [Category("Autobuff Options"),

        Description(

            "Set this value to the minimum HP (as a percent of maximum HP) " +

            "required for your homun to use healing skills."

            ),

        DefaultValue(40)]

        public int HealOwnerHP

	{

		get { return _HealOwnerHP; }

		set {

			if (value < 0)

			{

				_HealOwnerHP=0;

			}

			else if (value > 100){

				_HealOwnerHP=100;

			}

			else{

				_HealOwnerHP= value;

			}

		}

	}

        int _HealSelfHP = 0;

        [Category("Autobuff Options"),

        Description(

            "If your homun is a Vani, and UseAutoHeal is enabled then use" +

            "Chaotic Blessings to try to heal homun when it's below this" +

            "much hp"

            ),

        DefaultValue(0)]

        public int HealSelfHP

	{

		get { return _HealSelfHP; }

		set {

			if (value < 0)

			{

				_HealSelfHP=0;

			}

			else if (value > 100){

				_HealSelfHP=100;

			}

			else{

				_HealSelfHP= value;

			}

		}

	}





        UseAutoHealOptions _UseAutoHeal = UseAutoHealOptions.Never;

        [Category("Autobuff Options"),

        Description(

            "Choose when automatic healing is checked. Ward uses Bastion Renewal and its HP threshold; Occult uses Chaotic Heal Enabled and the Chaotic Heal HP thresholds. Enable the corresponding skill."

            ),

        DefaultValue(UseAutoHealOptions.Never)]

        public UseAutoHealOptions UseAutoHeal

        {

            get { return _UseAutoHeal; }

            set { _UseAutoHeal = value; }

        }



        // Kimi-specific autobuff options

        int _UseChaoticHeal = 1;

        [Category("Kimi Skills"),

        Description("Enable Chaotic Heal (Kimi skill) to heal owner or self automatically"),

        DefaultValue(1)]

        [Browsable(false)]
        public int UseChaoticHeal

        {

            get { return _UseChaoticHeal; }

            set { _UseChaoticHeal = value; }

        }



        int _ChaoticHealOwnerHP = 50;

        [Category("Autobuff Options"),

        Description("Use Chaotic Heal when owner HP is below this percentage"),

        DefaultValue(50)]

        public int ChaoticHealOwnerHP

        {

            get { return _ChaoticHealOwnerHP; }

            set {

                if (value < 0) { _ChaoticHealOwnerHP = 0; }

                else if (value > 100) { _ChaoticHealOwnerHP = 100; }

                else { _ChaoticHealOwnerHP = value; }

            }

        }



        int _ChaoticHealKimiHP = 50;

        [Category("Autobuff Options"),

        Description("Use Chaotic Heal when Kimi HP is below this percentage"),

        DefaultValue(50)]

        public int ChaoticHealKimiHP

        {

            get { return _ChaoticHealKimiHP; }

            set {

                if (value < 0) { _ChaoticHealKimiHP = 0; }

                else if (value > 100) { _ChaoticHealKimiHP = 100; }

                else { _ChaoticHealKimiHP = value; }

            }

        }



        [Category("Autobuff Options"), DisplayName("UseAutoBD"), DefaultValue(true),

         Description("Enable automatic Body Double. A positive owner HP threshold uses HP-based casting; threshold 0 with a positive Body Double Cast Interval uses timed casting. Requires Body Double Enabled in Skills & Behavior.")]

        public bool UseAutoBD { get { return UseBodyDouble; } set { UseBodyDouble = value; } }

        int _BodyDoubleCooldown;

        [Category("Autobuff Options"), DisplayName("Body Double Cast Interval"), DefaultValue(0),

         Description("Interval in seconds between Body Double casts. With owner HP threshold 0 and interval greater than 0, automatically cast without checking HP. With a positive threshold, cast only at or below that HP percentage. Both 0 disables automatic casting. Requires UseAutoBD and Body Double Enabled. Body Double has no built-in cooldown; its buff duration does not delay recasting. SP and current cast timing still apply.")]

        public int BodyDoubleCooldown { get { return _BodyDoubleCooldown; } set { _BodyDoubleCooldown = Math.Max(0,Math.Min(3600,value)); } }

        int _BastionRenewalCooldown;

        [Category("Autobuff Options"), DisplayName("Bastion Renewal Cooldown"), DefaultValue(0),

         Description("Seconds between Bastion Renewal cast attempts. A positive value overrides the stored 15-second cooldown; 0 uses that default. HP conditions, SP, and current cast timing still apply. The server decides whether each attempt succeeds.")]

        public int BastionRenewalCooldown { get { return _BastionRenewalCooldown; } set { _BastionRenewalCooldown = Math.Max(0,Math.Min(3600,value)); } }

        int _UseBodyDouble = 1;

        [Category("Autobuff Options"),

        Description("Enable Body Double (Kimi dies instead of master when HP critical - requires Loyal intimacy)"),

        DefaultValue(true)]

        [DisplayName("Use Body Double")]

        [Browsable(false)]

        public bool UseBodyDouble

        {

            get { return _UseBodyDouble != 0; }

            set { _UseBodyDouble = value ? 1 : 0; }

        }



        int _BodyDoubleOwnerHP = 20;

        [Category("Autobuff Options"),

        Description("Automatically cast Body Double at or below this owner HP percentage. Zero uses timed casting if Body Double Cast Interval is greater than zero; both zero disables automatic casting."),

        DefaultValue(20)]

        public int BodyDoubleOwnerHP

        {

            get { return _BodyDoubleOwnerHP; }

            set {

                if (value < 0) { _BodyDoubleOwnerHP = 0; }

                else if (value > 100) { _BodyDoubleOwnerHP = 100; }

                else { _BodyDoubleOwnerHP = value; }

            }

        }



        int _UseWarmDef = 1;

        [Category("Autobuff Options"),

        Description("Automatic Bastion Renewal setting for Ward Kimi."),

        DefaultValue(1)]

        [DisplayName("Auto Bastion Renewal")]

        [Browsable(false)]

        public int UseWarmDef

        {

            get { return _UseWarmDef; }

            set { _UseWarmDef = value; }

        }



        int _WarmDefHP = 100;

        [Category("Autobuff Options"), DisplayName("Bastion Renewal HP Threshold"), DefaultValue(100),

         Description("Automatically cast Bastion Renewal only while Ward HP is below this percentage. UseAutoHeal selects when checks run. Explicit combo casts retain their own sequence.")]

        public int WarmDefHP

        {

            get { return _WarmDefHP; }

            set { _WarmDefHP = Math.Max(0, Math.Min(100, value)); }

        }



        int _WarmDefCooldown = 30;

        [Category("Autobuff Options"),

        Description("Configured automatic Bastion Renewal interval in seconds."),

        DefaultValue(30)]

        [DisplayName("Bastion Renewal Interval")]

        [Browsable(false)]

        public int WarmDefCooldown

        {

            get { return _WarmDefCooldown; }

            set {

                if (value < 0) { _WarmDefCooldown = 0; }

                else if (value > 300) { _WarmDefCooldown = 300; }

                else { _WarmDefCooldown = value; }

            }

        }



        int _UseMasterSwap = 1;

        [Category("Autobuff Options"),

        Description("Enable Master Swap emergency skill (owner position swap) when owner HP critical"),

        DefaultValue(1)]

        [Browsable(false)]

        public int UseMasterSwap

        {

            get { return _UseMasterSwap; }

            set { _UseMasterSwap = value; }

        }



        int _MasterSwapOwnerHP = 15;

        [Category("Autobuff Options"),

        Description("Automatically cast enabled Master Swap at or below this owner HP percentage. Zero disables automatic Master Swap."),

        DefaultValue(15)]

        [Browsable(false)]

        public int MasterSwapOwnerHP

        {

            get { return _MasterSwapOwnerHP; }

            set {

                if (value < 0) { _MasterSwapOwnerHP = 0; }

                else if (value > 100) { _MasterSwapOwnerHP = 100; }

                else { _MasterSwapOwnerHP = value; }

            }

        }



        int _MasterSwapCooldown = 60;

        [Category("Autobuff Options"),

        Description("Minimum seconds between Master Swap casts (emergency skill cooldown)"),

        DefaultValue(60)]

        [Browsable(false)]

        public int MasterSwapCooldown

        {

            get { return _MasterSwapCooldown; }

            set {

                if (value < 0) { _MasterSwapCooldown = 0; }

                else if (value > 300) { _MasterSwapCooldown = 300; }

                else { _MasterSwapCooldown = value; }

            }

        }

























        #endregion



        #region Kiting Options

    bool _KiteParanoid = true;

    [Category("Kiting Options"),

    Description(

        "Set this to true if you want your mercenary to kite away from" +

        "monsters before being attacked by them"

        ),

    DefaultValue(true)]

    public bool KiteParanoid

    {

        get { return _KiteParanoid; }

        set { _KiteParanoid = value; }

    }





    int _KiteStep = 5;

    [Category("Kiting Options"),

    Description(

        "Move this many cells when kiting."

        ),

    DefaultValue(5)]

    public int KiteStep

    {

        get { return _KiteStep; }

        set

        {

            if (value < 2)

            {

                _KiteStep = 2;

            }

            else if (value > 8)

            {

                _KiteStep = 8;

            }

            else

            {

                _KiteStep = value;

            }

        }

    }





    int _KiteParanoidStep = 2;

    [Category("Kiting Options"),

    Description(

        "Move this many cells when kiting from a monster that." +

        "has not yet attacked the mercenary."

        ),

    DefaultValue(2)]

    public int KiteParanoidStep

    {

        get { return _KiteParanoidStep; }

        set

        {

            if (value < 2)

            {

                _KiteParanoidStep = 2;

            }

            else if (value > 8)

            {

                _KiteParanoidStep = 8;

            }

            else

            {

                _KiteParanoidStep = value;

            }

        }

    }





    int _KiteThreshold = 3;

    [Category("Kiting Options"),

    Description(

        "Kite when a monster is within this many cells." +

        ""

        ),

    DefaultValue(3)]

    public int KiteThreshold

    {

        get { return _KiteThreshold; }

        set

        {

            if (value < 1)

            {

                _KiteThreshold = 1;

            }

            else if (value > 8)

            {

                _KiteThreshold = 8;

            }

            else

            {

                _KiteThreshold = value;

            }

        }

    }





    int _KiteParanoidThreshold = 2;

    [Category("Kiting Options"),

    Description(

        "Kite when a monster that has not yet attacked " +

        "is within this many cells."

        ),

    DefaultValue(2)]

    public int KiteParanoidThreshold

    {

        get { return _KiteParanoidThreshold; }

        set

        {

            if (value < 1)

            {

                _KiteParanoidThreshold = 1;

            }

            else if (value > 8)

            {

                _KiteParanoidThreshold = 8;

            }

            else

            {

                _KiteParanoidThreshold = value;

            }

        }

    }





    int _KiteBounds = 8;

    [Category("Kiting Options"),

    Description(

        "When kiting do not exceed this distance from owner"

        ),

    DefaultValue(8)]

    public int KiteBounds

    {

        get { return _KiteBounds; }

        set

        {

            if (value < 0)

            {

                _KiteBounds = 0;

            }

            else if (value > 100)

            {

                _KiteBounds = 100;

            }

            else

            {

                _KiteBounds = value;

            }

        }

    }





    bool _ForceKite = false;

    [Category("Kiting Options"),

    Description(

        "Set this to true if you want your homunculus to kite monsters." +

        "even if it has no ranged attack. This may result in odd behavior"

        ),

    DefaultValue(false)]

    public bool ForceKite

    {

        get { return _ForceKite; }

        set { _ForceKite = value; }

    }

    int _FleeHP = 0;

    [Category("Kiting Options"),

    Description("If non-zero and kiting is enabled, only kite below this percent HP"),

    DefaultValue(0)]

    public int FleeHP

    {

        get { return _FleeHP; }

        set

        {

            if (value < 0)

            {

                _FleeHP = 0;

            }

            else if (value > 100)

            {

                _FleeHP = 100;

            }

            else

            {

                _FleeHP = value;

            }

        }

    }

        #endregion





        #region Friending Options

        bool _StandbyFriending = true;

        [Category("Friending Options"),

        Description(),

        DefaultValue(true)]

        public bool StandbyFriending

        {

            get { return _StandbyFriending; }

            set { _StandbyFriending = value; }

        }





        bool _MirAIFriending = false;

        [Category("Friending Options"),

        Description(

            "Set this to true if you want the homunculus to emulate MirAI " +

            "friending."

            ),

        DefaultValue(false)]

        public bool MirAIFriending

        {

            get { return _MirAIFriending; }

            set { _MirAIFriending = value; }

        }

        int _FriendAssistDamageHPThresholdOwner = 50;

        [Category("Friending Options"),

        Description("Assist owner on damage only when owner HP is at or below this percentage (0 disables gating)"),

        DefaultValue(50)]

        public int FriendAssistDamageHPThresholdOwner

        {

            get { return _FriendAssistDamageHPThresholdOwner; }

            set

            {

                if (value < 0) _FriendAssistDamageHPThresholdOwner = 0;

                else if (value > 100) _FriendAssistDamageHPThresholdOwner = 100;

                else _FriendAssistDamageHPThresholdOwner = value;

            }

        }

        #endregion





        #region Standby Options

        bool _DefendStandby = false;

        [Category("Standby Options"),

        Description(

            "Set this to true if you want your homunculus to defend you " +

            "even while in standby."

            ),

        DefaultValue(false)]

        public bool DefendStandby

        {

            get { return _DefendStandby; }

            set { _DefendStandby = value; }

        }





        StickyStandbyOptions _StickyStandby = StickyStandbyOptions.Disabled;

        [Category("Standby Options"),

        Description(

            "Set this to true if you want your homunculus to return to " +

            "standby mode after attacking, using skills, etc."

            ),

        DefaultValue(StickyStandbyOptions.Disabled)]

        public StickyStandbyOptions StickyStandby

        {

            get { return _StickyStandby; }

            set { _StickyStandby = value; }

        }

        #endregion





        #region Berserk Options

        int _UseBerserkMobbed = 0;

        [Category("Berserk Options"),

        Description(

            "If you want your homunculus to go berzerk while being " +

            "attacked, set this to a value greater than 0. Otherwise, set " +

            "it to 0 to disable berzerk mode while being mobbed."

            ),

        DefaultValue(0)]

        public int UseBerserkMobbed

        {

            get { return _UseBerserkMobbed; }

            set { _UseBerserkMobbed = value; }

        }





        bool _UseBerserkSkill = false;

        [Category("Berserk Options"),

        Description(

            "Set to true to have your homunculus go berzerk when told to " +

            "use a skill on a target."

            ),

        DefaultValue(false)]

        public bool UseBerserkSkill

        {

            get { return _UseBerserkSkill; }

            set { _UseBerserkSkill = value; }

        }





        bool _UseBerserkAttack = false;

        [Category("Berserk Options"),

        Description(

            "Set this to true to have your homunculus go berzerk when told " +

            "to attack a target."

            ),

        DefaultValue(false)]

        public bool UseBerserkAttack

        {

            get { return _UseBerserkAttack; }

            set { _UseBerserkAttack = value; }

        }





        bool _Berserk_SkillAlways = false;

        [Category("Berserk Options"),

        Description(

            "Set this to true to ignore skill use limits while in berzerk " +

            "mode."

            ),

        DefaultValue(false)]

        public bool Berserk_SkillAlways

        {

            get { return _Berserk_SkillAlways; }

            set { _Berserk_SkillAlways = value; }

        }





        bool _Berserk_Dance = false;

        [Category("Berserk Options"),

        Description(

            "Set this to true if you want your homunculus to use dance " +

            "attack while in berzerk mode."

            ),

        DefaultValue(false)]

        public bool Berserk_Dance

        {

            get { return _Berserk_Dance; }

            set { _Berserk_Dance = value; }

        }

        bool _Berserk_IgnoreMinSP = false;

        [Category("Berserk Options"),

        Description(

            "Set this to true if you want your homunculus to use dance " +

            "attack while in berzerk mode."

            ),

        DefaultValue(false)]

        public bool Berserk_IgnoreMinSP

        {

            get { return _Berserk_IgnoreMinSP; }

            set { _Berserk_IgnoreMinSP = value; }

        }

        bool _Berserk_ComboAlways = false;

        [Category("Berserk Options"),

        Description(

            "Enable this to use full combos while in berserk mode." +

            "                                                              "

            ),

        DefaultValue(false)]

        public bool Berserk_ComboAlways

        {

            get { return _Berserk_ComboAlways; }

            set { _Berserk_ComboAlways = value; }

        }





        #endregion





        #region PVP Options

        bool _PVPmode = false;

        [Category("PVP Options"),

        Description(

            "Set this to true if you want to use your homunculus in PVP " +

            "enabled maps."

            ),

        DefaultValue(false)]

        public bool PVPmode

        {

            get { return _PVPmode; }

            set { _PVPmode = value; }

        }

        #endregion

    }

}

