namespace UFO_50_Mod_Converter.Models
{
    internal static class Constants
    {
        // Config files
        public static readonly string IniPath = "GMLoader.ini";
        public static readonly string LogPath = "log.txt";

        // Input files
        public static readonly string VanillaWinPath = "vanilla.win";
        public static readonly string ModdedWinPath = "data.win";

        // Export folders (intermediate)
        public static readonly string ExportVanillaPath = "Export Vanilla";
        public static readonly string ExportModdedPath = "Export Modded";
        public static readonly string ConvertedOutputPath = "Converted Output";

        // Export subfolder names (used during export phase)
        public static readonly string ExportedCodeOutputFolder = "code";
        public static readonly string ExportedTexturesOutputFolder = "textures";
        public static readonly string ExportedTexturesConfigOutputFolder = Path.Combine("config", "textures_properties");
        public static readonly string ExportedBackgroundsOutputFolder = Path.Combine("textures", "backgrounds");
        public static readonly string ExportedBackgroundsConfigOutputFolder = Path.Combine("config", "textures_properties", "backgrounds_properties");
        public static readonly string ExportedObjectDataOutputFolder = "objects";
        public static readonly string ExportedRoomDataOutputFolder = "room";
        public static readonly string ExportedAudioOutputFolder = "audio";
        public static readonly string ExportedAudioConfigOutputFolder = Path.Combine("config", "audio_properties");

        // Compared YAML names
        public static readonly string MyModdedTexturesConfig = "MyModdedTexturesConfig.yaml";
        public static readonly string MyModdedBackgroundsConfig = "MyModdedBackgroundsConfig.yaml";
        public static readonly string MyModdedAudioConfig = "MyModdedAudioConfig.yaml";
        public static readonly string MyModdedPrefix = "MyModded";
    }
}