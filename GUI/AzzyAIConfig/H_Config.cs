// H_Config.cs
//
// Programmed by Machiavellian of iRO Chaos
//
// Description:
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AzzyAIConfig
{
    public static class H_Config
    {
        #region Save
        public static void Save(string fileName)
        {
            string file = File.ReadAllText(fileName);
            
            file = SaveBasicOptions(file);
            file = SaveAutoSkillOptions(file);
            file = SaveWalkFollowOptions(file);
            file = SaveAutobuffOptions(file);
            file = SaveSkillComboOptions(file);
            file = SaveKitingOptions(file);
            file = SaveFriendingOptions(file);
            file = SaveStandbyOptions(file);
            file = SaveBerserkOptions(file);
            file = SavePVPOptions(file);

            Program.WriteLine("Saving to file: {0}", fileName);
            File.WriteAllText(fileName, file);
            Program.WriteLine("Saving complete.");
        }

        public static void Load(string fileName)
        {
            string file = File.ReadAllText(fileName);
            
            LoadBasicOptions(file);
            LoadAutoSkillOptions(file);
            LoadWalkFollowOptions(file);
            LoadAutobuffOptions(file);
            LoadSkillComboOptions(file);
            LoadKitingOptions(file);
            LoadFriendingOptions(file);
            LoadStandbyOptions(file);
            LoadBerserkOptions(file);
            LoadPVPOptions(file);
            
            Program.WriteLine("Loading from file: {0}", fileName);
            Program.WriteLine("Loading complete.");
        }

        static string WriteConfigValue(string file, string key, int value)
        {
            string pattern = key + "\\s*=\\s*-?\\d+";
            if (Regex.IsMatch(file, pattern, RegexOptions.Multiline))
            {
                file = Regex.Replace(file, pattern, string.Format("{0,-25}= {1}", key, value), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, key, value);
            }
            return file;
        }

        static string SaveBasicOptions(string file)
        {
            Program.WriteLine("Writing Basic Options");

            file = Regex.Replace(file, "LastSavedDate\\s*=\\s*\".*\"", 
                string.Format("{0,-25}= {1}", "LastSavedDate", "\"" + DateTime.Now + "\""), 
                RegexOptions.Multiline);

            file = WriteConfigValue(file, "AggroHP", AggroHP);
            file = WriteConfigValue(file, "AggroSP", AggroSP);
            file = WriteConfigValue(file, "KiteMonsters", KiteMonsters);

            file = WriteConfigValue(file, "SuperPassive", SuperPassive);
            file = WriteConfigValue(file, "UseAttackSkill", UseAttackSkill);
            file = WriteConfigValue(file, "AssumeHomun", AssumeHomun);
            file = WriteConfigValue(file, "DoNotChase", DoNotChase);
            file = WriteConfigValue(file, "UseDanceAttack", UseDanceAttack);
            file = WriteConfigValue(file, "UseAvoid", UseAvoid);
            file = WriteConfigValue(file, "TankMonsterLimit", TankMonsterLimit);
            file = WriteConfigValue(file, "RescueOwnerLowHP", RescueOwnerLowHP);
            file = WriteConfigValue(file, "StationaryAggroDist", StationaryAggroDist);
            file = WriteConfigValue(file, "MobileAggroDist", MobileAggroDist);
            file = WriteConfigValue(file, "OldHomunType", OldHomunType);
            file = WriteConfigValue(file, "OpportunisticTargeting", OpportunisticTargeting);
            file = WriteConfigValue(file, "AttackLastFullSP", AttackLastFullSP);
            file = WriteConfigValue(file, "DanceMinSP", DanceMinSP);
            file = WriteConfigValue(file, "AttackTimeLimit", AttackTimeLimit);
            file = WriteConfigValue(file, "LagReduction", LagReduction);
            file = WriteConfigValue(file, "DoNotAttackMoving", DoNotAttackMoving);
            file = WriteConfigValue(file, "LiveMobID", LiveMobID);

            return string.Copy(file);
        }

        static string SaveAutoSkillOptions(string file)
        {
            Program.WriteLine("Writing AutoSkill Options");

            file = WriteConfigValue(file, "AttackSkillReserveSP", AttackSkillReserveSP);
            file = WriteConfigValue(file, "AutoMobCount", AutoMobCount);
            file = WriteConfigValue(file, "UseSkillOnly", UseSkillOnly);
            file = WriteConfigValue(file, "AutoSkillDelay", AutoSkillDelay);
            file = WriteConfigValue(file, "AutoSkillLimit", AutoSkillLimit);
            file = WriteConfigValue(file, "UseAutoPushback", UseAutoPushback);
            file = WriteConfigValue(file, "AutoPushbackThreshold", AutoPushbackThreshold);
            file = WriteConfigValue(file, "AoEReserveSP", AoEReserveSP);
            file = WriteConfigValue(file, "AoEFixedLevel", AoEFixedLevel);
            file = WriteConfigValue(file, "AutoMobMode", AutoMobMode);
            file = WriteConfigValue(file, "AutoComboMode", AutoComboMode);
            file = WriteConfigValue(file, "AutoComboSpheres", AutoComboSpheres);
            file = WriteConfigValue(file, "AoEMaximizeTargets", AoEMaximizeTargets);

            file = WriteConfigValue(file, "illusionOfClawsLevel", illusionOfClawsLevel);
            file = WriteConfigValue(file, "illusionOfBreathLevel", illusionOfBreathLevel);
            file = WriteConfigValue(file, "illusionOfCrusherLevel", illusionOfCrusherLevel);
            file = WriteConfigValue(file, "illusionOfLightLevel", illusionOfLightLevel);
            file = WriteConfigValue(file, "chaoticHealLevel", chaoticHealLevel);
            file = WriteConfigValue(file, "bodyDoubleLevel", bodyDoubleLevel);
            file = WriteConfigValue(file, "warmDefLevel", warmDefLevel);
            file = WriteConfigValue(file, "onlyAOE", onlyAOE);

            return string.Copy(file);
        }

        static void LoadBasicOptions(string file)
        {
            Program.WriteLine("Loading Basic Options");
            
            if (Regex.IsMatch(file, "AggroHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AggroHP = Convert.ToInt32(Regex.Match(file, "AggroHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AggroSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AggroSP = Convert.ToInt32(Regex.Match(file, "AggroSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "KiteMonsters\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                KiteMonsters = Convert.ToInt32(Regex.Match(file, "KiteMonsters\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());

            if (Regex.IsMatch(file, "SuperPassive\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                SuperPassive = Convert.ToInt32(Regex.Match(file, "SuperPassive\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseAttackSkill\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseAttackSkill = Convert.ToInt32(Regex.Match(file, "UseAttackSkill\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AssumeHomun\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AssumeHomun = Convert.ToInt32(Regex.Match(file, "AssumeHomun\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "DoNotChase\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                DoNotChase = Convert.ToInt32(Regex.Match(file, "DoNotChase\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseDanceAttack\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseDanceAttack = Convert.ToInt32(Regex.Match(file, "UseDanceAttack\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseAvoid\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseAvoid = Convert.ToInt32(Regex.Match(file, "UseAvoid\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "TankMonsterLimit\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                TankMonsterLimit = Convert.ToInt32(Regex.Match(file, "TankMonsterLimit\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "RescueOwnerLowHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                RescueOwnerLowHP = Convert.ToInt32(Regex.Match(file, "RescueOwnerLowHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "StationaryAggroDist\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                StationaryAggroDist = Convert.ToInt32(Regex.Match(file, "StationaryAggroDist\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MobileAggroDist\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MobileAggroDist = Convert.ToInt32(Regex.Match(file, "MobileAggroDist\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "OldHomunType\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                OldHomunType = Convert.ToInt32(Regex.Match(file, "OldHomunType\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "OpportunisticTargeting\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                OpportunisticTargeting = Convert.ToInt32(Regex.Match(file, "OpportunisticTargeting\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AttackLastFullSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AttackLastFullSP = Convert.ToInt32(Regex.Match(file, "AttackLastFullSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "DanceMinSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                DanceMinSP = Convert.ToInt32(Regex.Match(file, "DanceMinSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AttackTimeLimit\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AttackTimeLimit = Convert.ToInt32(Regex.Match(file, "AttackTimeLimit\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "LagReduction\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                LagReduction = Convert.ToInt32(Regex.Match(file, "LagReduction\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "DoNotAttackMoving\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                DoNotAttackMoving = Convert.ToInt32(Regex.Match(file, "DoNotAttackMoving\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "LiveMobID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                LiveMobID = Convert.ToInt32(Regex.Match(file, "LiveMobID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadAutoSkillOptions(string file)
        {
            Program.WriteLine("Loading AutoSkill Options");
            
            if (Regex.IsMatch(file, "AttackSkillReserveSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AttackSkillReserveSP = Convert.ToInt32(Regex.Match(file, "AttackSkillReserveSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseSkillOnly\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseSkillOnly = Convert.ToInt32(Regex.Match(file, "UseSkillOnly\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AutoSkillDelay\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AutoSkillDelay = Convert.ToInt32(Regex.Match(file, "AutoSkillDelay\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AutoMobMode\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AutoMobMode = Convert.ToInt32(Regex.Match(file, "AutoMobMode\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AoEReserveSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AoEReserveSP = Convert.ToInt32(Regex.Match(file, "AoEReserveSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AoEFixedLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AoEFixedLevel = Convert.ToInt32(Regex.Match(file, "AoEFixedLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AutoMobCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AutoMobCount = Convert.ToInt32(Regex.Match(file, "AutoMobCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "illusionOfClawsLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                illusionOfClawsLevel = Convert.ToInt32(Regex.Match(file, "illusionOfClawsLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "illusionOfBreathLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                illusionOfBreathLevel = Convert.ToInt32(Regex.Match(file, "illusionOfBreathLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "illusionOfCrusherLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                illusionOfCrusherLevel = Convert.ToInt32(Regex.Match(file, "illusionOfCrusherLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "illusionOfLightLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                illusionOfLightLevel = Convert.ToInt32(Regex.Match(file, "illusionOfLightLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "chaoticHealLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                chaoticHealLevel = Convert.ToInt32(Regex.Match(file, "chaoticHealLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "bodyDoubleLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                bodyDoubleLevel = Convert.ToInt32(Regex.Match(file, "bodyDoubleLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "warmDefLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                warmDefLevel = Convert.ToInt32(Regex.Match(file, "warmDefLevel\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "onlyAOE\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                onlyAOE = Convert.ToInt32(Regex.Match(file, "onlyAOE\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AutoSkillLimit\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AutoSkillLimit = Convert.ToInt32(Regex.Match(file, "AutoSkillLimit\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AoEMaximizeTargets\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AoEMaximizeTargets = Convert.ToInt32(Regex.Match(file, "AoEMaximizeTargets\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseAutoPushback\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseAutoPushback = Convert.ToInt32(Regex.Match(file, "UseAutoPushback\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "AutoPushbackThreshold\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                AutoPushbackThreshold = Convert.ToInt32(Regex.Match(file, "AutoPushbackThreshold\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadWalkFollowOptions(string file)
        {
            Program.WriteLine("Loading Walk/Follow Options");
            
            if (Regex.IsMatch(file, "FollowStayBack\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                FollowStayBack = Convert.ToInt32(Regex.Match(file, "FollowStayBack\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "RestXOff\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                RestXOff = Convert.ToInt32(Regex.Match(file, "RestXOff\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "RestYOff\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                RestYOff = Convert.ToInt32(Regex.Match(file, "RestYOff\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "DoNotUseRest\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                DoNotUseRest = Convert.ToInt32(Regex.Match(file, "DoNotUseRest\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "SpawnDelay\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                SpawnDelay = Convert.ToInt32(Regex.Match(file, "SpawnDelay\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MoveSticky\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MoveSticky = Convert.ToInt32(Regex.Match(file, "MoveSticky\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MoveStickyFight\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MoveStickyFight = Convert.ToInt32(Regex.Match(file, "MoveStickyFight\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseIdleWalk\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseIdleWalk = Convert.ToInt32(Regex.Match(file, "UseIdleWalk\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "IdleWalkSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                IdleWalkSP = Convert.ToInt32(Regex.Match(file, "IdleWalkSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseCastleRoute\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseCastleRoute = Convert.ToInt32(Regex.Match(file, "UseCastleRoute\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "RelativeRoute\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                RelativeRoute = Convert.ToInt32(Regex.Match(file, "RelativeRoute\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "IdleWalkDistance\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                IdleWalkDistance = Convert.ToInt32(Regex.Match(file, "IdleWalkDistance\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ChaseSPPause\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ChaseSPPause = Convert.ToInt32(Regex.Match(file, "ChaseSPPause\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ChaseSPPauseSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ChaseSPPauseSP = Convert.ToInt32(Regex.Match(file, "ChaseSPPauseSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ChaseSPPauseTime\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ChaseSPPauseTime = Convert.ToInt32(Regex.Match(file, "ChaseSPPauseTime\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "StationaryMoveBounds\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                StationaryMoveBounds = Convert.ToInt32(Regex.Match(file, "StationaryMoveBounds\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MobileMoveBounds\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MobileMoveBounds = Convert.ToInt32(Regex.Match(file, "MobileMoveBounds\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadAutobuffOptions(string file)
        {
            Program.WriteLine("Loading Autobuff Options");
            
            if (Regex.IsMatch(file, "HealOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                HealOwnerHP = Convert.ToInt32(Regex.Match(file, "HealOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseAutoHeal\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseAutoHeal = Convert.ToInt32(Regex.Match(file, "UseAutoHeal\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "HealSelfHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                HealSelfHP = Convert.ToInt32(Regex.Match(file, "HealSelfHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            
            // Chaotic Heal options
            if (Regex.IsMatch(file, "UseChaoticHeal\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseChaoticHeal = Convert.ToInt32(Regex.Match(file, "UseChaoticHeal\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ChaoticHealOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ChaoticHealOwnerHP = Convert.ToInt32(Regex.Match(file, "ChaoticHealOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ChaoticHealKimiHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ChaoticHealKimiHP = Convert.ToInt32(Regex.Match(file, "ChaoticHealKimiHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            
            // Body Double options
            if (Regex.IsMatch(file, "UseBodyDouble\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseBodyDouble = Convert.ToInt32(Regex.Match(file, "UseBodyDouble\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "BodyDoubleOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                BodyDoubleOwnerHP = Convert.ToInt32(Regex.Match(file, "BodyDoubleOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            
            // Warm Def options
            if (Regex.IsMatch(file, "UseWarmDef\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseWarmDef = Convert.ToInt32(Regex.Match(file, "UseWarmDef\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "WarmDefCooldown\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                WarmDefCooldown = Convert.ToInt32(Regex.Match(file, "WarmDefCooldown\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            
            // Master Swap options
            if (Regex.IsMatch(file, "UseMasterSwap\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseMasterSwap = Convert.ToInt32(Regex.Match(file, "UseMasterSwap\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MasterSwapOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MasterSwapOwnerHP = Convert.ToInt32(Regex.Match(file, "MasterSwapOwnerHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MasterSwapCooldown\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MasterSwapCooldown = Convert.ToInt32(Regex.Match(file, "MasterSwapCooldown\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadSkillComboOptions(string file)
        {
            Program.WriteLine("Loading Skill Combo Options");

            if (Regex.IsMatch(file, "ComboEnabled\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboEnabled = Convert.ToInt32(Regex.Match(file, "ComboEnabled\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "BlueprintComboEnabled\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                BlueprintComboEnabled = Convert.ToInt32(Regex.Match(file, "BlueprintComboEnabled\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboRunDuringChase\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboRunDuringChase = Convert.ToInt32(Regex.Match(file, "ComboRunDuringChase\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboRunDuringAttack\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboRunDuringAttack = Convert.ToInt32(Regex.Match(file, "ComboRunDuringAttack\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboRunDuringIdle\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboRunDuringIdle = Convert.ToInt32(Regex.Match(file, "ComboRunDuringIdle\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboResetOnTargetChange\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboResetOnTargetChange = Convert.ToInt32(Regex.Match(file, "ComboResetOnTargetChange\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboAutoAttackDelay\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboAutoAttackDelay = Convert.ToInt32(Regex.Match(file, "ComboAutoAttackDelay\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot1_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot1_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot1_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot1_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot1_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot1_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot2_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot2_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot2_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot2_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot2_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot2_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot3_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot3_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot3_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot3_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot3_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot3_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot4_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot4_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot4_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot4_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot4_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot4_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot5_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot5_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot5_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot5_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot5_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot5_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot6_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot6_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot6_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot6_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot6_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot6_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot7_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot7_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot7_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot7_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot7_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot7_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot8_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot8_SkillID = Convert.ToInt32(Regex.Match(file, "ComboSlot8_SkillID\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "ComboSlot8_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                ComboSlot8_ComboCount = Convert.ToInt32(Regex.Match(file, "ComboSlot8_ComboCount\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadKitingOptions(string file)
        {
            // Output to the console "Loading Kiting Options"
            Program.WriteLine("Loading Kiting Options");

            // Check for each Kiting Option variable in the file contents and store it
            if (Regex.IsMatch(file, "KiteParanoid\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                KiteParanoid = Convert.ToInt32(Regex.Match(file, "KiteParanoid\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
            if (Regex.IsMatch(file, "KiteStep\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                KiteStep = Convert.ToInt32(Regex.Match(file, "KiteStep\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
            if (Regex.IsMatch(file, "KiteParanoidStep\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                KiteParanoidStep = Convert.ToInt32(Regex.Match(file, "KiteParanoidStep\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
            if (Regex.IsMatch(file, "KiteThreshold\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                KiteThreshold = Convert.ToInt32(Regex.Match(file, "KiteThreshold\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
            if (Regex.IsMatch(file, "KiteParanoidThreshold\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                KiteParanoidThreshold = Convert.ToInt32(Regex.Match(file, "KiteParanoidThreshold\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
            if (Regex.IsMatch(file, "KiteBounds\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                KiteBounds = Convert.ToInt32(Regex.Match(file, "KiteBounds\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
            if (Regex.IsMatch(file, "ForceKite\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                ForceKite = Convert.ToInt32(Regex.Match(file, "ForceKite\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            } 
            if (Regex.IsMatch(file, "FleeHP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
            {
                FleeHP = Convert.ToInt32(Regex.Match(file, "FleeHP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            }
        }

        static void LoadFriendingOptions(string file)
        {
            Program.WriteLine("Loading Friending Options");
            
            if (Regex.IsMatch(file, "StandbyFriending\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                StandbyFriending = Convert.ToInt32(Regex.Match(file, "StandbyFriending\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "MirAIFriending\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                MirAIFriending = Convert.ToInt32(Regex.Match(file, "MirAIFriending\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "FriendAssistDamageHPThresholdOwner\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                FriendAssistDamageHPThresholdOwner = Convert.ToInt32(Regex.Match(file, "FriendAssistDamageHPThresholdOwner\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadStandbyOptions(string file)
        {
            Program.WriteLine("Loading Standby Options");
            
            if (Regex.IsMatch(file, "DefendStandby\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                DefendStandby = Convert.ToInt32(Regex.Match(file, "DefendStandby\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "StickyStandby\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                StickyStandby = Convert.ToInt32(Regex.Match(file, "StickyStandby\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadBerserkOptions(string file)
        {
            Program.WriteLine("Loading Berserk Options");
            
            if (Regex.IsMatch(file, "UseBerserkMobbed\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseBerserkMobbed = Convert.ToInt32(Regex.Match(file, "UseBerserkMobbed\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseBerserkSkill\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseBerserkSkill = Convert.ToInt32(Regex.Match(file, "UseBerserkSkill\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "UseBerserkAttack\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                UseBerserkAttack = Convert.ToInt32(Regex.Match(file, "UseBerserkAttack\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "Berserk_SkillAlways\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                Berserk_SkillAlways = Convert.ToInt32(Regex.Match(file, "Berserk_SkillAlways\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "Berserk_Dance\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                Berserk_Dance = Convert.ToInt32(Regex.Match(file, "Berserk_Dance\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "Berserk_IgnoreMinSP\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                Berserk_IgnoreMinSP = Convert.ToInt32(Regex.Match(file, "Berserk_IgnoreMinSP\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
            if (Regex.IsMatch(file, "Berserk_ComboAlways\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                Berserk_ComboAlways = Convert.ToInt32(Regex.Match(file, "Berserk_ComboAlways\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static void LoadPVPOptions(string file)
        {
            Program.WriteLine("Loading PVP Options");
            
            if (Regex.IsMatch(file, "PVPmode\\s*=\\s*-?\\d+", RegexOptions.Multiline))
                PVPmode = Convert.ToInt32(Regex.Match(file, "PVPmode\\s*=\\s*-?\\d+", RegexOptions.Multiline).Value.Split('=')[1].Trim());
        }

        static string SaveWalkFollowOptions(string file)
        {
            // Output to the console "Writing Walk/Follow Options"
            Program.WriteLine("Writing Walk/Follow Options");

            // Check for each Walk/Follow Option variable and update it if found
            // If a Walk/Follow Option variable is not found, add it to the file contents
            if (Regex.IsMatch(file, "FollowStayBack\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(FollowStayBack\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "FollowStayBack", FollowStayBack), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "FollowStayBack", FollowStayBack);
            }
            
            if (Regex.IsMatch(file, "RestXOff\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(RestXOff\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "RestXOff", RestXOff), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "RestXOff", RestXOff);
            }
            if (Regex.IsMatch(file, "RestYOff\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(RestYOff\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "RestYOff", RestYOff), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "RestYOff", RestYOff);
            }
            if (Regex.IsMatch(file, "DoNotUseRest\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(DoNotUseRest\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "DoNotUseRest", DoNotUseRest), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "DoNotUseRest", DoNotUseRest);
            }
            if (Regex.IsMatch(file, "SpawnDelay\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(SpawnDelay\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "SpawnDelay", SpawnDelay), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "SpawnDelay", SpawnDelay);
            }
            
            if (Regex.IsMatch(file, "MoveSticky\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(MoveSticky\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "MoveSticky", MoveSticky), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "MoveSticky", MoveSticky);
            }
            if (Regex.IsMatch(file, "MoveStickyFight\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(MoveStickyFight\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "MoveStickyFight", MoveStickyFight), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "MoveStickyFight", MoveStickyFight);
            }
            if (Regex.IsMatch(file, "UseIdleWalk\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(UseIdleWalk\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "UseIdleWalk", UseIdleWalk), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "UseIdleWalk", UseIdleWalk);
            }
            if (Regex.IsMatch(file, "IdleWalkSP\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(IdleWalkSP\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "IdleWalkSP", IdleWalkSP), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "IdleWalkSP", IdleWalkSP);
            }
            if (Regex.IsMatch(file, "UseCastleRoute\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(UseCastleRoute\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "UseCastleRoute", UseCastleRoute), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "UseCastleRoute", UseCastleRoute);
            }
            if (Regex.IsMatch(file, "RelativeRoute\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(RelativeRoute\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "RelativeRoute", RelativeRoute), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "RelativeRoute", RelativeRoute);
            }
            if (Regex.IsMatch(file, "IdleWalkDistance\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(IdleWalkDistance\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "IdleWalkDistance", IdleWalkDistance), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "IdleWalkDistance", IdleWalkDistance);
            }
            if (Regex.IsMatch(file, "ChaseSPPause\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(ChaseSPPause\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "ChaseSPPause", ChaseSPPause), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "ChaseSPPause", ChaseSPPause);
            }
            if (Regex.IsMatch(file, "ChaseSPPauseSP\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(ChaseSPPauseSP\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "ChaseSPPauseSP", ChaseSPPauseSP), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "ChaseSPPauseSP", ChaseSPPauseSP);
            }
            if (Regex.IsMatch(file, "ChaseSPPauseTime\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(ChaseSPPauseTime\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "ChaseSPPauseTime", ChaseSPPauseTime), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "ChaseSPPauseTime", ChaseSPPauseTime);
            }
            if (Regex.IsMatch(file, "StationaryMoveBounds\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(StationaryMoveBounds\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "StationaryMoveBounds", StationaryMoveBounds), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "StationaryMoveBounds", StationaryMoveBounds);
            }
            if (Regex.IsMatch(file, "MobileMoveBounds\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(MobileMoveBounds\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "MobileMoveBounds", MobileMoveBounds), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "MobileMoveBounds", MobileMoveBounds);
            }

            // Return the new file contents
            return string.Copy(file);
        }

        static string SaveAutobuffOptions(string file)
        {
            Program.WriteLine("Writing Autobuff Options");

            file = WriteConfigValue(file, "HealOwnerHP", HealOwnerHP);
            file = WriteConfigValue(file, "UseAutoHeal", UseAutoHeal);
            file = WriteConfigValue(file, "HealSelfHP", HealSelfHP);
            
            // Chaotic Heal options
            file = WriteConfigValue(file, "UseChaoticHeal", UseChaoticHeal);
            file = WriteConfigValue(file, "ChaoticHealOwnerHP", ChaoticHealOwnerHP);
            file = WriteConfigValue(file, "ChaoticHealKimiHP", ChaoticHealKimiHP);
            
            // Body Double options
            file = WriteConfigValue(file, "UseBodyDouble", UseBodyDouble);
            file = WriteConfigValue(file, "BodyDoubleOwnerHP", BodyDoubleOwnerHP);
            
            // Warm Def options
            file = WriteConfigValue(file, "UseWarmDef", UseWarmDef);
            file = WriteConfigValue(file, "WarmDefCooldown", WarmDefCooldown);
            
            // Master Swap options
            file = WriteConfigValue(file, "UseMasterSwap", UseMasterSwap);
            file = WriteConfigValue(file, "MasterSwapOwnerHP", MasterSwapOwnerHP);
            file = WriteConfigValue(file, "MasterSwapCooldown", MasterSwapCooldown);

            return string.Copy(file);
        }

        static string SaveSkillComboOptions(string file)
        {
            Program.WriteLine("Writing Skill Combo Options");

            file = WriteConfigValue(file, "ComboEnabled", ComboEnabled);
            file = WriteConfigValue(file, "BlueprintComboEnabled", BlueprintComboEnabled);
            file = WriteConfigValue(file, "ComboRunDuringChase", ComboRunDuringChase);
            file = WriteConfigValue(file, "ComboRunDuringAttack", ComboRunDuringAttack);
            file = WriteConfigValue(file, "ComboRunDuringIdle", ComboRunDuringIdle);
            file = WriteConfigValue(file, "ComboResetOnTargetChange", ComboResetOnTargetChange);
            file = WriteConfigValue(file, "ComboAutoAttackDelay", ComboAutoAttackDelay);
            file = WriteConfigValue(file, "ComboSlot1_SkillID", ComboSlot1_SkillID);
            file = WriteConfigValue(file, "ComboSlot1_ComboCount", ComboSlot1_ComboCount);
            file = WriteConfigValue(file, "ComboSlot2_SkillID", ComboSlot2_SkillID);
            file = WriteConfigValue(file, "ComboSlot2_ComboCount", ComboSlot2_ComboCount);
            file = WriteConfigValue(file, "ComboSlot3_SkillID", ComboSlot3_SkillID);
            file = WriteConfigValue(file, "ComboSlot3_ComboCount", ComboSlot3_ComboCount);
            file = WriteConfigValue(file, "ComboSlot4_SkillID", ComboSlot4_SkillID);
            file = WriteConfigValue(file, "ComboSlot4_ComboCount", ComboSlot4_ComboCount);
            file = WriteConfigValue(file, "ComboSlot5_SkillID", ComboSlot5_SkillID);
            file = WriteConfigValue(file, "ComboSlot5_ComboCount", ComboSlot5_ComboCount);
            file = WriteConfigValue(file, "ComboSlot6_SkillID", ComboSlot6_SkillID);
            file = WriteConfigValue(file, "ComboSlot6_ComboCount", ComboSlot6_ComboCount);
            file = WriteConfigValue(file, "ComboSlot7_SkillID", ComboSlot7_SkillID);
            file = WriteConfigValue(file, "ComboSlot7_ComboCount", ComboSlot7_ComboCount);
            file = WriteConfigValue(file, "ComboSlot8_SkillID", ComboSlot8_SkillID);
            file = WriteConfigValue(file, "ComboSlot8_ComboCount", ComboSlot8_ComboCount);

            return string.Copy(file);
        }

        static string SaveKitingOptions(string file)
        {
            // Output to the console "Writing Kiting Options"
            Program.WriteLine("Writing Kiting Options");

            // Check for each Kiting Option variable and update it if found
            // If a Kiting Option variable is not found, add it to the file contents
            if (Regex.IsMatch(file, "KiteParanoid\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(KiteParanoid\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "KiteParanoid", KiteParanoid), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "KiteParanoid", KiteParanoid);
            }
            if (Regex.IsMatch(file, "KiteStep\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(KiteStep\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "KiteStep", KiteStep), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "KiteStep", KiteStep);
            }
            if (Regex.IsMatch(file, "KiteParanoidStep\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(KiteParanoidStep\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "KiteParanoidStep", KiteParanoidStep), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "KiteParanoidStep", KiteParanoidStep);
            }
            if (Regex.IsMatch(file, "KiteThreshold\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(KiteThreshold\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "KiteThreshold", KiteThreshold), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "KiteThreshold", KiteThreshold);
            }
            if (Regex.IsMatch(file, "KiteParanoidThreshold\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(KiteParanoidThreshold\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "KiteParanoidThreshold", KiteParanoidThreshold), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "KiteParanoidThreshold", KiteParanoidThreshold);
            }
            if (Regex.IsMatch(file, "KiteBounds\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(KiteBounds\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "KiteBounds", KiteBounds), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "KiteBounds", KiteBounds);
            }
            if (Regex.IsMatch(file, "ForceKite\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(ForceKite\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "ForceKite", ForceKite), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "ForceKite", ForceKite);
            }
            if (Regex.IsMatch(file, "FleeHP\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(FleeHP\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "FleeHP", FleeHP), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "FleeHP", FleeHP);
            }
            // Return the new file contents
            return string.Copy(file);
        }

        static string SaveFriendingOptions(string file)
        {
            // Output to the console "Writing Friending Options"
            Program.WriteLine("Writing Friending Options");

            // Check for each Friending Option variable and update it if found
            // If a Friending Option variable is not found, add it to the file contents
            if (Regex.IsMatch(file, "StandbyFriending\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(StandbyFriending\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "StandbyFriending", StandbyFriending), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "StandbyFriending", StandbyFriending);
            }
            if (Regex.IsMatch(file, "MirAIFriending\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(MirAIFriending\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "MirAIFriending", MirAIFriending), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "MirAIFriending", MirAIFriending);
            }

            // Friend assist HP gating threshold
            file = WriteConfigValue(file, "FriendAssistDamageHPThresholdOwner", FriendAssistDamageHPThresholdOwner);

            // Return the new file contents
            return string.Copy(file);
        }

        static string SaveStandbyOptions(string file)
        {
            // Output to the console "Writing Standby Options"
            Program.WriteLine("Writing Standby Options");

            // Check for each Standby Option variable and update it if found
            // If a Standby Option variable is not found, add it to the file contents
            if (Regex.IsMatch(file, "DefendStandby\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(DefendStandby\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "DefendStandby", DefendStandby), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "DefendStandby", DefendStandby);
            }
            if (Regex.IsMatch(file, "StickyStandby\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(StickyStandby\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "StickyStandby", StickyStandby), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "StickyStandby", StickyStandby);
            }

            // Return the new file contents
            return string.Copy(file);
        }

        static string SaveBerserkOptions(string file)
        {
            // Output to the console "Writing Berserk Options"
            Program.WriteLine("Writing Berserk Options");

            // Check for each Berserk Option variable and update it if found
            // If a Berserk Option variable is not found, add it to the file contents
            if (Regex.IsMatch(file, "UseBerserkMobbed\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(UseBerserkMobbed\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "UseBerserkMobbed", UseBerserkMobbed), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "UseBerserkMobbed", UseBerserkMobbed);
            }
            if (Regex.IsMatch(file, "UseBerserkSkill\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(UseBerserkSkill\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "UseBerserkSkill", UseBerserkSkill), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "UseBerserkSkill", UseBerserkSkill);
            }
            if (Regex.IsMatch(file, "UseBerserkAttack\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(UseBerserkAttack\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "UseBerserkAttack", UseBerserkAttack), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "UseBerserkAttack", UseBerserkAttack);
            }
            if (Regex.IsMatch(file, "Berserk_SkillAlways\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(Berserk_SkillAlways\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "Berserk_SkillAlways", Berserk_SkillAlways), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "Berserk_SkillAlways", Berserk_SkillAlways);
            }
            if (Regex.IsMatch(file, "Berserk_Dance\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(Berserk_Dance\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "Berserk_Dance", Berserk_Dance), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "Berserk_Dance", Berserk_Dance);
            } 
            if (Regex.IsMatch(file, "Berserk_IgnoreMinSP\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(Berserk_IgnoreMinSP\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "Berserk_IgnoreMinSP", Berserk_IgnoreMinSP), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "Berserk_IgnoreMinSP", Berserk_IgnoreMinSP);
            }
            if (Regex.IsMatch(file, "Berserk_ComboAlways\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(Berserk_ComboAlways\\s*=\\s*)(\\d+|-\\+)", string.Format("{0,-25}= {1}", "Berserk_ComboAlways", Berserk_ComboAlways), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "Berserk_ComboAlways", Berserk_ComboAlways);
            }
            // Return the new file contents
            return string.Copy(file);
        }

        static string SavePVPOptions(string file)
        {
            // Output to the console "Writing PVP Options"
            Program.WriteLine("Writing PVP Options");

            // Check for each PVP Option variable and update it if found
            // If a PVP Option variable is not found, add it to the file contents
            if (Regex.IsMatch(file, "PVPmode\\s*=\\s*(\\d+|-\\d+)", RegexOptions.Multiline))
            {
                file = Regex.Replace(file, "(PVPmode\\s*=\\s*)(\\d+|-\\d+)", string.Format("{0,-25}= {1}", "PVPmode", PVPmode), RegexOptions.Multiline);
            }
            else
            {
                file = string.Format("{1}{0}{2,-25}= {3}", Environment.NewLine, file, "PVPmode", PVPmode);
            }

            // Return the new file contents
            return string.Copy(file);
        }
        #endregion

        #region Basic Options
        /////////////////
        //Basic Options//
        /////////////////
        //AggroHP: The mercenary will be aggressive when it's current HP is greater than this percent of it's maximum HP
        //To make your homun behave in a passive manner, set this to 100. 
        //Set AggroDistance to the maximum distance you want your merc to attack when aggressive.
        //If SuperPassive is set to 1, the AI will be completely passive, and will not attack in any way
        //This is intended for the purpose of getting killcount or using the AI for utility purposes.
        //Set KiteMonsters to 1 to attempt to avoid or "kite" monsters if you have an archer mercenary 
        //Set UseAttackSkill to 1 to automatically use offencive skills while attacking.
        /////////////////
        static int _AggroHP = 60;
        public static int AggroHP //Set this to 100 for passive mode (only attack monsters targeting owner/friends.
        {
            get { return _AggroHP; }
            set { _AggroHP = value; }
        }
        static int _AggroSP = 0;
        public static int AggroSP //As above for SP. 
        {
            get { return _AggroSP; }
            set { _AggroSP = value; }
        }
        static int _KiteMonsters = 0;
        public static int KiteMonsters
        {
            get { return _KiteMonsters; }
            set { _KiteMonsters = value; }
        }
        static int _PainkillerFriends = 0;
        [System.ComponentModel.Browsable(false)]
        public static int PainkillerFriends
        {
            get { return _PainkillerFriends; }
            set { _PainkillerFriends = value; }
        }
        static int _PainkillerFriendsSave = 0;
        [System.ComponentModel.Browsable(false)]
        public static int PainkillerFriendsSave
        {
            get { return _PainkillerFriendsSave; }
            set { _PainkillerFriendsSave = value; }
        }
        static int _SuperPassive = 0;
        public static int SuperPassive
        {
            get { return _SuperPassive; }
            set { _SuperPassive = value; }
        }
        static int _UseAttackSkill = 1;
        public static int UseAttackSkill
        {
            get { return _UseAttackSkill; }
            set { _UseAttackSkill = value; }
        }
        static int _AssumeHomun = 1;
        public static int AssumeHomun //Set this to 1 if you have a homun. 
        {
            get { return _AssumeHomun; }
            set { _AssumeHomun = value; }
        }
        static int _DoNotChase = 0;
        public static int DoNotChase
        {
            get { return _DoNotChase; }
            set { _DoNotChase = value; }
        }
        static int _UseDanceAttack = 0;
        public static int UseDanceAttack
        {
            get { return _UseDanceAttack; }
            set { _UseDanceAttack = value; }
        }
        static int _UseAvoid = 0;
        public static int UseAvoid
        {
            get { return _UseAvoid; }
            set { _UseAvoid = value; }
        }
        static int _TankMonsterLimit = 4;
        public static int TankMonsterLimit
        {
            get { return _TankMonsterLimit; }
            set { _TankMonsterLimit = value; }
        }
        static int _RescueOwnerLowHP = 0;
        public static int RescueOwnerLowHP
        {
            get { return _RescueOwnerLowHP; }
            set { _RescueOwnerLowHP = value; }
        }
        static int _StationaryAggroDist = 0;
        public static int StationaryAggroDist
        {
            get { return _StationaryAggroDist; }
            set { _StationaryAggroDist = value; }
        }
        static int _MobileAggroDist = 0;
        public static int MobileAggroDist
        {
            get { return _MobileAggroDist; }
            set { _MobileAggroDist = value; }
        }
        static int _OldHomunType = 0;
        public static int OldHomunType
        {
            get { return _OldHomunType; }
            set { _OldHomunType = value; }
        }
        static int _OpportunisticTargeting = 0;
        public static int OpportunisticTargeting
        {
            get { return _OpportunisticTargeting; }
            set { _OpportunisticTargeting = value; }
        }
        static int _AttackLastFullSP = 0;
        public static int AttackLastFullSP
        {
            get { return _AttackLastFullSP; }
            set { _AttackLastFullSP = value; }
        }
        static int _DanceMinSP = 0;
        public static int DanceMinSP
        {
            get { return _DanceMinSP; }
            set { _DanceMinSP = value; }
        }
        static int _AttackTimeLimit = 60;
        public static int AttackTimeLimit //Set this to 100 for passive mode (only attack monsters targeting owner/friends.
        {
            get { return _AttackTimeLimit; }
            set { _AttackTimeLimit = value; }
        }
        static int _LagReduction = 0;
        public static int LagReduction
        {
            get { return _LagReduction; }
            set { _LagReduction = value; }
        }
        static int _DoNotAttackMoving = 0;
        public static int DoNotAttackMoving
        {
            get { return _DoNotAttackMoving; }
            set { _DoNotAttackMoving = value; }
        }

        static int _LiveMobID = 0;
        public static int LiveMobID
        {
            get { return _LiveMobID; }
            set { _LiveMobID = value; }
        }
        #endregion

        #region AutoSkill Options
        /////////////////////
        //AutoSkill Options//
        /////////////////////
        // Kimi Skill Levels (0-10, 0 = disabled)
        static int _illusionOfClawsLevel = 5;
        public static int illusionOfClawsLevel
        {
            get { return _illusionOfClawsLevel; }
            set { _illusionOfClawsLevel = value; }
        }
        static int _illusionOfBreathLevel = 10;
        public static int illusionOfBreathLevel
        {
            get { return _illusionOfBreathLevel; }
            set { _illusionOfBreathLevel = value; }
        }
        static int _illusionOfCrusherLevel = 0;
        public static int illusionOfCrusherLevel
        {
            get { return _illusionOfCrusherLevel; }
            set { _illusionOfCrusherLevel = value; }
        }
        static int _illusionOfLightLevel = 0;
        public static int illusionOfLightLevel
        {
            get { return _illusionOfLightLevel; }
            set { _illusionOfLightLevel = value; }
        }
        static int _chaoticHealLevel = 5;
        public static int chaoticHealLevel
        {
            get { return _chaoticHealLevel; }
            set { _chaoticHealLevel = value; }
        }
        static int _bodyDoubleLevel = 5;
        public static int bodyDoubleLevel
        {
            get { return _bodyDoubleLevel; }
            set { _bodyDoubleLevel = value; }
        }
        static int _warmDefLevel = 0;
        public static int warmDefLevel
        {
            get { return _warmDefLevel; }
            set { _warmDefLevel = value; }
        }
        static int _onlyAOE = 0;
        public static int onlyAOE
        {
            get { return _onlyAOE; }
            set { _onlyAOE = value; }
        }

        // Skill Combo Options
        static int _ComboEnabled = 0;
        public static int ComboEnabled
        {
            get { return _ComboEnabled; }
            set { _ComboEnabled = value; }
        }
        static int _BlueprintComboEnabled = 0;
        [System.ComponentModel.Description("Enable Blueprint Combo System - visual combo editor with node-based flow. Exported combos will execute during gameplay.")]
        [System.ComponentModel.Category("Blueprint Combos")]
        public static int BlueprintComboEnabled
        {
            get { return _BlueprintComboEnabled; }
            set { _BlueprintComboEnabled = value; }
        }
        static int _ComboRunDuringChase = 1;
        public static int ComboRunDuringChase
        {
            get { return _ComboRunDuringChase; }
            set { _ComboRunDuringChase = value; }
        }
        static int _ComboRunDuringAttack = 1;
        public static int ComboRunDuringAttack
        {
            get { return _ComboRunDuringAttack; }
            set { _ComboRunDuringAttack = value; }
        }
        static int _ComboRunDuringIdle = 0;
        public static int ComboRunDuringIdle
        {
            get { return _ComboRunDuringIdle; }
            set { _ComboRunDuringIdle = value; }
        }
        static int _ComboResetOnTargetChange = 1;
        public static int ComboResetOnTargetChange
        {
            get { return _ComboResetOnTargetChange; }
            set { _ComboResetOnTargetChange = value; }
        }
        static int _ComboAutoAttackDelay = 200;
        public static int ComboAutoAttackDelay
        {
            get { return _ComboAutoAttackDelay; }
            set { _ComboAutoAttackDelay = value; }
        }
        static int _ComboSlot1_SkillID = 0;
        public static int ComboSlot1_SkillID
        {
            get { return _ComboSlot1_SkillID; }
            set { _ComboSlot1_SkillID = value; }
        }
        static int _ComboSlot1_ComboCount = 1;
        public static int ComboSlot1_ComboCount
        {
            get { return _ComboSlot1_ComboCount; }
            set { _ComboSlot1_ComboCount = value; }
        }
        static int _ComboSlot2_SkillID = 0;
        public static int ComboSlot2_SkillID
        {
            get { return _ComboSlot2_SkillID; }
            set { _ComboSlot2_SkillID = value; }
        }
        static int _ComboSlot2_ComboCount = 1;
        public static int ComboSlot2_ComboCount
        {
            get { return _ComboSlot2_ComboCount; }
            set { _ComboSlot2_ComboCount = value; }
        }
        static int _ComboSlot3_SkillID = 0;
        public static int ComboSlot3_SkillID
        {
            get { return _ComboSlot3_SkillID; }
            set { _ComboSlot3_SkillID = value; }
        }
        static int _ComboSlot3_ComboCount = 1;
        public static int ComboSlot3_ComboCount
        {
            get { return _ComboSlot3_ComboCount; }
            set { _ComboSlot3_ComboCount = value; }
        }
        static int _ComboSlot4_SkillID = 0;
        public static int ComboSlot4_SkillID
        {
            get { return _ComboSlot4_SkillID; }
            set { _ComboSlot4_SkillID = value; }
        }
        static int _ComboSlot4_ComboCount = 1;
        public static int ComboSlot4_ComboCount
        {
            get { return _ComboSlot4_ComboCount; }
            set { _ComboSlot4_ComboCount = value; }
        }
        static int _ComboSlot5_SkillID = 0;
        public static int ComboSlot5_SkillID
        {
            get { return _ComboSlot5_SkillID; }
            set { _ComboSlot5_SkillID = value; }
        }
        static int _ComboSlot5_ComboCount = 0;
        public static int ComboSlot5_ComboCount
        {
            get { return _ComboSlot5_ComboCount; }
            set { _ComboSlot5_ComboCount = value; }
        }
        static int _ComboSlot6_SkillID = 0;
        public static int ComboSlot6_SkillID
        {
            get { return _ComboSlot6_SkillID; }
            set { _ComboSlot6_SkillID = value; }
        }
        static int _ComboSlot6_ComboCount = 0;
        public static int ComboSlot6_ComboCount
        {
            get { return _ComboSlot6_ComboCount; }
            set { _ComboSlot6_ComboCount = value; }
        }
        static int _ComboSlot7_SkillID = 0;
        public static int ComboSlot7_SkillID
        {
            get { return _ComboSlot7_SkillID; }
            set { _ComboSlot7_SkillID = value; }
        }
        static int _ComboSlot7_ComboCount = 0;
        public static int ComboSlot7_ComboCount
        {
            get { return _ComboSlot7_ComboCount; }
            set { _ComboSlot7_ComboCount = value; }
        }
        static int _ComboSlot8_SkillID = 0;
        public static int ComboSlot8_SkillID
        {
            get { return _ComboSlot8_SkillID; }
            set { _ComboSlot8_SkillID = value; }
        }
        static int _ComboSlot8_ComboCount = 0;
        public static int ComboSlot8_ComboCount
        {
            get { return _ComboSlot8_ComboCount; }
            set { _ComboSlot8_ComboCount = value; }
        }
        
        //If you want the mercenary to always keep a certain amount of SP (ex. for quicken skills), set AttackSkillReserveSP to that amount
        //To automatically use anti-mob skills on multiple monsters, set AutoMobCount to the minimum 
        //number of monsters to use that on. Otherwise, set it to something really big. 
        //Do not change AutoSkillDelay unless you really think you know what you're doing.
        //Set UseSkillOnly to never use normal attacks, can be useful because normal attacks
        //are treated differently by monsters for the purposes of target changing. 
        //UseSkillOnly will be ignored if UseAttackSkill is set to 0. 
        /////////////////////
        static int _AttackSkillReserveSP = 0;
        public static int AttackSkillReserveSP
        {
            get { return _AttackSkillReserveSP; }
            set { _AttackSkillReserveSP = value; }
        }
        static int _AutoMobCount = 2;
        public static int AutoMobCount
        {
            get { return _AutoMobCount; }
            set { _AutoMobCount = value; }
        }
        static int _UseSkillOnly = -1;
        public static int UseSkillOnly //Set to 0 to only use skills when attacking normally, -1 to skill while attacking or chasing, and 1 to only use skills. 
        {
            get { return _UseSkillOnly; }
            set { _UseSkillOnly = value; }
        }
        static int _AutoSkillDelay = 400;
        public static int AutoSkillDelay //Leave at 400
        {
            get { return _AutoSkillDelay; }
            set { _AutoSkillDelay = value; }
        }
        static int _AutoSkillLimit = 100;
        public static int AutoSkillLimit //Default number of times to use an attack skill on a single monster, if not specified otherwise in the tact file. Tact list overrides this.
        {
            get { return _AutoSkillLimit; }
            set { _AutoSkillLimit = value; }
        }
        static int _UseAutoPushback = 0;
        public static int UseAutoPushback //Set to 1 to autouse pushback skills on mobs targeting merc. Set to 2 to use on monsters targeting owner/friends too (the latter is not reccomended)
        {
            get { return _UseAutoPushback; }
            set { _UseAutoPushback = value; }
        }
        static int _AutoPushbackThreshold = 2;
        public static int AutoPushbackThreshold //This is the distance between the monster and the target for use of pushback
        {
            get { return _AutoPushbackThreshold; }
            set { _AutoPushbackThreshold = value; }
        }

        static int _AoEReserveSP = 0;
        public static int AoEReserveSP
        {
            get { return _AoEReserveSP; }
            set { _AoEReserveSP = value; }
        }
        static int _AoEFixedLevel = 0;
        public static int AoEFixedLevel
        {
            get { return _AoEFixedLevel; }
            set { _AoEFixedLevel = value; }
        }
        static int _AutoMobMode = 0;
        public static int AutoMobMode
        {
            get { return _AutoMobMode; }
            set { _AutoMobMode = value; }
        }
        static int _AutoComboMode = 0;
        public static int AutoComboMode
        {
            get { return _AutoComboMode; }
            set { _AutoComboMode = value; }
        }
        static int _AutoComboSpheres = 0;
        public static int AutoComboSpheres
        {
            get { return _AutoComboSpheres; }
            set { _AutoComboSpheres = value; }
        }
        static int _AoEMaximizeTargets = 0;
        public static int AoEMaximizeTargets
        {
            get { return _AoEMaximizeTargets; }
            set { _AoEMaximizeTargets = value; }
        }
        #endregion

        #region Walk/Follow Options
        ///////////////////////
        //Walk/Follow Options//
        ///////////////////////
        //To make your mercenary stay x cells away from you, set FollowStayBack to x
        //MoveBounds is the max distance from you that your merc will get without immediately trying to return to you. 
        //To make your mercenary orbit you while idle at full HP, set UseOrbitWalk to the radius
        //which you want it to orbit at. This is kinda pointless and is not reccomended.
        //To make your mercenary walk randomly when above AggroHP, set UseRandomWalk to 1, and autofollow (ctrl+shift+rclick) your merc.
        ///////////////////////
        static int _FollowStayBack = 2;
        public static int FollowStayBack // Reccomend to set this to 1 or 2 for melees, 2 or 3 for archer
        {
            get { return _FollowStayBack; }
            set { _FollowStayBack = value; }
        }
        
        static int _RestXOff = -2;
        public static int RestXOff
        {
            get { return _RestXOff; }
            set { _RestXOff = value; }
        }
        static int _RestYOff = 0;
        public static int RestYOff // Set these to the coordinate offset you want your merc to move to when you sit. 
        {
            get { return _RestYOff; }
            set { _RestYOff = value; }
        }
        static int _DoNotUseRest = 0;
        public static int DoNotUseRest // Disable rest feature
        {
            get { return _DoNotUseRest; }
            set { _DoNotUseRest = value; }
        }
        static int _SpawnDelay = 1000;
        public static int SpawnDelay
        {
            get { return _SpawnDelay; }
            set { _SpawnDelay = value; }
        }
        static int _MoveSticky = 0;
        public static int MoveSticky // Set to 1 to stay where told to move to
        {
            get { return _MoveSticky; }
            set { _MoveSticky = value; }
        }
        static int _MoveStickyFight = 0;
        public static int MoveStickyFight // Set to 1 to fight normally in above mode
        {
            get { return _MoveStickyFight; }
            set { _MoveStickyFight = value; }
        }

        static int _UseIdleWalk = 0;
        public static int UseIdleWalk
        {
            get { return _UseIdleWalk; }
            set { _UseIdleWalk = value; }
        }
        
        static int _IdleWalkSP = 0;
        public static int IdleWalkSP
        {
            get { return _IdleWalkSP; }
            set { _IdleWalkSP = value; }
        }
        static int _UseCastleRoute = 0;
        public static int UseCastleRoute
        {
            get { return _UseCastleRoute; }
            set { _UseCastleRoute = value; }
        }
        static int _RelativeRoute = 0;
        public static int RelativeRoute
        {
            get { return _RelativeRoute; }
            set { _RelativeRoute = value; }
        }
        static int _IdleWalkDistance = 0;
        public static int IdleWalkDistance
        {
            get { return _IdleWalkDistance; }
            set { _IdleWalkDistance = value; }
        }
        static int _ChaseSPPause = 0;
        public static int ChaseSPPause
        {
            get { return _ChaseSPPause; }
            set { _ChaseSPPause = value; }
        }
        static int _ChaseSPPauseSP = 0;
        public static int ChaseSPPauseSP
        {
            get { return _ChaseSPPauseSP; }
            set { _ChaseSPPauseSP = value; }
        }
        static int _ChaseSPPauseTime = 0;
        public static int ChaseSPPauseTime
        {
            get { return _ChaseSPPauseTime; }
            set { _ChaseSPPauseTime = value; }
        }
        static int _StationaryMoveBounds = 0;
        public static int StationaryMoveBounds
        {
            get { return _StationaryMoveBounds; }
            set { _StationaryMoveBounds = value; }
        }
        static int _MobileMoveBounds = 0;
        public static int MobileMoveBounds
        {
            get { return _MobileMoveBounds; }
            set { _MobileMoveBounds = value; }
        }

        #endregion

        #region Autobuff Options
        ////////////////////
        //Autobuff options//
        ////////////////////
        //To automatically use these skills so as to keep them up at all times
        //set UseAuto(skill) to 1. 
        //AutoSight will only proc when hidden monsters or players are on screen
        //(Currently disabled - if UseAutoSight is on, sight will be kept up always)
        ////////////////////
        static int _HealOwnerHP = 50;
        public static int HealOwnerHP 
        {
            get { return _HealOwnerHP; }
            set { _HealOwnerHP = value; }
        }
        static int _UseAutoHeal = 0;
        public static int UseAutoHeal
        {
            get { return _UseAutoHeal; }
            set { _UseAutoHeal = value; }
        }
        static int _HealSelfHP = 0;
        public static int HealSelfHP
        {
            get { return _HealSelfHP; }
            set { _HealSelfHP = value; }
        }

        // Chaotic Heal options
        static int _UseChaoticHeal = 1;
        public static int UseChaoticHeal
        {
            get { return _UseChaoticHeal; }
            set { _UseChaoticHeal = value; }
        }
        static int _ChaoticHealOwnerHP = 50;
        public static int ChaoticHealOwnerHP
        {
            get { return _ChaoticHealOwnerHP; }
            set { _ChaoticHealOwnerHP = value; }
        }
        static int _ChaoticHealKimiHP = 50;
        public static int ChaoticHealKimiHP
        {
            get { return _ChaoticHealKimiHP; }
            set { _ChaoticHealKimiHP = value; }
        }

        // Body Double options
        static int _UseBodyDouble = 1;
        public static int UseBodyDouble
        {
            get { return _UseBodyDouble; }
            set { _UseBodyDouble = value; }
        }
        static int _BodyDoubleOwnerHP = 20;
        public static int BodyDoubleOwnerHP
        {
            get { return _BodyDoubleOwnerHP; }
            set { _BodyDoubleOwnerHP = value; }
        }

        // Warm Def options
        static int _UseWarmDef = 1;
        public static int UseWarmDef
        {
            get { return _UseWarmDef; }
            set { _UseWarmDef = value; }
        }
        static int _WarmDefCooldown = 30;
        public static int WarmDefCooldown
        {
            get { return _WarmDefCooldown; }
            set { _WarmDefCooldown = value; }
        }

        // Master Swap options (8005) - Emergency position swap
        static int _UseMasterSwap = 1;
        public static int UseMasterSwap
        {
            get { return _UseMasterSwap; }
            set { _UseMasterSwap = value; }
        }
        static int _MasterSwapOwnerHP = 15;
        public static int MasterSwapOwnerHP
        {
            get { return _MasterSwapOwnerHP; }
            set { _MasterSwapOwnerHP = value; }
        }
        static int _MasterSwapCooldown = 60;
        public static int MasterSwapCooldown
        {
            get { return _MasterSwapCooldown; }
            set { _MasterSwapCooldown = value; }
        }


        #endregion

        #region Kiting Options
        //////////////////
        //Kiting Options//
        //////////////////
        //Set ForceKite to use attempt kiting for all mercenaries, even non archers. Will behave strangely. 
        //KiteStep is the distance the mercenary will move in each kiting attempt, while KiteThreshold 
        //the distance between merc and monster at which it will attempt to kite. 
        //KiteBound is the maximum distance that the merc will ever move from the owner when kiting
        //Set KiteParanoid to 1 to attempt to kite monsters that are not currently attacking.
        //KiteParanoidStep and KiteParanoidThreshold have the same purpose as KiteStep and KiteThreshold
        //////////////////
        static int _KiteParanoid = 1;
        public static int KiteParanoid //Set to 1 if the map you're on has aggressive monsters that own your merc.
        {
            get { return _KiteParanoid; }
            set { _KiteParanoid = value; }
        }
        static int _KiteStep = 5;
        public static int KiteStep //It is not reccomended to change these without good reason
        {
            get { return _KiteStep; }
            set { _KiteStep = value; }
        }
        static int _KiteParanoidStep = 2;
        public static int KiteParanoidStep //Theres a bit of trial and error to setting these
        {
            get { return _KiteParanoidStep; }
            set { _KiteParanoidStep = value; }
        }
        static int _KiteThreshold = 3;
        public static int KiteThreshold //such that the mercenary acts in a desirable manner
        {
            get { return _KiteThreshold; }
            set { _KiteThreshold = value; }
        }
        static int _KiteParanoidThreshold = 2;
        public static int KiteParanoidThreshold
        {
            get { return _KiteParanoidThreshold; }
            set { _KiteParanoidThreshold = value; }
        }
        static int _KiteBounds = 8;
        public static int KiteBounds
        {
            get { return _KiteBounds; }
            set { _KiteBounds = value; }
        }
        static int _ForceKite = 0;
        public static int ForceKite // Not reccomended
        {
            get { return _ForceKite; }
            set { _ForceKite = value; }
        }
        static int _FleeHP = 8;
        public static int FleeHP
        {
            get { return _FleeHP; }
            set { _FleeHP = value; }
        }
        #endregion

        #region Friending Options
        /////////////////////
        //Friending Options//
        /////////////////////
        static int _StandbyFriending = 1;
        public static int StandbyFriending
        {
            get { return _StandbyFriending; }
            set { _StandbyFriending = value; }
        }
        static int _MirAIFriending = 0;
        public static int MirAIFriending
        {
            get { return _MirAIFriending; }
            set { _MirAIFriending = value; }
        }
        static int _FriendAssistDamageHPThresholdOwner = 50;
        public static int FriendAssistDamageHPThresholdOwner
        {
            get { return _FriendAssistDamageHPThresholdOwner; }
            set { _FriendAssistDamageHPThresholdOwner = value; }
        }
        #endregion

        #region Standby Options
        ///////////////////
        //Standby Options//
        ///////////////////
        static int _DefendStandby = 0;
        public static int DefendStandby //Set to 1 to defend while in standby
        {
            get { return _DefendStandby; }
            set { _DefendStandby = value; }
        }
        static int _StickyStandby = 0;
        public static int StickyStandby //Set to 1 to return to standby after doing other things (ex: defending)
        {
            get { return _StickyStandby; }
            set { _StickyStandby = value; }
        }
        #endregion

        #region Berserk Options
        ///////////////////
        //Berserk Options//
        ///////////////////
        static int _UseBerserkMobbed = 0;
        public static int UseBerserkMobbed //Set to a number other than 0 to go berzerk when this many monsters are attacking
        {
            get { return _UseBerserkMobbed; }
            set { _UseBerserkMobbed = value; }
        }
        static int _UseBerserkSkill = 0;
        public static int UseBerserkSkill //Set to 1 to go berzerk when told to use a skill on target
        {
            get { return _UseBerserkSkill; }
            set { _UseBerserkSkill = value; }
        }
        static int _UseBerserkAttack = 0;
        public static int UseBerserkAttack //Set to 1 to go berzerk when told to attack a target
        {
            get { return _UseBerserkAttack; }
            set { _UseBerserkAttack = value; }
        }
        static int _Berserk_SkillAlways = 0;
        public static int Berserk_SkillAlways //Set to 1 to ignore skill use limits when berzerk
        {
            get { return _Berserk_SkillAlways; }
            set { _Berserk_SkillAlways = value; }
        }
        static int _Berserk_Dance = 0;
        public static int Berserk_Dance //Set to 1 to use dance attack when berzerk
        {
            get { return _Berserk_Dance; }
            set { _Berserk_Dance = value; }
        }
        static int _Berserk_IgnoreMinSP = 0;
        public static int Berserk_IgnoreMinSP //Set to 1 to use dance attack when berzerk
        {
            get { return _Berserk_IgnoreMinSP; }
            set { _Berserk_IgnoreMinSP = value; }
        }

        static int _Berserk_ComboAlways = 0;
        public static int Berserk_ComboAlways
        {
            get { return _Berserk_ComboAlways; }
            set { _Berserk_ComboAlways = value; }
        }
        #endregion

        #region PVP Options
        ///////////////
        //PVP Options//
        ///////////////
        static int _PVPmode = 0;
        public static int PVPmode // Set to 1 to enable PVP behavior
        {
            get { return _PVPmode; }
            set { _PVPmode = value; }
        }
        #endregion

        #region SetDefaults Method
        /// <summary>
        /// Reset all config values to their initial static defaults
        /// </summary>
        public static void SetDefaults()
        {
            // Basic Options
            AggroHP = 60;
            AggroSP = 0;
            KiteMonsters = 0;

            SuperPassive = 0;
            UseAttackSkill = 1;
            AssumeHomun = 1;
            DoNotChase = 0;
            UseDanceAttack = 0;
            UseAvoid = 0;
            TankMonsterLimit = 4;
            RescueOwnerLowHP = 0;
            StationaryAggroDist = 0;
            MobileAggroDist = 0;
            OldHomunType = 0;
            OpportunisticTargeting = 1;
            AttackLastFullSP = 0;
            DanceMinSP = 0;
            AttackTimeLimit = 0;
            LagReduction = 0;
            DoNotAttackMoving = 0;
            LiveMobID = 0;

            // AutoSkill Options
            AttackSkillReserveSP = 0;
            UseSkillOnly = 0;
            AutoSkillDelay = 0;
            AutoMobMode = 0;
            AoEReserveSP = 0;
            AoEFixedLevel = 0;
            AutoMobCount = 0;
            illusionOfClawsLevel = 0;
            illusionOfBreathLevel = 0;
            illusionOfCrusherLevel = 0;
            illusionOfLightLevel = 0;
            chaoticHealLevel = 0;
            bodyDoubleLevel = 0;
            warmDefLevel = 0;
            onlyAOE = 0;
            AoEMaximizeTargets = 0;
            AutoComboMode = 0;
            AutoComboSpheres = 0;

            // Walk and Follow Options
            FollowStayBack = 0;
            StationaryMoveBounds = 0;
            MobileMoveBounds = 0;
            DoNotUseRest = 0;
            RestXOff = 0;
            RestYOff = 0;
            MoveSticky = 0;
            MoveStickyFight = 0;
            UseIdleWalk = 0;
            IdleWalkSP = 0;
            IdleWalkDistance = 0;
            RelativeRoute = 0;
            UseCastleRoute = 0;

            // Autobuff Options
            HealSelfHP = 0;
            HealOwnerHP = 0;
            UseAutoHeal = 0;
            UseChaoticHeal = 0;
            ChaoticHealOwnerHP = 0;
            ChaoticHealKimiHP = 0;
            UseBodyDouble = 0;
            BodyDoubleOwnerHP = 0;
            UseWarmDef = 0;
            WarmDefCooldown = 0;
            UseMasterSwap = 0;
            MasterSwapOwnerHP = 0;
            MasterSwapCooldown = 0;

            // Skill Combo Options
            ComboEnabled = 0;
            BlueprintComboEnabled = 0;
            ComboRunDuringChase = 0;
            ComboRunDuringAttack = 0;
            ComboRunDuringIdle = 0;
            ComboResetOnTargetChange = 0;
            ComboAutoAttackDelay = 200;
            ComboSlot1_SkillID = 0;
            ComboSlot1_ComboCount = 1;
            ComboSlot2_SkillID = 0;
            ComboSlot2_ComboCount = 1;
            ComboSlot3_SkillID = 0;
            ComboSlot3_ComboCount = 1;
            ComboSlot4_SkillID = 0;
            ComboSlot4_ComboCount = 1;
            ComboSlot5_SkillID = 0;
            ComboSlot5_ComboCount = 0;
            ComboSlot6_SkillID = 0;
            ComboSlot6_ComboCount = 0;
            ComboSlot7_SkillID = 0;
            ComboSlot7_ComboCount = 0;
            ComboSlot8_SkillID = 0;
            ComboSlot8_ComboCount = 0;

            // Kiting Options
            KiteBounds = 0;
            KiteStep = 0;
            KiteParanoidStep = 0;
            KiteThreshold = 0;
            KiteParanoidThreshold = 0;
            KiteParanoid = 0;
            ForceKite = 0;

            // Friending Options
            FleeHP = 0;
            StandbyFriending = 0;
            MirAIFriending = 0;
            FriendAssistDamageHPThresholdOwner = 50;

            // Standby Options
            DefendStandby = 0;
            StickyStandby = 0;

            // Berserk Options
            UseBerserkMobbed = 0;
            UseBerserkSkill = 0;
            UseBerserkAttack = 0;
            Berserk_SkillAlways = 0;
            Berserk_Dance = 0;
            Berserk_IgnoreMinSP = 0;
            Berserk_ComboAlways = 0;

            // PVP Options
            PVPmode = 0;
        }
        #endregion
    }
}