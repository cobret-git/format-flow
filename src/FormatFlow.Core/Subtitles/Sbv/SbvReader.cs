using System.Text;
using System.Text.RegularExpressions;

namespace FormatFlow.Core.Subtitles.Sbv
{
    public class SbvReader : ISubtitleReader
    {
        public string Format => "sbv";

        // Regex for timestamp line: "0:00:05.000,0:00:10.500" or "00:00:05.000,00:00:10.500"
        private static readonly Regex TimestampRegex = new(
            @"^(\d{1,2}:\d{2}:\d{2}\.\d{3}),(\d{1,2}:\d{2}:\d{2}\.\d{3})$",
            RegexOptions.Compiled);

        public SubtitleDocument Read(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(stream, encoding, leaveOpen: true);

            var entries = new List<SubtitleEntry>();
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Try to parse as timestamp line
                var match = TimestampRegex.Match(line);
                if (!match.Success)
                {
                    throw new FormatException($"Invalid SBV timestamp format: {line}");
                }

                var startTime = ParseTime(match.Groups[1].Value);
                var endTime = ParseTime(match.Groups[2].Value);

                // Read text lines until empty line or end of file
                var textBuilder = new StringBuilder();
                while ((line = reader.ReadLine()) != null && !string.IsNullOrWhiteSpace(line))
                {
                    if (textBuilder.Length > 0)
                        textBuilder.AppendLine();
                    textBuilder.Append(line);
                }

                entries.Add(new SubtitleEntry
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = textBuilder.ToString()
                });
            }

            return new SubtitleDocument { Entries = entries };
        }

        public Task<SubtitleDocument> ReadAsync(Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Read(stream, encoding), cancellationToken);
        }

        private static TimeSpan ParseTime(string time)
        {
            // "0:00:20.000" or "00:00:20.000" → TimeSpan
            var parts = time.Split(':', '.');

            return new TimeSpan(
                0, // days
                int.Parse(parts[0]),  // hours
                int.Parse(parts[1]),  // minutes
                int.Parse(parts[2]),  // seconds
                int.Parse(parts[3])   // milliseconds
            );
        }
    }
}