using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace AzzyAIConfig
{
    /// <summary>
    /// Installs the Lua runtime shipped with the GUI beside the executable.
    /// Core files follow the GUI version; user configuration files are created
    /// on first use and then left under the user's control.
    /// </summary>
    public static class RuntimeFiles
    {
        private const string ResourcePrefix = "AzzyAIConfig.USER_AI.";
        private const string LegacyAutoBuffsComment =
            "-- IMPORTANT: Return 1 to SKIP buffs, return 0/nil to ALLOW buffs";

        private static readonly string[] CoreFiles =
        {
            "AI.lua",
            "AI_main.lua",
            "AzzyUtil.lua",
            "Const_.lua",
            "Stubs.lua",
            "H_SkillList.lua",
            "KimiNavigation.lua",
            "NavigationData.txt",
            "NavigationMaps.txt"
        };

        private static readonly string[] UserFiles =
        {
            "H_Config.lua",
            "H_Extra.lua",
            "H_Tactics.lua",
            "H_PVP_Tact.lua",
            "H_Avoid.lua",
            "A_Friends.lua",
            "Defaults.lua",
            "Mob_ID.lua",
            "twRO.lua"
        };

        public static string UserAiDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static void Install()
        {
            string userAiDirectory = UserAiDirectory;
            Directory.CreateDirectory(userAiDirectory);
            Directory.CreateDirectory(Path.Combine(userAiDirectory, "data"));
            Directory.CreateDirectory(Path.Combine(userAiDirectory, "ErrorLog"));

            for (int i = 0; i < CoreFiles.Length; i++)
                InstallCoreFile(userAiDirectory, CoreFiles[i]);

            for (int i = 0; i < UserFiles.Length; i++)
                InstallMissingUserFile(userAiDirectory, UserFiles[i]);

            MigrateLegacyExtra(Path.Combine(userAiDirectory, "H_Extra.lua"));
        }

        private static void InstallCoreFile(string userAiDirectory, string fileName)
        {
            byte[] source = ReadResource(fileName);
            string target = Path.Combine(userAiDirectory, fileName);

            if (File.Exists(target) && ByteArraysEqual(File.ReadAllBytes(target), source))
                return;

            if (File.Exists(target))
                BackupOnce(target);

            File.WriteAllBytes(target, source);
        }

        private static void InstallMissingUserFile(string userAiDirectory, string fileName)
        {
            string target = Path.Combine(userAiDirectory, fileName);
            if (!File.Exists(target))
                File.WriteAllBytes(target, ReadResource(fileName));
        }

        private static byte[] ReadResource(string fileName)
        {
            string resourceName = fileName == "H_SkillList.lua"
                ? SkillList.CanonicalResourceName
                : ResourcePrefix + fileName;

            Assembly assembly = typeof(RuntimeFiles).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        "The embedded USER_AI resource is missing: " + resourceName);

                using (MemoryStream buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    return buffer.ToArray();
                }
            }
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; i++)
                if (left[i] != right[i])
                    return false;

            return true;
        }

        private static void BackupOnce(string fileName)
        {
            string backupName = fileName + ".bak";
            if (!File.Exists(backupName))
                File.Copy(fileName, backupName);
        }

        private static void MigrateLegacyExtra(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] originalBytes = File.ReadAllBytes(fileName);
            byte[] migrated = ReplaceLegacyAutoBuffs(originalBytes);

            if (migrated == null)
                return;

            BackupOnce(fileName);
            File.WriteAllBytes(fileName, migrated);
        }

        private static byte[] ReplaceLegacyAutoBuffs(byte[] source)
        {
            byte[] markerBytes = Encoding.ASCII.GetBytes(LegacyAutoBuffsComment);
            int marker = IndexOf(source, markerBytes, 0);
            if (marker < 0)
                return null;

            byte[] functionBytes = Encoding.ASCII.GetBytes("function OnAutoBuffs");
            byte[] endMarkerBytes = Encoding.ASCII.GetBytes("-- OnFailUnknownMode:");
            byte[] headingBytes = Encoding.ASCII.GetBytes("-- OnAutoBuffs");
            int functionStart = IndexOf(source, functionBytes, marker);
            int endMarker = functionStart < 0
                ? -1
                : IndexOf(source, endMarkerBytes, functionStart);
            if (functionStart < 0 || endMarker < 0)
                return null;

            int replacementStart = LineStart(source, marker);
            int heading = LastIndexOf(source, headingBytes, marker);
            if (heading >= 0 && marker - heading <= 1024)
                replacementStart = LineStart(source, heading);

            byte[] newline = IndexOf(source, new byte[] { 0x0D, 0x0A }, 0) >= 0
                ? new byte[] { 0x0D, 0x0A }
                : new byte[] { 0x0A };
            byte[] replacement = Encoding.ASCII.GetBytes(
                "-- OnAutoBuffs runs after standard buffs. Return 1 when no action consumed the tick." +
                (newline[0] == 0x0D ? "\r\n" : "\n") +
                "function OnAutoBuffs(buffmode)" +
                (newline[0] == 0x0D ? "\r\n" : "\n") +
                "\t-- No custom buff was cast. Return 1 so AI state processing can continue." +
                (newline[0] == 0x0D ? "\r\n" : "\n") +
                "\treturn 1" +
                (newline[0] == 0x0D ? "\r\n" : "\n") +
                "end" +
                (newline[0] == 0x0D ? "\r\n" : "\n"));

            byte[] result = new byte[replacementStart + replacement.Length + source.Length - endMarker];
            Buffer.BlockCopy(source, 0, result, 0, replacementStart);
            Buffer.BlockCopy(replacement, 0, result, replacementStart, replacement.Length);
            Buffer.BlockCopy(source, endMarker, result, replacementStart + replacement.Length, source.Length - endMarker);
            return result;
        }

        private static int LineStart(byte[] source, int index)
        {
            int newline = LastIndexOf(source, new byte[] { 0x0A }, index);
            return newline < 0 ? 0 : newline + 1;
        }

        private static int IndexOf(byte[] source, byte[] value, int start)
        {
            for (int i = Math.Max(0, start); i <= source.Length - value.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < value.Length; j++)
                {
                    if (source[i + j] != value[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            return -1;
        }

        private static int LastIndexOf(byte[] source, byte[] value, int start)
        {
            int last = Math.Min(start, source.Length - value.Length);
            for (int i = last; i >= 0; i--)
            {
                bool match = true;
                for (int j = 0; j < value.Length; j++)
                {
                    if (source[i + j] != value[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }
            return -1;
        }
    }
}
