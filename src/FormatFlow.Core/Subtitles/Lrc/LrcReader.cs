using System.Text;
using System.Text.RegularExpressions;

namespace FormatFlow.Core.Subtitles.Lrc
{
    public class LrcReader : ISubtitleReader
    {
        public string Format => "lrc";

        // Matches LRC header tags: [ti:Title], [ar:Artist], [al:Album], [offset:0]
        // Deliberately excludes lines that also contain timestamp tags.
        private static readonly Regex HeaderTagRegex = new(
            @"^\[([a-zA-Z][a-zA-Z0-9]*):(.*)\]$",
            RegexOptions.Compiled);

        // Matches one timestamp tag: [MM:SS.xx] (centiseconds) or [MM:SS.xxx] (milliseconds)
        private static readonly Regex TimestampTagRegex = new(
            @"\[(\d{1,3}):(\d{2})\.(\d{2,3})\]",
            RegexOptions.Compiled);

        /// <summary>
        /// Fallback duration assigned to the last subtitle entry, which has no successor
        /// from which to infer an end time.
        /// </summary>
        private static readonly TimeSpan DefaultLastEntryDuration = TimeSpan.FromSeconds(5);

        public SubtitleDocument Read(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(stream, encoding, leaveOpen: true);

            string? title = null;
            int offsetMs = 0;
            var customProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var timedLines = new List<(TimeSpan StartTime, string Text)>();

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // A header tag is a line whose entire content is a single bracketed tag with
                // no timestamp pattern. Check timestamps first to avoid misclassifying lines
                // like [01:23.45]text as a header.
                var timestampMatches = TimestampTagRegex.Matches(trimmed);
                if (timestampMatches.Count == 0)
                {
                    var headerMatch = HeaderTagRegex.Match(trimmed);
                    if (headerMatch.Success)
                    {
                        var tag = headerMatch.Groups[1].Value.ToLowerInvariant();
                        var value = headerMatch.Groups[2].Value.Trim();
                        HandleHeaderTag(tag, value, ref title, ref offsetMs, customProperties);
                    }
                    // Lines that match neither pattern are silently ignored (comments, unknown tags).
                    continue;
                }

                // Text is everything after the last timestamp tag on the line.
                var lastTs = timestampMatches[timestampMatches.Count - 1];
                var text = trimmed.Substring(lastTs.Index + lastTs.Length).Trim();

                // One timed entry per timestamp (multi-timestamp lines expand here).
                foreach (Match ts in timestampMatches)
                {
                    timedLines.Add((ParseTime(ts, offsetMs), text));
                }
            }

            // LRC files have no inherent ordering guarantee; sort before inferring end times.
            timedLines.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

            var entries = BuildEntries(timedLines);

            return new SubtitleDocument
            {
                Metadata = new SubtitleMetadata
                {
                    Title = title,
                    CustomProperties = customProperties
                },
                Entries = entries
            };
        }

        public Task<SubtitleDocument> ReadAsync(
            Stream stream,
            Encoding? encoding = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Read(stream, encoding), cancellationToken);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void HandleHeaderTag(
            string tag,
            string value,
            ref string? title,
            ref int offsetMs,
            Dictionary<string, string> customProperties)
        {
            switch (tag)
            {
                case "ti":
                    title = value;
                    break;
                case "offset":
                    // offset is in milliseconds; ignore parse failures gracefully.
                    int.TryParse(value, out offsetMs);
                    break;
                default:
                    // Preserve ar, al, and any other tags as custom properties.
                    customProperties[tag] = value;
                    break;
            }
        }

        private static List<SubtitleEntry> BuildEntries(
            List<(TimeSpan StartTime, string Text)> timedLines)
        {
            var entries = new List<SubtitleEntry>(timedLines.Count);

            for (int i = 0; i < timedLines.Count; i++)
            {
                var (startTime, text) = timedLines[i];

                // End time is the start of the next entry, or a fixed fallback for the last.
                var endTime = i + 1 < timedLines.Count
                    ? timedLines[i + 1].StartTime
                    : startTime + DefaultLastEntryDuration;

                // Guard: end must not precede start (can happen with duplicate timestamps).
                if (endTime < startTime)
                    endTime = startTime + DefaultLastEntryDuration;

                entries.Add(new SubtitleEntry
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = text
                });
            }

            return entries;
        }

        private static TimeSpan ParseTime(Match timestampMatch, int offsetMs)
        {
            var minutes = int.Parse(timestampMatch.Groups[1].Value);
            var seconds = int.Parse(timestampMatch.Groups[2].Value);
            var fractionStr = timestampMatch.Groups[3].Value;

            // LRC traditionally uses centiseconds (2 digits); extended LRC uses milliseconds (3).
            int milliseconds = fractionStr.Length == 2
                ? int.Parse(fractionStr) * 10   // centiseconds → milliseconds
                : int.Parse(fractionStr);        // already milliseconds

            var totalMs = (minutes * 60_000L) + (seconds * 1_000L) + milliseconds + offsetMs;
            if (totalMs < 0) totalMs = 0;

            return TimeSpan.FromMilliseconds(totalMs);
        }
    }
}
