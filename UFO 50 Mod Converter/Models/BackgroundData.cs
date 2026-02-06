using VYaml.Annotations;

namespace UFO_50_Mod_Converter.Models
{
    [YamlObject]
    public partial class BackgroundData
    {
        [YamlMember("tile_count")]
        public uint? yml_tile_count { get; set; }
        [YamlMember("tile_width")]
        public uint? yml_tile_width { get; set; }
        [YamlMember("tile_height")]
        public uint? yml_tile_height { get; set; }
        [YamlMember("border_x")]
        public uint? yml_border_x { get; set; }
        [YamlMember("border_y")]
        public uint? yml_border_y { get; set; }
        [YamlMember("tile_column")]
        public uint? yml_tile_column { get; set; }
        [YamlMember("item_per_tile")]
        public uint? yml_item_per_tile { get; set; }
        [YamlMember("transparent")]
        public bool? yml_transparent { get; set; }
        [YamlMember("smooth")]
        public bool? yml_smooth { get; set; }
        [YamlMember("preload")]
        public bool? yml_preload { get; set; }
        [YamlMember("frametime")]
        public long? yml_frametime { get; set; }
    }
}
