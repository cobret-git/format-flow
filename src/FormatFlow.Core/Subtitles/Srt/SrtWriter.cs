using System.Text;

namespace FormatFlow.Core.Subtitles.Srt
{
    public class SrtWriter : ISubtitleWriter
    {
        public string Format => "srt";

        public void Write(SubtitleDocument document, Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var writer = new StreamWriter(stream, encoding, leaveOpen: true);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                var entry = document.Entries[i];

                // Sequence number (1-based)
                writer.WriteLine(i + 1);

                // Timestamp line
                writer.WriteLine($"{FormatTime(entry.StartTime)} --> {FormatTime(entry.EndTime)}");

                // Text content
                writer.WriteLine(entry.Text);

                // Empty line separator (except after last entry)
                if (i < document.Entries.Count - 1)
                    writer.WriteLine();
            }

            writer.Flush();
        }

        public Task WriteAsync(SubtitleDocument document, Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Write(document, stream, encoding), cancellationToken);
        }

        private static string FormatTime(TimeSpan time)
        {
            // Format: "00:00:20,000"
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
        }
    }
}
