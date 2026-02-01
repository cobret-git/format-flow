using System.Text;
using System.Text.RegularExpressions;

namespace FormatFlow.Core.Subtitles.Vtt
{
    public class VttReader : ISubtitleReader
    {
        public string Format => "vtt";

        // Regex for timestamp line with optional cue settings
        // Supports both "00:00:01.000 --> 00:00:04.000" and "00:01.000 --> 00:04.000" formats
        private static readonly Regex TimestampRegex = new(
            @"^((?:\d{2}:)?\d{2}:\d{2}\.\d{3})\s*-->\s*((?:\d{2}:)?\d{2}:\d{2}\.\d{3})(.*)$",
            RegexOptions.Compiled);

        // Regex for voice tag: <v Speaker Name>text</v>
        private static readonly Regex VoiceTagRegex = new(
            @"<v\s+([^>]+)>",
            RegexOptions.Compiled);

        public SubtitleDocument Read(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(stream, encoding, leaveOpen: true);

            var entries = new List<SubtitleEntry>();

            // First line must be WEBVTT (with optional BOM)
            var firstLine = reader.ReadLine()?.Trim();
            if (firstLine == null || !firstLine.StartsWith("WEBVTT"))
            {
                throw new FormatException("Invalid VTT file: missing WEBVTT header");
            }

            // Parse optional header metadata after WEBVTT
            var headerParts = firstLine.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string? title = headerParts.Length > 1 ? headerParts[1] : null;

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                // Skip empty lines and NOTE comments
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("NOTE"))
                {
                    // Skip comment block until empty line
                    while ((line = reader.ReadLine()) != null && !string.IsNullOrWhiteSpace(line)) { }
                    continue;
                }

                // Skip STYLE blocks
                if (line.StartsWith("STYLE"))
                {
                    while ((line = reader.ReadLine()) != null && !string.IsNullOrWhiteSpace(line)) { }
                    continue;
                }

                // Check if this line is a timestamp or a cue identifier
                var timestampLine = line;
                var match = TimestampRegex.Match(line);

                if (!match.Success)
                {
                    // This might be a cue identifier, read next line for timestamp
                    timestampLine = reader.ReadLine();
                    if (timestampLine == null) break;

                    match = TimestampRegex.Match(timestampLine);
                    if (!match.Success)
                    {
                        throw new FormatException($"Invalid VTT timestamp format: {timestampLine}");
                    }
                }

                var startTime = ParseTime(match.Groups[1].Value);
                var endTime = ParseTime(match.Groups[2].Value);
                var position = ParseCueSettings(match.Groups[3].Value.Trim());

                // Read text lines until empty line
                var textBuilder = new StringBuilder();
                string? voiceLabel = null;

                while ((line = reader.ReadLine()) != null && !string.IsNullOrWhiteSpace(line))
                {
                    if (textBuilder.Length > 0)
                        textBuilder.AppendLine();

                    // Extract voice label from first occurrence
                    if (voiceLabel == null)
                    {
                        var voiceMatch = VoiceTagRegex.Match(line);
                        if (voiceMatch.Success)
                        {
                            voiceLabel = voiceMatch.Groups[1].Value;
                        }
                    }

                    textBuilder.Append(line);
                }

                entries.Add(new SubtitleEntry
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = textBuilder.ToString(),
                    VoiceLabel = voiceLabel,
                    Position = position
                });
            }

            return new SubtitleDocument
            {
                Metadata = new SubtitleMetadata { Title = title },
                Entries = entries
            };
        }

        public Task<SubtitleDocument> ReadAsync(Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Read(stream, encoding), cancellationToken);
        }

        private static TimeSpan ParseTime(string time)
        {
            // Supports both "00:00:20.000" (HH:MM:SS.mmm) and "00:20.000" (MM:SS.mmm)
            var parts = time.Split(':', '.');

            if (parts.Length == 4)
            {
                // HH:MM:SS.mmm format
                return new TimeSpan(
                    0, // days
                    int.Parse(parts[0]),  // hours
                    int.Parse(parts[1]),  // minutes
                    int.Parse(parts[2]),  // seconds
                    int.Parse(parts[3])   // milliseconds
                );
            }
            else if (parts.Length == 3)
            {
                // MM:SS.mmm format (no hours)
                return new TimeSpan(
                    0, // days
                    0, // hours
                    int.Parse(parts[0]),  // minutes
                    int.Parse(parts[1]),  // seconds
                    int.Parse(parts[2])   // milliseconds
                );
            }
            else
            {
                throw new FormatException($"Invalid VTT timestamp format: {time}");
            }
        }

        private static VttPosition? ParseCueSettings(string settings)
        {
            if (string.IsNullOrWhiteSpace(settings))
                return null;

            int? line = null;
            int? position = null;
            int? size = null;
            PositionAlignment? align = null;
            string? vertical = null;

            var parts = settings.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var kvp = part.Split(':');
                if (kvp.Length != 2) continue;

                var key = kvp[0].ToLowerInvariant();
                var value = kvp[1].TrimEnd('%');

                switch (key)
                {
                    case "line":
                        if (int.TryParse(value, out var lineVal))
                            line = lineVal;
                        break;
                    case "position":
                        if (int.TryParse(value, out var posVal))
                            position = posVal;
                        break;
                    case "size":
                        if (int.TryParse(value, out var sizeVal))
                            size = sizeVal;
                        break;
                    case "align":
                        align = value.ToLowerInvariant() switch
                        {
                            "start" or "left" => PositionAlignment.Start,
                            "center" or "middle" => PositionAlignment.Center,
                            "end" or "right" => PositionAlignment.End,
                            _ => null
                        };
                        break;
                    case "vertical":
                        vertical = value;
                        break;
                }
            }

            // Only create position if at least one setting was found
            if (line == null && position == null && size == null && align == null && vertical == null)
                return null;

            return new VttPosition
            {
                Line = line,
                Position = position,
                Size = size,
                HorizontalAlign = align,
                Vertical = vertical
            };
        }
    }
}