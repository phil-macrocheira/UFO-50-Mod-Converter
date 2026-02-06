using Serilog;
using System.Text.Json;
using UndertaleModLib.Models;

namespace UFO_50_Mod_Converter.ExportScripts
{
    public static class ExportRooms
    {
        private static readonly JsonWriterOptions WriterOptions = new() { Indented = true };
        private static string _outputPath;

        public static void Export(string outputRoot, string exportedRoomDataOutputFolder)
        {
            var data = UFO_50_Mod_Converter.Export.Data;
            _outputPath = Path.Combine(outputRoot, exportedRoomDataOutputFolder);
            Directory.CreateDirectory(_outputPath);

            Log.Information($"Exporting rooms to {_outputPath}");

            foreach (var room in data.Rooms) {
                if (room == null) continue;

                try {
                    Log.Information($"Exporting {room.Name.Content}");
                    WriteRoomToJson(room);
                }
                catch (Exception ex) {
                    Log.Error($"Failed to export room {room.Name?.Content}: {ex.Message}");
                }
            }

            Log.Information("Room export complete");
        }

        private static void WriteRoomToJson(UndertaleRoom room)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, WriterOptions);

            writer.WriteStartObject();
            WriteRoomProperties(writer, room);
            WriteBackgrounds(writer, room);
            WriteViews(writer, room);
            WriteGameObjects(writer, room);
            WriteTiles(writer, room);
            WriteLayers(writer, room);
            writer.WriteEndObject();

            writer.Flush();

