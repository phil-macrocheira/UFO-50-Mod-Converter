using Serilog;
using Standart.Hash.xxHash;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using UFO_50_Mod_Converter.Models;

namespace UFO_50_Mod_Converter
{
    public static class Compare
    {
        public static void Start()
        {
            if (!Directory.Exists(Constants.ExportVanillaPath) || !Directory.Exists(Constants.ExportModdedPath)) {
                Log.Error("Export directories are missing. Ensure export settings are enabled in GMLoader.ini");
                return;
            }

            CompareAndCopyFiles();
            CopyAndMergeConfigFiles();
            CleanUpFiles();
            Log.Information($"Done converting, files have been copied into {Constants.ConvertedOutputPath}");
        }

        private static void CompareAndCopyFiles()
        {
            if (!Directory.Exists(Constants.ExportVanillaPath)) {
                Log.Error($"Vanilla export directory not found: {Constants.ExportVanillaPath}");
                return;
            }
            if (!Directory.Exists(Constants.ExportModdedPath)) {
                Log.Error($"Modded export directory not found: {Constants.ExportModdedPath}");
                return;
            }

            var vanillaFiles = Directory.GetFiles(Constants.ExportVanillaPath, "*.*", SearchOption.AllDirectories);
            var moddedFiles = Directory.GetFiles(Constants.ExportModdedPath, "*.*", SearchOption.AllDirectories);
            var regex = new Regex(@"^(.*?)(?=_f[0-9])", RegexOptions.Compiled);
            var vanillaFileNames = new ConcurrentDictionary<string, string>();

            Parallel.ForEach(vanillaFiles, vanillaFile => {
                var match = regex.Match(Path.GetFileName(vanillaFile));
                if (match.Success) {
                    vanillaFileNames[match.Groups[1].Value] = vanillaFile;
                }
                else if (vanillaFile.Contains("backgrounds")) {
                    vanillaFileNames[Path.GetFileName(vanillaFile)] = vanillaFile;
                }
                else {
                    vanillaFileNames[Path.GetFileName(vanillaFile)] = vanillaFile;
                }
            });

            Parallel.ForEach(moddedFiles, moddedFile => {
                string fileName = Path.GetFileName(moddedFile);
                var match = regex.Match(fileName);
                if (match.Success) {
                    fileName = match.Groups[1].Value;
                }

                string relativePath = Path.GetRelativePath(Constants.ExportModdedPath, moddedFile);
                string outputFilePath = Path.Combine(Constants.ConvertedOutputPath, relativePath);

                if (fileName.Equals("data.json", StringComparison.OrdinalIgnoreCase)) {
                    Log.Information($"Copying {moddedFile}");
                    EnsureDirectoryExists(Path.GetDirectoryName(outputFilePath));
                    File.Copy(moddedFile, outputFilePath, true);
                    return;
                }

                if (vanillaFileNames.TryGetValue(fileName, out string vanillaFile)) {
                    ulong vanillaHash = ComputeFileHash(vanillaFile);
                    ulong modHash = ComputeFileHash(moddedFile);

                    if (vanillaHash != modHash) {
                        Log.Information($"File modified: {Path.GetFileName(moddedFile)}");
                        EnsureDirectoryExists(Path.GetDirectoryName(outputFilePath));
                        File.Copy(moddedFile, outputFilePath, true);
                    }
                }
                else {
                    Log.Information($"New file: {Path.GetFileName(moddedFile)}");
                    EnsureDirectoryExists(Path.GetDirectoryName(outputFilePath));
                    File.Copy(moddedFile, outputFilePath, true);
                }
            });
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static ulong ComputeFileHash(string filePath)
        {
            try {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                if (fileBytes.Length == 0) {
                    Log.Warning($"Empty file: {Path.GetFileName(filePath)}");
                    return 0;
                }
                return xxHash3.ComputeHash(fileBytes, fileBytes.Length, 0);
            }
            catch (Exception ex) {
                Log.Error($"Failed to hash {filePath}: {ex.Message}");
                return 0;
            }
        }

        private static void CopyAndMergeConfigFiles()
        {
            if (Settings.Config.ExportTextures) {
                CopyMatchingConfigFiles(
                    Constants.ExportedTexturesOutputFolder,
                    Constants.ExportedTexturesConfigOutputFolder,
                    "texture");
                MergeConfigFiles(
                    Path.Combine(Constants.ConvertedOutputPath, Constants.ExportedTexturesConfigOutputFolder), Constants.MyModdedTexturesConfig);
            }

            if (Settings.Config.ExportBackgrounds) {
                CopyMatchingConfigFiles(
                    Constants.ExportedBackgroundsOutputFolder,
                    Constants.ExportedBackgroundsConfigOutputFolder,
                    "background");
                MergeConfigFiles(
                    Path.Combine(Constants.ConvertedOutputPath, Constants.ExportedBackgroundsConfigOutputFolder), Constants.MyModdedBackgroundsConfig);
            }

            if (Settings.Config.ExportAudio) {
                CopyMatchingConfigFiles(
                    Constants.ExportedAudioOutputFolder,
                    Constants.ExportedAudioConfigOutputFolder,
                    "audio");
                MergeConfigFiles(
                    Path.Combine(Constants.ConvertedOutputPath, Constants.ExportedAudioConfigOutputFolder), Constants.MyModdedAudioConfig);
            }
        }

        private static void CopyMatchingConfigFiles(string assetFolder, string configFolder, string assetType)
        {
            Log.Information($"Copying {assetType} configuration files");

            string outputAssetFolder = Path.Combine(Constants.ConvertedOutputPath, assetFolder);
            string outputConfigFolder = Path.Combine(Constants.ConvertedOutputPath, configFolder);
            string moddedConfigFolder = Path.Combine(Constants.ExportModdedPath, configFolder);

            if (!Directory.Exists(outputAssetFolder) || !Directory.Exists(moddedConfigFolder))
                return;

            var assetNames = Directory.GetFiles(outputAssetFolder, "*.*", SearchOption.AllDirectories)
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToHashSet();

            EnsureDirectoryExists(outputConfigFolder);

            foreach (var yamlFile in Directory.GetFiles(moddedConfigFolder, "*.yaml", SearchOption.TopDirectoryOnly)) {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(yamlFile);
                if (assetNames.Contains(nameWithoutExt)) {
                    try {
                        string destPath = Path.Combine(outputConfigFolder, Path.GetFileName(yamlFile));
                        File.Copy(yamlFile, destPath, overwrite: true);
                        Log.Debug($"Copied config: {Path.GetFileName(yamlFile)}");
                    }
                    catch (Exception ex) {
                        Log.Error($"Failed to copy {yamlFile}: {ex.Message}");
                    }
                }
            }
        }

        private static void MergeConfigFiles(string configFolder, string outputFileName)
        {
            if (!Directory.Exists(configFolder))
                return;

            var configFiles = Directory.GetFiles(configFolder, "*.yaml", SearchOption.TopDirectoryOnly)
                .Where(f => !Path.GetFileName(f).StartsWith(Constants.MyModdedPrefix))
                .ToArray();

            if (configFiles.Length == 0) {
                Log.Information($"No config files to merge in {configFolder}");
                return;
            }

            Log.Information($"Merging {configFiles.Length} config files into {outputFileName}");

            string outputPath = Path.Combine(configFolder, outputFileName);
            using (var writer = new StreamWriter(outputPath)) {
                foreach (string file in configFiles) {
                    writer.WriteLine(File.ReadAllText(file));
                }
            }

            foreach (string file in configFiles) {
                File.Delete(file);
            }
        }

        private static void CleanUpFiles()
        {
            if (!Directory.Exists(Constants.ConvertedOutputPath)) {
                Log.Warning("No converted output to process");
                return;
            }

            if (Directory.Exists(Constants.ExportModdedPath) && Settings.Config.AutoDeleteModdedExport)
                Directory.Delete(Constants.ExportModdedPath, true);

            if (Directory.Exists(Constants.ExportVanillaPath) && Settings.Config.AutoDeleteVanillaExport)
                Directory.Delete(Constants.ExportVanillaPath, true);

            DeleteEmptyFolders(Constants.ConvertedOutputPath);
        }
        private static void DeleteEmptyFolders(string path)
        {
            if (!Directory.Exists(path))
                return;

            foreach (var dir in Directory.GetDirectories(path)) {
                DeleteEmptyFolders(dir);
            }

            if (!Directory.EnumerateFileSystemEntries(path).Any() && path != Path.GetPathRoot(path)) {
                Directory.Delete(path);
                Log.Debug($"Deleted empty folder: {path}");
            }
        }
    }
}