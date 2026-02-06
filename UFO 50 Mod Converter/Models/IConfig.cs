using Config.Net;

namespace UFO_50_Mod_Converter.Models
{
    public static class Settings
    {
        public static IConfig Config { get; set; }
    }

    public interface IConfig
    {
        [Option(DefaultValue = true)]
        bool ExportCode { get; }

        [Option(DefaultValue = true)]
        bool ExportTextures { get; }

        [Option(DefaultValue = true)]
        bool ExportBackgrounds { get; }

        [Option(DefaultValue = true)]
        bool ExportObjects { get; }

        [Option(DefaultValue = true)]
        bool ExportRooms { get; }

        [Option(DefaultValue = true)]
        bool ExportAudio { get; }

        [Option(DefaultValue = false)]
        bool ReuseVanillaExport { get; }
        [Option(DefaultValue = false)]
        bool AutoDeleteVanillaExport { get; }
        [Option(DefaultValue = false)]
        bool AutoDeleteModdedExport { get; }

        [Option(DefaultValue = "sTest,s39_RoomTest")]
        string TexturesToIgnore { get; }
    }
}