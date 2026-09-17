// SkillList.cs
//
// H_SkillList.lua is maintained by the runtime in USER_AI. The GUI keeps a
// copy beside its executable for the existing Apply workflow, but it must copy
// the canonical embedded source instead of recreating a stale skill table.

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace AzzyAIConfig
{
    public static class SkillList
    {
        public const string CanonicalResourceName = "AzzyAIConfig.USER_AI.H_SkillList.lua";

        public static void Save(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            Assembly assembly = typeof(SkillList).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(CanonicalResourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        "The canonical USER_AI/H_SkillList.lua resource is not embedded. " +
                        "Check AzzyAIConfig.csproj.");

                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    string source = reader.ReadToEnd();
                    File.WriteAllText(fileName, source, new UTF8Encoding(false));
                }
            }
        }
    }
}
