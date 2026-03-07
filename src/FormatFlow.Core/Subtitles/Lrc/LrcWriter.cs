using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Core.Subtitles.Lrc
{
    public class LrcWriter : ISubtitleWriter
    {
        public string Format => "lrc";

        public void Write(SubtitleDocument document, Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var writer = new StreamWriter(stream, encoding, leaveOpen: true);

            WriteHeader(writer, document.Metadata);

            foreach (var entry in document.Entries)
            {
                // Blank entries carry no lyric content; skip them.
                if (string.IsNullOrWhiteSpace(entry.Text))
                    continue;

                writer.WriteLine($"[{FormatTime(entry.StartTime)}]{entry.Text}");
            }

            writer.Flush();
        }

        public Task WriteAsync(
            SubtitleDocument document,
            Stream stream,
            Encoding? encoding = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Write(document, stream, encoding), cancellationToken);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void WriteHeader(StreamWriter writer, SubtitleMetadata metadata)
        {
            // Emit known tags in the conventional LRC order, then a blank separator.
            if (!string.IsNullOrWhiteSpace(metadata.Title))
                writer.WriteLine($"[ti:{metadata.Title}]");

            if (metadata.CustomProperties.TryGetValue("ar", out var artist))
                writer.WriteLine($"[ar:{artist}]");

            if (metadata.CustomProperties.TryGetValue("al", out var album))
                writer.WriteLine($"[al:{album}]");

            // Always emit [offset:0]; we apply any offset during conversion, not at write time.
            writer.WriteLine("[offset:0]");
            writer.WriteLine();
        }

        /// <summary>
        /// Formats a TimeSpan as LRC's MM:SS.xx (centiseconds, 2 digits).
        /// Minutes are not capped at 59 — LRC supports values like [125:30.00].
        /// </summary>
        private static string FormatTime(TimeSpan time)
        {
            var totalMinutes = (int)time.TotalMinutes;
            var seconds = time.Seconds;
            var centiseconds = time.Milliseconds / 10;
            return $"{totalMinutes:D2}:{seconds:D2}.{centiseconds:D2}";
        }
    }
}
