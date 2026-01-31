namespace FormatFlow.Cli
{
    public class ConsoleReporter : IConversionReporter
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Warning(string message)
        {
            WriteColored($"⚠ {message}", ConsoleColor.Yellow);
        }

        public void Error(string message)
        {
            WriteColored($"✗ {message}", ConsoleColor.Red);
        }

        public void Success(string input, string output)
        {
            var fileName = Path.GetFileName(input);
            var outputName = Path.GetFileName(output);
            WriteColored($"✓ {fileName} → {outputName}", ConsoleColor.Green);
        }

        public void Failure(string input, string reason)
        {
            var fileName = Path.GetFileName(input);
            WriteColored($"✗ {fileName}: {reason}", ConsoleColor.Red);
        }

        public void Skip(string input, string reason)
        {
            var fileName = Path.GetFileName(input);
            WriteColored($"○ {fileName}: {reason}", ConsoleColor.DarkGray);
        }

        public void Summary(ConversionSummary summary)
        {
            Console.WriteLine();
            Console.WriteLine($"Completed: {summary.Succeeded} converted, {summary.Failed} failed, {summary.Skipped} skipped");
        }

        public void Prompt()
        {
            Console.WriteLine();
            WriteColored("formatflow> ", ConsoleColor.Cyan, newLine: false);
        }

        private static void WriteColored(string message, ConsoleColor color, bool newLine = true)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ForegroundColor = previousColor;
        }
    }
}
