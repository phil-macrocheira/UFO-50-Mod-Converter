using Serilog;
using Underanalyzer.Decompiler;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;

namespace UFO_50_Mod_Converter.ExportScripts
{
    public static class ExportCode
    {
        public static void Export(string outputRoot, string ExportedCodeOutputFolder)
        {
            var Data = UFO_50_Mod_Converter.Export.Data;
            string outputPath = Path.Combine(outputRoot, ExportedCodeOutputFolder);
            Directory.CreateDirectory(outputPath);

            var globalDecompileContext = new GlobalDecompileContext(Data);
            var decompilerSettings = Data.ToolInfo.DecompilerSettings;
            List<UndertaleCode> toDump = Data.Code.Where(c => c.ParentEntry is null).ToList();

            int coreCount = CalculateOptimalCoreCount();

            Log.Information($"Using {coreCount} cores to dump code");

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = coreCount };
            Parallel.ForEach(toDump, parallelOptions, code => {
                DumpSingleCode(code, outputPath, globalDecompileContext, decompilerSettings);
            });

            Log.Information($"All code has been exported to {Path.GetFullPath(outputPath)}");
        }
        private static int CalculateOptimalCoreCount()
        {
            int availableCores = Environment.ProcessorCount;
            return Math.Max(1, availableCores - 1);
        }
        private static void DumpSingleCode(UndertaleCode code, string codeFolder, GlobalDecompileContext globalDecompileContext, IDecompileSettings decompilerSettings)
        {
            if (code is null) {
                Log.Warning("Encountered null code entry");
                return;
            }

            string fileName = $"{code.Name.Content}.gml";
            string filePath = Path.Combine(codeFolder, fileName);

            Log.Information($"Exporting: {code.Name.Content}");

            try {
                var context = new DecompileContext(globalDecompileContext, code, decompilerSettings);
                string decompiledCode = context.DecompileToString();
                File.WriteAllText(filePath, decompiledCode);
            }
            catch (Exception ex) {
                HandleDecompilationError(filePath, code.Name.Content, ex);
            }
        }
        private static void HandleDecompilationError(string filePath, string codeName, Exception exception)
        {
            Log.Error($"Decompilation failed for {codeName}: {exception.Message}");

            string errorContent = $"/*\nDECOMPILATION FAILED: {codeName}\n\n" +
                                 $"Error: {exception.Message}\n\n" +
                                 $"Stack Trace:\n{exception.StackTrace}\n*/";

            try {
                File.WriteAllText(filePath, errorContent);
            }
            catch (Exception fileEx) {
                Log.Error($"Failed to write error file for {codeName}: {fileEx.Message}");
            }
        }
    }
}