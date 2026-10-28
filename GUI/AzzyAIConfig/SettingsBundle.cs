using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AzzyAIConfig
{
    internal static class SettingsBundle
    {
        private static readonly string[] AllowedFiles =
        {
            "H_Config.lua",
            "H_Tactics.lua",
            "H_PVP_Tact.lua",
            "H_Extra.lua",
            "H_Avoid.lua",
            "A_Friends.lua",
            "Defaults.lua",
            "Mob_ID.lua",
            "twRO.lua",
            "combo_tactics.cbp",
            "ComboTactics.lua"
        };

        private static readonly HashSet<string> AllowedFileNames =
            new HashSet<string>(AllowedFiles, StringComparer.Ordinal);

        private static readonly HashSet<string> RequiredFiles =
            new HashSet<string>(new[] { "H_Config.lua", "H_Tactics.lua" }, StringComparer.Ordinal);

        internal static void Export(string path, string directory)
        {
            string archivePath = GetFullPath(path, "path");
            string sourceDirectory = GetFullPath(directory, "directory");
            Dictionary<string, byte[]> files = ReadSourceFiles(sourceDirectory);
            string archiveDirectory = Path.GetDirectoryName(archivePath);

            if (String.IsNullOrEmpty(archiveDirectory) || !Directory.Exists(archiveDirectory))
                throw new DirectoryNotFoundException("The settings bundle directory does not exist: " + archiveDirectory);

            string temporaryPath = archivePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < AllowedFiles.Length; i++)
                    {
                        byte[] contents;
                        if (!files.TryGetValue(AllowedFiles[i], out contents))
                            continue;

                        ZipArchiveEntry entry = archive.CreateEntry(AllowedFiles[i]);
                        using (Stream entryStream = entry.Open())
                            entryStream.Write(contents, 0, contents.Length);
                    }
                }

                if (File.Exists(archivePath))
                    File.Replace(temporaryPath, archivePath, null);
                else
                    File.Move(temporaryPath, archivePath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        internal static void Import(string path, string directory)
        {
            string archivePath = GetFullPath(path, "path");
            string targetDirectory = GetFullPath(directory, "directory");
            List<BundleFile> files = ReadArchiveFiles(archivePath);
            Dictionary<string, FileSnapshot> snapshots = new Dictionary<string, FileSnapshot>(StringComparer.OrdinalIgnoreCase);
            List<string> writtenFiles = new List<string>();

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            for (int i = 0; i < files.Count; i++)
            {
                string targetPath = Path.Combine(targetDirectory, files[i].Name);
                if (Directory.Exists(targetPath))
                    throw new IOException("A directory already exists for the settings file: " + targetPath);

                bool exists = File.Exists(targetPath);
                snapshots.Add(targetPath, new FileSnapshot(exists, exists ? File.ReadAllBytes(targetPath) : null));
            }

            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string targetPath = Path.Combine(targetDirectory, files[i].Name);
                    writtenFiles.Add(targetPath);
                    File.WriteAllBytes(targetPath, files[i].Contents);
                }
            }
            catch (Exception importError)
            {
                try
                {
                    Rollback(writtenFiles, snapshots);
                }
                catch (Exception rollbackError)
                {
                    throw new IOException("Settings import failed and rollback also failed.",
                        new AggregateException(importError, rollbackError));
                }

                throw;
            }
        }

        private static Dictionary<string, byte[]> ReadSourceFiles(string directory)
        {
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>(StringComparer.Ordinal);

            for (int i = 0; i < AllowedFiles.Length; i++)
            {
                string fileName = AllowedFiles[i];
                string sourcePath = Path.Combine(directory, fileName);
                if (!File.Exists(sourcePath))
                {
                    if (RequiredFiles.Contains(fileName))
                        throw new FileNotFoundException("Required settings file is missing: " + sourcePath, sourcePath);

                    continue;
                }

                files.Add(fileName, File.ReadAllBytes(sourcePath));
            }

            return files;
        }

        private static List<BundleFile> ReadArchiveFiles(string archivePath)
        {
            List<BundleFile> files = new List<BundleFile>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            using (FileStream stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fileName = ValidateEntryName(entry.FullName);
                    if (!seen.Add(fileName))
                        throw new InvalidDataException("The settings bundle contains a duplicate entry: " + fileName);

                    using (Stream entryStream = entry.Open())
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        entryStream.CopyTo(buffer);
                        files.Add(new BundleFile(fileName, buffer.ToArray()));
                    }
                }
            }

            for (int i = 0; i < AllowedFiles.Length; i++)
            {
                if (RequiredFiles.Contains(AllowedFiles[i]) && !seen.Contains(AllowedFiles[i]))
                    throw new InvalidDataException("The settings bundle is missing required entry: " + AllowedFiles[i]);
            }

            return files;
        }

        private static string ValidateEntryName(string name)
        {
            if (String.IsNullOrEmpty(name) || name.IndexOf('/') >= 0 || name.IndexOf('\\') >= 0 ||
                name == "." || name == "..")
                throw new InvalidDataException("The settings bundle contains an unsafe entry name: " + name);

            if (!AllowedFileNames.Contains(name))
                throw new InvalidDataException("The settings bundle contains an unknown entry: " + name);

            return name;
        }

        private static void Rollback(IList<string> writtenFiles, IDictionary<string, FileSnapshot> snapshots)
        {
            Exception firstError = null;

            for (int i = writtenFiles.Count - 1; i >= 0; i--)
            {
                string targetPath = writtenFiles[i];
                try
                {
                    FileSnapshot snapshot = snapshots[targetPath];
                    if (snapshot.Existed)
                        File.WriteAllBytes(targetPath, snapshot.Contents);
                    else if (File.Exists(targetPath))
                        File.Delete(targetPath);
                }
                catch (Exception error)
                {
                    if (firstError == null)
                        firstError = error;
                }
            }

            if (firstError != null)
                throw new IOException("One or more settings files could not be rolled back.", firstError);
        }

        private static string GetFullPath(string path, string argumentName)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("A path is required.", argumentName);

            return Path.GetFullPath(path);
        }

        private sealed class BundleFile
        {
            internal readonly string Name;
            internal readonly byte[] Contents;

            internal BundleFile(string name, byte[] contents)
            {
                Name = name;
                Contents = contents;
            }
        }

        private sealed class FileSnapshot
        {
            internal readonly bool Existed;
            internal readonly byte[] Contents;

            internal FileSnapshot(bool existed, byte[] contents)
            {
                Existed = existed;
                Contents = contents;
            }
        }
    }
}
