using VYaml.Annotations;

namespace UFO_50_Mod_Converter.Models
{
    [YamlObject]
    public partial class SpriteData
    {
        [YamlMember("frames")]
        public int? yml_frame { get; set; }

        [YamlMember("x")]
        public int? yml_x { get; set; }

        [YamlMember("y")]
        public int? yml_y { get; set; }

        [YamlMember("transparent")]
        public bool? yml_transparent { get; set; }

        [YamlMember("smooth")]
        public bool? yml_smooth { get; set; }

        [YamlMember("preload")]
        public bool? yml_preload { get; set; }

        [YamlMember("speed_type")]
        public uint? yml_speedtype { get; set; }

        [YamlMember("frame_speed")]
        public float? yml_framespeed { get; set; }

        [YamlMember("bounding_box_type")]
        public uint? yml_boundingboxtype { get; set; }

        [YamlMember("bbox_left")]
        public int? yml_bboxleft { get; set; }

        [YamlMember("bbox_right")]
        public int? yml_bboxright { get; set; }

        [YamlMember("bbox_bottom")]
        public int? yml_bboxbottom { get; set; }

        [YamlMember("bbox_top")]
        public int? yml_bboxtop { get; set; }

        [YamlMember("sepmasks")]
        public uint? yml_sepmask { get; set; }
    }
}