            string outputFile = Path.Combine(_outputPath, room.Name.Content + ".json");
            File.WriteAllBytes(outputFile, stream.ToArray());
        }

        private static void WriteString(Utf8JsonWriter writer, string name, UndertaleString str)
        {
            if (str?.Content == null)
                writer.WriteNull(name);
            else
                writer.WriteString(name, str.Content);
        }

        private static void WriteRoomProperties(Utf8JsonWriter writer, UndertaleRoom room)
        {
            WriteString(writer, "caption", room.Caption);
            writer.WriteNumber("width", room.Width);
            writer.WriteNumber("height", room.Height);
            writer.WriteNumber("speed", room.Speed);
            writer.WriteBoolean("persistent", room.Persistent);
            writer.WriteNumber("background_color", room.BackgroundColor ^ 0xFF000000);
            writer.WriteBoolean("draw_background_color", room.DrawBackgroundColor);
            WriteString(writer, "creation_code_id", room.CreationCodeId?.Name);
            writer.WriteNumber("flags", Convert.ToInt32(room.Flags));
            writer.WriteBoolean("world", room.World);
            writer.WriteNumber("top", room.Top);
            writer.WriteNumber("left", room.Left);
            writer.WriteNumber("right", room.Right);
            writer.WriteNumber("bottom", room.Bottom);
            writer.WriteNumber("gravity_x", room.GravityX);
            writer.WriteNumber("gravity_y", room.GravityY);
            writer.WriteNumber("meters_per_pixel", room.MetersPerPixel);
        }

        private static void WriteBackgrounds(Utf8JsonWriter writer, UndertaleRoom room)
        {
            writer.WriteStartArray("backgrounds");
            if (room.Backgrounds != null) {
                foreach (var bg in room.Backgrounds) {
                    writer.WriteStartObject();
                    if (bg != null) {
                        writer.WriteBoolean("enabled", bg.Enabled);
                        writer.WriteBoolean("foreground", bg.Foreground);
                        WriteString(writer, "background_definition", bg.BackgroundDefinition?.Name);
                        writer.WriteNumber("x", bg.X);
                        writer.WriteNumber("y", bg.Y);
                        writer.WriteBoolean("tiled_vertically", bg.TiledVertically);
                        writer.WriteBoolean("tiled_horizontally", bg.TiledHorizontally);
                        writer.WriteNumber("speed_x", bg.SpeedX);
                        writer.WriteNumber("speed_y", bg.SpeedY);
                        writer.WriteBoolean("stretch", bg.Stretch);
                    }
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndArray();
        }

        private static void WriteViews(Utf8JsonWriter writer, UndertaleRoom room)
        {
            writer.WriteStartArray("views");
            if (room.Views != null) {
                foreach (var view in room.Views) {
                    writer.WriteStartObject();
                    if (view != null) {
                        writer.WriteBoolean("enabled", view.Enabled);
                        writer.WriteNumber("view_x", view.ViewX);
                        writer.WriteNumber("view_y", view.ViewY);
                        writer.WriteNumber("view_width", view.ViewWidth);
                        writer.WriteNumber("view_height", view.ViewHeight);
                        writer.WriteNumber("port_x", view.PortX);
                        writer.WriteNumber("port_y", view.PortY);
                        writer.WriteNumber("port_width", view.PortWidth);
                        writer.WriteNumber("port_height", view.PortHeight);
                        writer.WriteNumber("border_x", view.BorderX);
                        writer.WriteNumber("border_y", view.BorderY);
                        writer.WriteNumber("speed_x", view.SpeedX);
                        writer.WriteNumber("speed_y", view.SpeedY);
                        WriteString(writer, "object_id", view.ObjectId?.Name);
                    }
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndArray();
        }

        private static void WriteGameObjects(Utf8JsonWriter writer, UndertaleRoom room)
        {
            writer.WriteStartArray("game_objects");
            if (room.GameObjects != null) {
                foreach (var go in room.GameObjects) {
                    WriteGameObject(writer, go);
                }
            }
            writer.WriteEndArray();
        }

        private static void WriteGameObject(Utf8JsonWriter writer, UndertaleRoom.GameObject go)
        {
            writer.WriteStartObject();
            if (go != null) {
                writer.WriteNumber("x", go.X);
                writer.WriteNumber("y", go.Y);
                WriteString(writer, "object_definition", go.ObjectDefinition?.Name);
                writer.WriteNumber("instance_id", go.InstanceID);
                WriteString(writer, "creation_code", go.CreationCode?.Name);
                writer.WriteNumber("scale_x", go.ScaleX);
                writer.WriteNumber("scale_y", go.ScaleY);
                writer.WriteNumber("color", go.Color);
                writer.WriteNumber("rotation", go.Rotation);
                WriteString(writer, "pre_create_code", go.PreCreateCode?.Name);
                writer.WriteNumber("image_speed", go.ImageSpeed);
                writer.WriteNumber("image_index", go.ImageIndex);
            }
            writer.WriteEndObject();
        }

        private static void WriteTiles(Utf8JsonWriter writer, UndertaleRoom room)
        {
            writer.WriteStartArray("tiles");
            if (room.Tiles != null) {
                foreach (var tile in room.Tiles) {
                    WriteTile(writer, tile);
                }
            }
            writer.WriteEndArray();
        }

        private static void WriteTile(Utf8JsonWriter writer, UndertaleRoom.Tile tile)
        {
            writer.WriteStartObject();
            if (tile != null) {
                writer.WriteBoolean("sprite_mode", tile.spriteMode);
                writer.WriteNumber("x", tile.X);
                writer.WriteNumber("y", tile.Y);
                WriteString(writer, "background_definition", tile.BackgroundDefinition?.Name);
                WriteString(writer, "sprite_definition", tile.SpriteDefinition?.Name);
                writer.WriteNumber("source_x", tile.SourceX);
                writer.WriteNumber("source_y", tile.SourceY);
                writer.WriteNumber("width", tile.Width);
                writer.WriteNumber("height", tile.Height);
                writer.WriteNumber("tile_depth", tile.TileDepth);
                writer.WriteNumber("instance_id", tile.InstanceID);
                writer.WriteNumber("scale_x", tile.ScaleX);
                writer.WriteNumber("scale_y", tile.ScaleY);
                writer.WriteNumber("color", tile.Color);
            }
            writer.WriteEndObject();
        }

        private static void WriteLayers(Utf8JsonWriter writer, UndertaleRoom room)
        {
            writer.WriteStartArray("layers");
            if (room.Layers != null) {
                foreach (var layer in room.Layers) {
                    WriteLayer(writer, layer);
                }
            }
            writer.WriteEndArray();
        }

        private static void WriteLayer(Utf8JsonWriter writer, UndertaleRoom.Layer layer)
        {
            writer.WriteStartObject();
            if (layer == null) {
                writer.WriteEndObject();
                return;
            }

            WriteString(writer, "layer_name", layer.LayerName);
            writer.WriteNumber("layer_id", layer.LayerId);
            writer.WriteNumber("layer_type", Convert.ToInt32(layer.LayerType));
            writer.WriteNumber("layer_depth", layer.LayerDepth);
            writer.WriteNumber("x_offset", layer.XOffset);
            writer.WriteNumber("y_offset", layer.YOffset);
            writer.WriteNumber("h_speed", layer.HSpeed);
            writer.WriteNumber("v_speed", layer.VSpeed);
            writer.WriteBoolean("is_visible", layer.IsVisible);

            writer.WriteStartObject("layer_data");
            if (layer.Data != null) {
                switch (layer.LayerType) {
                    case UndertaleRoom.LayerType.Background:
                        WriteBackgroundLayerData(writer, (UndertaleRoom.Layer.LayerBackgroundData)layer.Data);
                        break;
                    case UndertaleRoom.LayerType.Instances:
                        WriteInstancesLayerData(writer, (UndertaleRoom.Layer.LayerInstancesData)layer.Data);
                        break;
                    case UndertaleRoom.LayerType.Assets:
                        WriteAssetsLayerData(writer, (UndertaleRoom.Layer.LayerAssetsData)layer.Data);
                        break;
                    case UndertaleRoom.LayerType.Tiles:
                        WriteTilesLayerData(writer, (UndertaleRoom.Layer.LayerTilesData)layer.Data);
                        break;
                }
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        private static void WriteBackgroundLayerData(Utf8JsonWriter writer, UndertaleRoom.Layer.LayerBackgroundData data)
        {
            writer.WriteBoolean("visible", data.Visible);
            writer.WriteBoolean("foreground", data.Foreground);
            WriteString(writer, "sprite", data.Sprite?.Name);
            writer.WriteBoolean("tiled_horizontally", data.TiledHorizontally);
            writer.WriteBoolean("tiled_vertically", data.TiledVertically);
            writer.WriteBoolean("stretch", data.Stretch);
            writer.WriteNumber("color", data.Color);
            writer.WriteNumber("first_frame", data.FirstFrame);
            writer.WriteNumber("animation_speed", data.AnimationSpeed);
            writer.WriteNumber("animation_speed_type", Convert.ToInt32(data.AnimationSpeedType));
        }

        private static void WriteInstancesLayerData(Utf8JsonWriter writer, UndertaleRoom.Layer.LayerInstancesData data)
        {
            writer.WriteStartArray("instances");
            if (data.Instances != null) {
                foreach (var instance in data.Instances) {
                    WriteGameObject(writer, instance);
                }
            }
            writer.WriteEndArray();
        }

        private static void WriteAssetsLayerData(Utf8JsonWriter writer, UndertaleRoom.Layer.LayerAssetsData data)
        {
            writer.WriteStartArray("legacy_tiles");
            if (data.LegacyTiles != null) {
                foreach (var tile in data.LegacyTiles)
                    WriteTile(writer, tile);
            }
            writer.WriteEndArray();

            writer.WriteStartArray("sprites");
            if (data.Sprites != null) {
                foreach (var sprite in data.Sprites)
                    WriteSpriteInstance(writer, sprite);
            }
            writer.WriteEndArray();

            writer.WriteStartArray("sequences");
            if (data.Sequences != null) {
                foreach (var seq in data.Sequences)
                    WriteSequenceInstance(writer, seq);
            }
            writer.WriteEndArray();

            writer.WriteStartArray("nine_slices");
            if (data.NineSlices != null) {
                foreach (var slice in data.NineSlices)
                    WriteSpriteInstance(writer, slice);
            }
            writer.WriteEndArray();
        }

        private static void WriteSpriteInstance(Utf8JsonWriter writer, UndertaleRoom.SpriteInstance sprite)
        {
            writer.WriteStartObject();
            if (sprite != null) {
                WriteString(writer, "name", sprite.Name);
                WriteString(writer, "sprite", sprite.Sprite?.Name);
                writer.WriteNumber("x", sprite.X);
                writer.WriteNumber("y", sprite.Y);
                writer.WriteNumber("scale_x", sprite.ScaleX);
                writer.WriteNumber("scale_y", sprite.ScaleY);
                writer.WriteNumber("color", sprite.Color);
                writer.WriteNumber("animation_speed", sprite.AnimationSpeed);
                writer.WriteNumber("animation_speed_type", Convert.ToInt32(sprite.AnimationSpeedType));
                writer.WriteNumber("frame_index", sprite.FrameIndex);
                writer.WriteNumber("rotation", sprite.Rotation);
            }
            writer.WriteEndObject();
        }

        private static void WriteSequenceInstance(Utf8JsonWriter writer, UndertaleRoom.SequenceInstance seq)
        {
            writer.WriteStartObject();
            if (seq != null) {
                WriteString(writer, "name", seq.Name);
                WriteString(writer, "sequence", seq.Sequence?.Name);
                writer.WriteNumber("x", seq.X);
                writer.WriteNumber("y", seq.Y);
                writer.WriteNumber("scale_x", seq.ScaleX);
                writer.WriteNumber("scale_y", seq.ScaleY);
                writer.WriteNumber("color", seq.Color);
                writer.WriteNumber("animation_speed", seq.AnimationSpeed);
                writer.WriteNumber("animation_speed_type", Convert.ToInt32(seq.AnimationSpeedType));
                writer.WriteNumber("frame_index", seq.FrameIndex);
                writer.WriteNumber("rotation", seq.Rotation);
            }
            writer.WriteEndObject();
        }

        private static void WriteTilesLayerData(Utf8JsonWriter writer, UndertaleRoom.Layer.LayerTilesData data)
        {
            WriteString(writer, "background", data.Background?.Name);
            writer.WriteNumber("tiles_x", data.TilesX);
            writer.WriteNumber("tiles_y", data.TilesY);

            writer.WriteStartArray("tile_data");
            if (data.TileData != null) {
                foreach (var row in data.TileData) {
                    writer.WriteStartArray();
                    foreach (uint tileId in row) {
                        writer.WriteNumberValue(tileId);
                    }
                    writer.WriteEndArray();
                }
            }
            writer.WriteEndArray();
        }
    }
}