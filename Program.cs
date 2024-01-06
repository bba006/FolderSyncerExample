using CommandLine;

namespace FolderSyncer;

public class Program
{
    private class CliOptions
    {
        [Option("copy-from-path", Default = "D:\\FolderSyncer\\OriginalFolder")]
        public string InitialFolderPath { get; set; } = default!;

        [Option("copy-to-path", Default = "D:\\FolderSyncer\\NewFolder")]
        public string NewFolderPath { get; set; } = default!;

        [Option("sync-interval-ms", Default = 5000)]
        public int SynchronizationInterval { get; set; } = default!;

        [Option("log-file-path", Default = "D:\\FolderSyncer\\log.txt")]
        public string LogFilePath { get; set; } = default!;
    }

    private static void Main(string[] args)
    {
       var parser = new Parser(with =>
       {
           with.AutoHelp = true;
           with.AutoVersion = true;
           with.HelpWriter = Parser.Default.Settings.HelpWriter;
       });

        ParserResult<CliOptions> parserResult = parser.ParseArguments<CliOptions>(args);

        parserResult.WithParsed((CliOptions opts) => RunSyncer(opts, args));
    }

    private static void RunSyncer(CliOptions opts, string[] args)
    {
        var logger = Logging.BuildLogger(opts.LogFilePath);
        var fileManipulation = new FileManipulations();

        while (true)
        {
            if (Directory.Exists(opts.InitialFolderPath))
            {
                fileManipulation.Manipulate(opts.InitialFolderPath, opts.NewFolderPath, logger);
            }
            else logger.Error("Directory does not exist.");
            Thread.Sleep(opts.SynchronizationInterval);
        }              
    }
}