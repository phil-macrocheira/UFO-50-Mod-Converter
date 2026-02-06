using ImageMagick;
using Serilog;
using UFO_50_Mod_Converter.Models;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using VYaml.Serialization;

namespace UFO_50_Mod_Converter.ExportScripts
{
    public static class ExportTextures
    {
        public const uint MAX_WIDTH = 65536;
        public const string ext = ".png";

        private static TextureWorker _worker;
        private static bool _padded;
        private static string _texFolder;
        private static string _texConfigFolder;
        private static string _bgFolder;
        private static string _bgConfigFolder;
        private static string[] _texturesToIgnore;

        public static void Export(string outputRoot, string exportedTexturesOutputFolder, string exportedTexturesConfigOutputFolder, string exportedBackgroundsOutputFolder, string exportedBackgroundsConfigOutputFolder)
        {
            var data = UFO_50_Mod_Converter.Export.Data;

            _padded = true;
            _texFolder = Path.Combine(outputRoot, exportedTexturesOutputFolder);
            _texConfigFolder = Path.Combine(outputRoot, exportedTexturesConfigOutputFolder);
            _bgFolder = Path.Combine(outputRoot, exportedBackgroundsOutputFolder);
            _bgConfigFolder = Path.Combine(outputRoot, exportedBackgroundsConfigOutputFolder);

            _texturesToIgnore = Settings.Config.TexturesToIgnore?
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            Directory.CreateDirectory(_texFolder);
            Directory.CreateDirectory(_bgFolder);
            Directory.CreateDirectory(_texConfigFolder);
            Directory.CreateDirectory(_bgConfigFolder);

            int coreCount = Math.Max(1, Environment.ProcessorCount - 1);
            var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount };

            Log.Information($"Using {coreCount} cores to dump textures");

            using (_worker = new TextureWorker()) {
                if (Settings.Config.ExportBackgrounds)
                    Parallel.ForEach(data.Backgrounds, options, DumpBackground);

                if (Settings.Config.ExportTextures)
                    Parallel.ForEach(data.Sprites, options, DumpSprite);
            }

            Log.Information($"All textures have been exported to {Path.GetFullPath(_texFolder)}");
        }

        private static void DumpSprite(UndertaleSprite sprite)
        {
            string spriteName = sprite.Name.Content;

            if (string.IsNullOrEmpty(spriteName)) {
                Log.Error("Skipped sprite with empty name");
                return;
            }

            foreach (string ignore in _texturesToIgnore) {
                if (spriteName.Contains(ignore)) {
                    return;
                }
            }

            Log.Information($"Exporting {spriteName}");

            var images = new List<IMagickImage<byte>>();
            uint totalWidth = 0;
            uint maxHeight = 0;
            int spriteFrame = 0;

            foreach (var texture in sprite.Textures) {
                if (texture?.Texture != null) {
                    var image = _worker.GetTextureFor(texture.Texture, spriteName, _padded);
                    images.Add(image);
                    totalWidth += image.Width;
                    maxHeight = Math.Max(maxHeight, image.Height);
                    spriteFrame++;
                }
            }

            if (totalWidth == 0 || maxHeight == 0) {
                Log.Error($"Error: {spriteName} has invalid width or height");
                foreach (var img in images) img.Dispose();
                return;
            }

            if (totalWidth >= MAX_WIDTH) {
                Log.Warning($"Skipped {spriteName} because width exceeds {MAX_WIDTH}");
                foreach (var img in images) img.Dispose();
                return;
            }

            var config = new SpriteData {
                yml_frame = spriteFrame,
                yml_x = sprite.OriginX,
                yml_y = sprite.OriginY,
                yml_transparent = sprite.Transparent,
                yml_smooth = sprite.Smooth,
                yml_preload = sprite.Preload,
                yml_boundingboxtype = sprite.BBoxMode,
                yml_bboxleft = sprite.MarginLeft,
                yml_bboxright = sprite.MarginRight,
                yml_bboxtop = sprite.MarginTop,
                yml_bboxbottom = sprite.MarginBottom,
                yml_sepmask = (uint)sprite.SepMasks,
                yml_speedtype = (uint)sprite.GMS2PlaybackSpeedType,
                yml_framespeed = sprite.GMS2PlaybackSpeed
            };

            var data = new Dictionary<string, SpriteData> { [spriteName] = config };
            var yamlBytes = YamlSerializer.Serialize(data);
            string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
            string configPath = Path.Combine(_texConfigFolder, spriteName + ".yaml");
            File.WriteAllText(configPath, yaml);

            using (var stripImage = new MagickImage(MagickColors.Transparent, totalWidth, maxHeight)) {
                int offsetX = 0;
                foreach (var image in images) {
                    stripImage.Composite(image, offsetX, 0, CompositeOperator.Over);
                    offsetX += (int)image.Width;
                    image.Dispose();
                }

                string stripPath = Path.Combine(_texFolder, spriteName + ext);
                stripImage.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "all");
                stripImage.Settings.Compression = CompressionMethod.Zip;
                stripImage.Write(stripPath);
            }
        }

        private static void DumpBackground(UndertaleBackground background)
        {
            if (background.Texture == null)
                return;

            string bgName = background.Name.Content;

            var config = new BackgroundData {
                yml_tile_count = background.GMS2TileCount,
                yml_tile_width = background.GMS2TileWidth,
                yml_tile_height = background.GMS2TileHeight,
                yml_border_x = background.GMS2OutputBorderX,
                yml_border_y = background.GMS2OutputBorderY,
                yml_tile_column = background.GMS2TileColumns,
                yml_item_per_tile = background.GMS2ItemsPerTileCount,
                yml_transparent = background.Transparent,
                yml_smooth = background.Smooth,
                yml_preload = background.Preload,
                yml_frametime = background.GMS2FrameLength,
            };

            var data = new Dictionary<string, BackgroundData> { [bgName] = config };
            var yamlBytes = YamlSerializer.Serialize(data);
            string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
            string configPath = Path.Combine(_bgConfigFolder, bgName + ".yaml");
            File.WriteAllText(configPath, yaml);

            string outputPath = Path.Combine(_bgFolder, bgName + ext);
            using (var image = _worker.GetTextureFor(background.Texture, bgName, _padded)) {
                image.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "all");
                image.Settings.Compression = CompressionMethod.Zip;
                image.Write(outputPath);
            }

            Log.Information($"Exported {bgName}");
        }
    }
}