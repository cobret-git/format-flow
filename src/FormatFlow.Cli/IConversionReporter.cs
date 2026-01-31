namespace FormatFlow.Cli
{
    /// <summary>
    /// Abstraction for reporting conversion progress and results.
    /// Allows for different output targets (console, file, etc.)
    /// </summary>
    public interface IConversionReporter
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Success(string input, string output);
        void Failure(string input, string reason);
        void Skip(string input, string reason);
        void Summary(ConversionSummary summary);
        void Prompt();
    }
}
