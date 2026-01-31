using System.Text;

namespace FormatFlow.Core.Subtitles.Srt
{
    public class SrtReader : ISubtitleReader
    {
        public string Format => "srt";

        public SubtitleDocument Read(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(stream, encoding, leaveOpen: true);

            var entries = new List<SubtitleEntry>();
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                // Skip sequence number
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Read timestamp line
                var timeLine = reader.ReadLine();
                if (timeLine == null) break;

                var (start, end) = ParseTimestamp(timeLine);

                // Read text lines until empty line
                var textBuilder = new StringBuilder();
                while ((line = reader.ReadLine()) != null && !string.IsNullOrWhiteSpace(line))
                {
                    if (textBuilder.Length > 0)
                        textBuilder.AppendLine();
                    textBuilder.Append(line);
                }

                entries.Add(new SubtitleEntry
                {
                    StartTime = start,
                    EndTime = end,
                    Text = textBuilder.ToString()
                });
            }

            return new SubtitleDocument { Entries = entries };
        }

        public Task<SubtitleDocument> ReadAsync(Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            // For simplicity, wrap sync version (optimize later if needed)
            return Task.Run(() => Read(stream, encoding), cancellationToken);
        }

        private static (TimeSpan Start, TimeSpan End) ParseTimestamp(string line)
        {
            // "00:00:20,000 --> 00:00:24,400"
            var parts = line.Split(" --> ");
            return (ParseTime(parts[0]), ParseTime(parts[1]));
        }

        private static TimeSpan ParseTime(string time)
        {
            // "00:00:20,000" → 20 seconds
            var parts = time.Split(':', ',');
            return new TimeSpan(
                int.Parse(parts[0]),  // hours
                int.Parse(parts[1]),  // minutes
                int.Parse(parts[2]),  // seconds
                int.Parse(parts[3])   // milliseconds
            );
        }
    }
}
