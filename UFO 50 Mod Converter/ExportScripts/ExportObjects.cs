using Serilog;
using UndertaleModLib.Models;
using Newtonsoft.Json;

namespace UFO_50_Mod_Converter.ExportScripts
{
    public static class ExportObjects
    {
        private static string _outputPath;

        public static void Export(string outputRoot, string exportedObjectDataOutputFolder)
        {
            var data = UFO_50_Mod_Converter.Export.Data;
            _outputPath = Path.Combine(outputRoot, exportedObjectDataOutputFolder);
            Directory.CreateDirectory(_outputPath);

            int coreCount = Math.Max(1, Environment.ProcessorCount - 1);
            var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount };

            Log.Information($"Using {coreCount} cores to dump object data");

            Parallel.ForEach(data.GameObjects, options, DumpObject);

            Log.Information($"All objects have been exported to {Path.GetFullPath(_outputPath)}");
        }

        private static void DumpObject(UndertaleGameObject obj)
        {
            if (obj == null)
                return;

            string objName = obj.Name?.Content;
            if (string.IsNullOrEmpty(objName)) {
                Log.Warning("Skipped object with empty name");
                return;
            }

            var objData = new {
                Sprite = obj.Sprite?.Name?.Content,
                Parent = obj.ParentId?.Name?.Content,
                TextureMaskID = obj.TextureMaskId?.Name?.Content,
                CollisionShape = obj.CollisionShape.ToString(),
                IsVisible = obj.Visible,
                IsSolid = obj.Solid,
                IsPersistent = obj.Persistent
            };

            string json = JsonConvert.SerializeObject(objData, Formatting.Indented);
            string outputFileName = Path.Combine(_outputPath, $"{objName}.json");

            try {
                File.WriteAllText(outputFileName, json);
                Log.Information($"Exported {objName}");
            }
            catch (Exception ex) {
                Log.Error($"Failed to export {objName}: {ex.Message}");
            }
        }
    }
}