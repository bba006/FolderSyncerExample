using Serilog;

namespace FolderSyncer
{
    public static class Logging
    {
        public static ILogger BuildLogger(string path)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.File(path)
                .WriteTo.Console()
                .CreateLogger();

            return logger;
        }
    }
}
