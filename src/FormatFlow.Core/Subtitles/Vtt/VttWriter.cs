using System.Text;

namespace FormatFlow.Core.Subtitles.Vtt
{
    public class VttWriter : ISubtitleWriter
    {
        public string Format => "vtt";

        public void Write(SubtitleDocument document, Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var writer = new StreamWriter(stream, encoding, leaveOpen: true);

            // Write WEBVTT header
            if (!string.IsNullOrWhiteSpace(document.Metadata.Title))
            {
                writer.WriteLine($"WEBVTT {document.Metadata.Title}");
            }
            else
            {
                writer.WriteLine("WEBVTT");
            }

            // Empty line after header
            writer.WriteLine();

            for (int i = 0; i < document.Entries.Count; i++)
            {
                var entry = document.Entries[i];

                // Cue identifier (1-based)
                writer.WriteLine(i + 1);

                // Timestamp line with optional cue settings
                var timestampLine = $"{FormatTime(entry.StartTime)} --> {FormatTime(entry.EndTime)}";
                var settings = FormatCueSettings(entry.Position);
                if (!string.IsNullOrEmpty(settings))
                {
                    timestampLine += $" {settings}";
                }
                writer.WriteLine(timestampLine);

                // Text content (with optional voice label wrapping)
                var text = entry.Text;
                if (!string.IsNullOrWhiteSpace(entry.VoiceLabel) && !text.Contains("<v "))
                {
                    // Wrap text with voice tag if not already present
                    text = $"<v {entry.VoiceLabel}>{text}</v>";
                }
                writer.WriteLine(text);

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
            // Format: "00:00:20.000" (VTT uses . not ,)
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }

        private static string FormatCueSettings(ISubtitlePosition? position)
        {
            if (position == null)
                return string.Empty;

            var parts = new List<string>();

            // Handle VttPosition natively with full fidelity
            if (position is VttPosition vtt)
            {
                if (vtt.Vertical != null)
                    parts.Add($"vertical:{vtt.Vertical}");

                if (vtt.Line.HasValue)
                    parts.Add($"line:{vtt.Line.Value}");

                if (vtt.Position.HasValue)
                    parts.Add($"position:{vtt.Position.Value}%");

                if (vtt.Size.HasValue)
                    parts.Add($"size:{vtt.Size.Value}%");

                if (vtt.HorizontalAlign.HasValue)
                {
                    var alignStr = FormatAlignment(vtt.HorizontalAlign.Value);
                    if (alignStr != null)
                        parts.Add($"align:{alignStr}");
                }
            }
            else
            {
                // Generic fallback: extract only what VTT supports from any position type
                if (position.HorizontalAlign.HasValue)
                {
                    var alignStr = FormatAlignment(position.HorizontalAlign.Value);
                    if (alignStr != null)
                        parts.Add($"align:{alignStr}");
                }
            }

            return string.Join(" ", parts);
        }

        private static string? FormatAlignment(PositionAlignment align)
        {
            return align switch
            {
                PositionAlignment.Start => "start",
                PositionAlignment.Center => "center",
                PositionAlignment.End => "end",
                _ => null
            };
        }
    }
}
