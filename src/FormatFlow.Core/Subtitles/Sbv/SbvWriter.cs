using System.Text;

namespace FormatFlow.Core.Subtitles.Sbv
{
    public class SbvWriter : ISubtitleWriter
    {
        public string Format => "sbv";

        public void Write(SubtitleDocument document, Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var writer = new StreamWriter(stream, encoding, leaveOpen: true);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                var entry = document.Entries[i];

                // Timestamp line: "0:00:05.000,0:00:10.500"
                writer.WriteLine($"{FormatTime(entry.StartTime)},{FormatTime(entry.EndTime)}");

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
            // Format: "0:00:20.000" (single digit hour when < 10)
            return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }
    }
}