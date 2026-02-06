using Serilog;
using UFO_50_Mod_Converter.Models;
using UndertaleModLib;
using UndertaleModLib.Models;
using VYaml.Serialization;

namespace UFO_50_Mod_Converter.ExportScripts
{
    public static class ExportAudio
    {
        private static readonly byte[] EmptyWavBytes = Convert.FromBase64String(
            "UklGRiQAAABXQVZFZm10IBAAAAABAAIAQB8AAAB9AAAEABAAZGF0YQAAAAA=");

        private const string DefaultAudioGroupName = "audiogroup_default";

        private static string _audioOutputPath;
        private static string _audioConfigPath;
        private static UndertaleData _data;
        private static Dictionary<string, IList<UndertaleEmbeddedAudio>> _loadedAudioGroups;

        public static void Export(string outputRoot, string audioFolder, string audioConfigFolder)
        {
            _data = UFO_50_Mod_Converter.Export.Data;
            _audioOutputPath = Path.Combine(outputRoot, audioFolder);
            _audioConfigPath = Path.Combine(outputRoot, audioConfigFolder);
            _loadedAudioGroups = new Dictionary<string, IList<UndertaleEmbeddedAudio>>();

            Directory.CreateDirectory(_audioOutputPath);
            Directory.CreateDirectory(_audioConfigPath);

            Log.Information($"Exporting audio to {_audioOutputPath}");

            foreach (var sound in _data.Sounds) {
                if (sound != null) {
                    try {
                        ExportSound(sound);
                    }
                    catch (Exception ex) {
                        Log.Error($"Failed to export {sound.Name?.Content}: {ex.Message}");
                    }
                }
            }

            Log.Information($"Audio export complete");
        }

        private static void ExportSound(UndertaleSound sound)
        {
            string soundName = sound.Name.Content;
            Log.Information($"Exporting {soundName}");

            bool isCompressed = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsCompressed);
            bool isEmbedded = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsEmbedded);

            string audioExt;
            bool embedded;

            if (isEmbedded && !isCompressed) {
                audioExt = ".wav";
                embedded = true;
            }
            else if (!isCompressed && !isEmbedded) {
                audioExt = ".ogg";
                embedded = false;
            }
            else {
                audioExt = ".ogg";
                embedded = true;
            }

            if (embedded) {
                byte[] audioData = GetSoundData(sound);
                string outputPath = Path.Combine(_audioOutputPath, soundName + audioExt);
                File.WriteAllBytes(outputPath, audioData);
            }
            else {
                ExportExternalAudio(sound, soundName, audioExt);
            }

            WriteAudioConfig(sound, soundName, audioExt, embedded, isCompressed);
        }

        private static void ExportExternalAudio(UndertaleSound sound, string soundName, string audioExt)
        {
            string externalFilename = sound.File?.Content;
            if (string.IsNullOrEmpty(externalFilename))
                return;

            if (!externalFilename.Contains('.'))
                externalFilename += ".ogg";

            if (!File.Exists(externalFilename)) {
                Log.Warning($"External audio file not found: {externalFilename}");
                return;
            }

            string externalDir = Path.Combine(_audioOutputPath, "external");
            Directory.CreateDirectory(externalDir);

            string destPath = Path.Combine(externalDir, soundName + audioExt);
            File.Copy(externalFilename, destPath, true);
        }

        private static byte[] GetSoundData(UndertaleSound sound)
        {
            if (sound.AudioFile?.Data != null)
                return sound.AudioFile.Data;

            if (sound.GroupID > _data.GetBuiltinSoundGroupID()) {
                var audioGroup = GetAudioGroupData(sound);
                if (audioGroup != null && sound.AudioID < audioGroup.Count)
                    return audioGroup[sound.AudioID].Data;
            }

            return EmptyWavBytes;
        }

        private static IList<UndertaleEmbeddedAudio> GetAudioGroupData(UndertaleSound sound)
        {
            string groupName = sound.AudioGroup?.Name?.Content ?? DefaultAudioGroupName;

            if (_loadedAudioGroups.TryGetValue(groupName, out var cached))
                return cached;

            string relativePath = sound.AudioGroup is { Path.Content: string customPath }
                ? customPath
                : $"audiogroup{sound.GroupID}.dat";

            if (!File.Exists(relativePath)) {
                Log.Warning($"Audio group file not found: {relativePath}");
                return null;
            }

            try {
                using var stream = new FileStream(relativePath, FileMode.Open, FileAccess.Read);
                var groupData = UndertaleIO.Read(stream, (warning, _) =>
                    Log.Warning($"Warning loading {groupName}: {warning}"));

                _loadedAudioGroups[groupName] = groupData.EmbeddedAudio;
                return groupData.EmbeddedAudio;
            }
            catch (Exception ex) {
                Log.Error($"Failed to load audio group {groupName}: {ex.Message}");
                return null;
            }
        }

        private static void WriteAudioConfig(UndertaleSound sound, string soundName,
            string audioExt, bool embedded, bool compressed)
        {
            var config = new AudioData {
                yml_type = audioExt,
                yml_embedded = embedded,
                yml_compressed = compressed,
                yml_effects = sound.Effects,
                yml_volume = sound.Volume,
                yml_pitch = sound.Pitch,
                yml_audiogroup_index = sound.GroupID,
                yml_audiofile_id = sound.AudioID,
                yml_preload = sound.Preload
            };

            var data = new Dictionary<string, AudioData> { [soundName] = config };
            var yamlBytes = YamlSerializer.Serialize(data);
            string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);

            string configPath = Path.Combine(_audioConfigPath, soundName + ".yaml");
            File.WriteAllText(configPath, yaml);
        }
    }
}