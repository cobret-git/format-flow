using FormatFlow.Core.Subtitles;
using System.Text;

namespace FormatFlow.Tests.Subtitles.Sbv
{
    public static class SbvTestData
    {
        /// <summary>
        /// Basic valid SBV with 3 entries
        /// </summary>
        public const string BasicSbv = """
            0:00:00.000,0:00:02.500
            Welcome to the Example Subtitle File!

            0:00:03.000,0:00:06.000
            This is a demonstration of SBV subtitles.

            0:00:07.000,0:00:10.500
            You can use SBV files to add subtitles to your videos.
            """;

        /// <summary>
        /// Multi-line subtitle text
        /// </summary>
        public const string MultiLineSbv = """
            0:00:00.000,0:00:03.000
            This is the first line.
            This is the second line.
            This is the third line.
            """;

        /// <summary>
        /// Single entry SBV
        /// </summary>
        public const string SingleEntrySbv = """
            0:00:00.000,0:00:02.000
            Single subtitle entry
            """;

        /// <summary>
        /// Large timestamp values (double-digit hours)
        /// </summary>
        public const string LongDurationSbv = """
            1:30:45.123,1:30:50.999
            Movie ending credits
            """;

        /// <summary>
        /// Double-digit hour format (both formats are valid)
        /// </summary>
        public const string DoubleDigitHourSbv = """
            00:00:00.000,00:00:02.000
            First subtitle

            01:30:45.000,01:30:50.000
            Later subtitle
            """;

        /// <summary>
        /// Text containing commas (should not conflict with timestamp parsing)
        /// </summary>
        public const string TextWithCommasSbv = """
            0:00:00.000,0:00:03.000
            Hello, how are you?

            0:00:04.000,0:00:07.000
            Fine, thanks, and you?
            """;

        /// <summary>
        /// Malformed timestamp (invalid)
        /// </summary>
        public const string InvalidTimestamp = """
            invalid timestamp format
            Some text
            """;

        /// <summary>
        /// Malformed timestamp separator (uses --> instead of ,)
        /// </summary>
        public const string WrongSeparator = """
            0:00:00.000 --> 0:00:02.000
            Wrong separator
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

        // Helper for comparing content (ignores trailing whitespace)
        public static string Normalize(string content) => content.TrimEnd();

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
                    Text = "This is a demonstration of SBV subtitles."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(7),
                    EndTime = TimeSpan.FromMilliseconds(10500),
                    Text = "You can use SBV files to add subtitles to your videos."
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

        public static SubtitleDocument CreateTextWithCommasDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(3),
                    Text = "Hello, how are you?"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(4),
                    EndTime = TimeSpan.FromSeconds(7),
                    Text = "Fine, thanks, and you?"
                }
            }
        };

        public static SubtitleDocument CreateEmptyDocument() => new()
        {
            Entries = Array.Empty<SubtitleEntry>()
        };
    }
}
