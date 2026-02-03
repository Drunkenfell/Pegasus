using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Pegasus.Configuration
{
    public static class ConfigManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static Config Config { get; private set; }

        public static void Initialise(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    // try a sane fallback: config.example.json next to executable
                    var examplePath = Path.Combine(Path.GetDirectoryName(path) ?? ",", "config.example.json");
                    if (File.Exists(examplePath))
                    {
                        log.Warn("Config file '{0}' not found; falling back to example config '{1}'", path, examplePath);
                        path = examplePath;
                    }
                    else
                    {
                        log.Fatal("Config file '{0}' not found and no example config at '{1}'.", path, examplePath);
                        throw new FileNotFoundException($"Config file not found: {path}");
                    }
                }

                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                log.Fatal(exception, "Failed to load configuration from '{0}'", path);
                throw;
            }
        }
    }
}
