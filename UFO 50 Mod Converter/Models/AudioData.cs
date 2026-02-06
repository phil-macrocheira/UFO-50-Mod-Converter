using VYaml.Annotations;

namespace UFO_50_Mod_Converter.Models
{
    [YamlObject]
    public partial class AudioData
    {
        [YamlMember("type")]
        public string? yml_type { get; set; }
        [YamlMember("embedded")]
        public bool? yml_embedded { get; set; }
        [YamlMember("compressed")]
        public bool? yml_compressed { get; set; }
        [YamlMember("effects")]
        public uint? yml_effects { get; set; }
        [YamlMember("volume")]
        public float? yml_volume { get; set; }
        [YamlMember("pitch")]
        public float? yml_pitch { get; set; }
        [YamlMember("audiogroup_index")]
        public int? yml_audiogroup_index { get; set; }
        [YamlMember("audiofile_id")]
        public int? yml_audiofile_id { get; set; }
        [YamlMember("preload")]
        public bool? yml_preload { get; set; }
    }
}
