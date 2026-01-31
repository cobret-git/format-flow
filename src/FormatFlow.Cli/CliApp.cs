using FormatFlow.Core.Subtitles;

namespace FormatFlow.Cli;

public class CliApp
{
    private readonly SubtitleConverter _converter;
    private readonly IConversionReporter _reporter;
    private readonly Func<string?> _readLine;
    private bool _isRunning;

    public CliApp(SubtitleConverter converter, IConversionReporter reporter)
        : this(converter, reporter, Console.ReadLine)
    {
    }

    // Constructor for testing with custom input source
    public CliApp(SubtitleConverter converter, IConversionReporter reporter, Func<string?> readLine)
    {
        _converter = converter;
        _reporter = reporter;
        _readLine = readLine;
    }

    public async Task<int> RunAsync(string[] args)
    {
        // Single command mode (non-interactive)
        if (args.Length > 0)
        {
            return await ExecuteCommandAsync(args);
        }

        // Interactive REPL mode
        return await RunInteractiveAsync();
    }

    private async Task<int> RunInteractiveAsync()
    {
        PrintGreeting();
        _isRunning = true;

        while (_isRunning)
        {
            _reporter.Prompt();
            var input = _readLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            var args = ParseInputLine(input);
            await ExecuteCommandAsync(args);
        }

        return 0;
    }

    private async Task<int> ExecuteCommandAsync(string[] args)
    {
        if (args.Length == 0)
            return 0;

        return args[0].ToLowerInvariant() switch
        {
            "convert" => await HandleConvertAsync(args[1..]),
            "formats" => HandleFormats(),
            "help" or "-h" or "--help" => HandleHelp(),
            "exit" or "quit" or "q" => HandleExit(),
            "clear" or "cls" => HandleClear(),
            _ => HandleUnknown(args[0])
        };
    }

    private static string[] ParseInputLine(string input)
    {
        // Simple parsing - splits on spaces, respects quoted strings
        var args = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (current.Length > 0)
            args.Add(current);

        return args.ToArray();
    }

    private async Task<int> HandleConvertAsync(string[] args)
    {
        var options = ConvertOptions.Parse(args);

        if (options.Error != null)
        {
            _reporter.Error(options.Error);
            return 1;
        }

        var command = new ConvertCommand(_converter, _reporter);
        return await command.ExecuteAsync(options);
    }

    private int HandleFormats()
    {
        _reporter.Info("Supported formats:");
        foreach (var format in _converter.SupportedFormats)
        {
            _reporter.Info($"  .{format}");
        }
        return 0;
    }

    private int HandleHelp()
    {
        PrintHelp();
        return 0;
    }

    private int HandleExit()
    {
        _reporter.Info("Goodbye!");
        _isRunning = false;
        return 0;
    }

    private int HandleClear()
    {
        Console.Clear();
        return 0;
    }

    private int HandleUnknown(string command)
    {
        _reporter.Error($"Unknown command: {command}");
        _reporter.Info("Type 'help' for available commands");
        return 1;
    }

    private void PrintGreeting()
    {
        _reporter.Info("""
            
            ╔═══════════════════════════════════════╗
            ║     FormatFlow - Subtitle Converter   ║
            ╚═══════════════════════════════════════╝
            
            Type 'help' for commands or 'exit' to quit.
            """);
    }

    private void PrintHelp()
    {
        _reporter.Info("""
            Available Commands:
              convert <path> --to <format>   Convert subtitle files
              formats                        List supported formats
              clear                          Clear the screen
              help                           Show this help
              exit                           Exit the application

            Convert Options:
              --from <format>    Source format (default: auto-detect)
              --to <format>      Target format (required)
              --recursive, -r    Process subdirectories
              --overwrite, -f    Overwrite existing files

            Examples:
              convert ./subs --from vtt --to srt
              convert ./video.vtt --to srt
              convert ./media --to srt --recursive
            """);
    }
}