using Serilog;
using Serilog.Events;
using Config.Net;
using UFO_50_Mod_Converter;
using UFO_50_Mod_Converter.Models;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File(Constants.LogPath, rollingInterval: RollingInterval.Infinite, fileSizeLimitBytes: null, rollOnFileSizeLimit: false, shared: false)
    .CreateLogger();

Settings.Config = new ConfigurationBuilder<IConfig>().UseIniFile(Constants.IniPath).Build();

try {
    Export.Start();
    Compare.Start();
}
catch (Exception ex) {
    Log.Fatal(ex, "An unhandled exception occurred");
}
finally {
    Log.CloseAndFlush();
}