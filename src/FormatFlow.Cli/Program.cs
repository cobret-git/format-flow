using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Sbv;
using FormatFlow.Core.Subtitles.Srt;
using FormatFlow.Core.Subtitles.Vtt;

namespace FormatFlow.Cli
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var converter = CreateConverter();
            var reporter = new ConsoleReporter();

            var app = new CliApp(converter, reporter);
            return await app.RunAsync(args);
        }

        private static SubtitleConverter CreateConverter()
        {
            var readers = new ISubtitleReader[] { new SrtReader(), new VttReader(), new SbvReader() };
            var writers = new ISubtitleWriter[] { new SrtWriter(), new VttWriter(), new SbvWriter() };
            return new SubtitleConverter(readers, writers);
        }
    }
}
