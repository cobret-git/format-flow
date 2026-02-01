using FormatFlow.Core.Subtitles;
using System.Text;

namespace FormatFlow.Tests.Subtitles.Srt
{
    public static class SrtTestData
    {
        /// <summary>
        /// Basic valid SRT with 3 entries
        /// </summary>
        public const string BasicSrt = """
        1
        00:00:00,000 --> 00:00:02,500
        Welcome to the Example Subtitle File!

        2
        00:00:03,000 --> 00:00:06,000
        This is a demonstration of SRT subtitles.

        3
        00:00:07,000 --> 00:00:10,500
        You can use SRT files to add subtitles to your videos.

        """;

        /// <summary>
        /// Multi-line subtitle text
        /// </summary>
        public const string MultiLineSrt = """
        1
        00:00:00,000 --> 00:00:03,000
        This is the first line.
        This is the second line.
        This is the third line.

        """;

        /// <summary>
        /// Single entry SRT
        /// </summary>
        public const string SingleEntrySrt = """
        1
        00:00:00,000 --> 00:00:02,000
        Single subtitle entry

        """;

        /// <summary>
        /// Edge case: empty subtitle text
        /// </summary>
        public const string EmptySrt = """
        1
        00:00:00,000 --> 00:00:02,000

        """;

        /// <summary>
        /// Timing edge case: zero duration
        /// </summary>
        public const string ZeroDurationSrt = """
        1
        00:00:00,000 --> 00:00:00,000
        Instant subtitle

        """;

        /// <summary>
        /// Large timestamp values
        /// </summary>
        public const string LongDurationSrt = """
        1
        01:30:45,123 --> 01:30:50,999
        Movie ending credits

        """;

        // Helper to convert string to stream
        public static Stream ToStream(string content) =>
            new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Helper to convert stream to string
        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        // Create sample SubtitleDocument for testing writers
        public static SubtitleDocument CreateBasicDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromMilliseconds(2500),
                    Text = "Welcome to the Example Subtitle File!"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(3),
                    EndTime = TimeSpan.FromSeconds(6),
                    Text = "This is a demonstration of SRT subtitles."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(7),
                    EndTime = TimeSpan.FromMilliseconds(10500),
                    Text = "You can use SRT files to add subtitles to your videos."
                }
            }
        };

        public static SubtitleDocument CreateMultiLineDocument() => new()
        {
            Entries = new[]
            {
                 new SubtitleEntry
                 {
                     StartTime = TimeSpan.Zero,
                     EndTime = TimeSpan.FromSeconds(3),
                     Text = "This is the first line.\r\nThis is the second line.\r\nThis is the third line."
                 }
             }
        };

        public static SubtitleDocument CreateSingleEntryDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Single subtitle entry"
                }
            }
        };

        public static SubtitleDocument CreateLongDurationDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = new TimeSpan(0, 1, 30, 45, 123),
                    EndTime = new TimeSpan(0, 1, 30, 50, 999),
                    Text = "Movie ending credits"
                }
            }
        };

        public static SubtitleDocument CreateEmptyDocument() => new()
        {
            Entries = Array.Empty<SubtitleEntry>()
        };
    }
}
