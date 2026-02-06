using UFO_50_Mod_Converter.Models;
using UFO_50_Mod_Converter.ExportScripts;
using UndertaleModLib;
using Serilog;

namespace UFO_50_Mod_Converter
{
    public static class Export
    {
        public static UndertaleData Data { get; private set; }

        public static void Start()
        {
            ValidateInputFiles();

            bool shouldExportVanilla = !Settings.Config.ReuseVanillaExport ||
                                       !Directory.Exists(Constants.ExportVanillaPath);

            if (shouldExportVanilla)
                ExportData(Constants.VanillaWinPath, Constants.ExportVanillaPath);

            ExportData(Constants.ModdedWinPath, Constants.ExportModdedPath);
        }

        private static void ValidateInputFiles()
        {
            if (!Settings.Config.ReuseVanillaExport) {
                while (!File.Exists(Constants.VanillaWinPath)) {
                    Log.Error($"Missing required file: {Constants.VanillaWinPath}");
                    Log.Information("Press any key to retry...");
                    Console.ReadKey();
                }
            }

            while (!File.Exists(Constants.ModdedWinPath)) {
                Log.Error($"Missing required file: {Constants.ModdedWinPath}");
                Log.Information("Press any key to retry...");
                Console.ReadKey();
            }
        }

        private static void ExportData(string dataPath, string outputRoot)
        {
            Log.Information($"Exporting {Path.GetFileName(dataPath)}");

            // Ensure output directory exists
            Directory.CreateDirectory(outputRoot);

            using (var stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read)) {
                Data = UndertaleIO.Read(stream);
            }

            bool anyExported = false;

            if (Settings.Config.ExportCode) {
                ExportCode.Export(outputRoot, Constants.ExportedCodeOutputFolder);
                anyExported = true;
            }

            if (Settings.Config.ExportTextures || Settings.Config.ExportBackgrounds) {
                ExportTextures.Export(
                    outputRoot,
                    Constants.ExportedTexturesOutputFolder,
                    Constants.ExportedTexturesConfigOutputFolder,
                    Constants.ExportedBackgroundsOutputFolder,
                    Constants.ExportedBackgroundsConfigOutputFolder);
                anyExported = true;
            }

            if (Settings.Config.ExportObjects) {
                ExportObjects.Export(outputRoot, Constants.ExportedObjectDataOutputFolder);
                anyExported = true;
            }

            if (Settings.Config.ExportRooms) {
                ExportRooms.Export(outputRoot, Constants.ExportedRoomDataOutputFolder);
                anyExported = true;
            }

            if (Settings.Config.ExportAudio) {
                ExportAudio.Export(
                    outputRoot,
                    Constants.ExportedAudioOutputFolder,
                    Constants.ExportedAudioConfigOutputFolder);
                anyExported = true;
            }

            if (!anyExported) {
                Log.Warning("No export options are enabled in GMLoader.ini - nothing was exported");
            }

            Log.Information($"Successfully exported {dataPath}");
        }
    }
}