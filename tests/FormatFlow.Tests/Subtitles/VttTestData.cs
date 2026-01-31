using FormatFlow.Core.Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles
{
    public static class VttTestData
    {
        /// <summary>
        /// Basic valid VTT with 3 entries
        /// </summary>
        public const string BasicVtt = """
            WEBVTT

            1
            00:00:00.000 --> 00:00:02.500
            Welcome to the Example Subtitle File!

            2
            00:00:03.000 --> 00:00:06.000
            This is a demonstration of VTT subtitles.

            3
            00:00:07.000 --> 00:00:10.500
            You can use VTT files to add subtitles to your videos.
            """;

        /// <summary>
        /// VTT with title in header
        /// </summary>
        public const string VttWithTitle = """
            WEBVTT My Video Title

            1
            00:00:00.000 --> 00:00:02.000
            Hello world
            """;

        /// <summary>
        /// VTT with formatting tags (bold, italic, underline)
        /// </summary>
        public const string FormattedVtt = """
            WEBVTT

            1
            00:00:01.000 --> 00:00:04.000
            Welcome to our <b>tutorial</b> series.

            2
            00:00:04.500 --> 00:00:07.200
            Today we'll learn about <i>video localization</i>.

            3
            00:00:08.000 --> 00:00:11.300
            <font color="yellow">Let's get started</font> with the basics.

            4
            00:00:12.000 --> 00:00:15.000
            This text is <u>underlined</u> for emphasis.

            5
            00:00:16.000 --> 00:00:19.000
            [Dramatic music plays]
            """;

        /// <summary>
        /// VTT with position cue settings
        /// </summary>
        public const string PositionedVtt = """
            WEBVTT

            1
            00:00:00.000 --> 00:00:02.000 line:0 position:50% align:center
            Centered at top

            2
            00:00:03.000 --> 00:00:05.000 line:-1 align:end
            Bottom right aligned

            3
            00:00:06.000 --> 00:00:08.000 position:10% align:start
            Left positioned
            """;

        /// <summary>
        /// VTT with voice labels
        /// </summary>
        public const string VoiceLabelVtt = """
            WEBVTT

            1
            00:00:00.000 --> 00:00:02.000
            <v Alice>Hello, how are you?</v>

            2
            00:00:02.500 --> 00:00:04.500
            <v Bob>I'm doing great, thanks!</v>

            3
            00:00:05.000 --> 00:00:07.000
            <v Alice>That's wonderful to hear.</v>
            """;

        /// <summary>
        /// VTT with NOTE comments (should be skipped)
        /// </summary>
        public const string VttWithComments = """
            WEBVTT

            NOTE This is a comment
            that spans multiple lines

            1
            00:00:00.000 --> 00:00:02.000
            First subtitle

            NOTE Another comment

            2
            00:00:03.000 --> 00:00:05.000
            Second subtitle
            """;

        /// <summary>
        /// VTT with multi-line text
        /// </summary>
        public const string MultiLineVtt = """
            WEBVTT

            1
            00:00:00.000 --> 00:00:03.000
            This is the first line.
            This is the second line.
            This is the third line.
            """;

        /// <summary>
        /// VTT without cue identifiers (valid per spec)
        /// </summary>
        public const string NoCueIdVtt = """
            WEBVTT

            00:00:00.000 --> 00:00:02.000
            First subtitle without ID

            00:00:03.000 --> 00:00:05.000
            Second subtitle without ID
            """;

        /// <summary>
        /// Single entry VTT
        /// </summary>
        public const string SingleEntryVtt = """
            WEBVTT

            1
            00:00:00.000 --> 00:00:02.000
            Single subtitle entry
            """;

        /// <summary>
        /// Large timestamp values
        /// </summary>
        public const string LongDurationVtt = """
            WEBVTT

            1
            01:30:45.123 --> 01:30:50.999
            Movie ending credits
            """;

        /// <summary>
        /// Missing WEBVTT header (invalid)
        /// </summary>
        public const string InvalidMissingHeader = """
            1
            00:00:00.000 --> 00:00:02.000
            Missing header
            """;

        /// <summary>
        /// Malformed timestamp (invalid)
        /// </summary>
        public const string InvalidTimestamp = """
            WEBVTT

            1
            invalid timestamp format
            Some text
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
                    Text = "This is a demonstration of VTT subtitles."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(7),
                    EndTime = TimeSpan.FromMilliseconds(10500),
                    Text = "You can use VTT files to add subtitles to your videos."
                }
            }
        };

        public static SubtitleDocument CreateDocumentWithTitle() => new()
        {
            Metadata = new SubtitleMetadata { Title = "My Video Title" },
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Hello world"
                }
            }
        };

        public static SubtitleDocument CreateFormattedDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(1),
                    EndTime = TimeSpan.FromSeconds(4),
                    Text = "Welcome to our <b>tutorial</b> series."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromMilliseconds(4500),
                    EndTime = TimeSpan.FromMilliseconds(7200),
                    Text = "Today we'll learn about <i>video localization</i>."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(8),
                    EndTime = TimeSpan.FromMilliseconds(11300),
                    Text = "<font color=\"yellow\">Let's get started</font> with the basics."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(12),
                    EndTime = TimeSpan.FromSeconds(15),
                    Text = "This text is <u>underlined</u> for emphasis."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(16),
                    EndTime = TimeSpan.FromSeconds(19),
                    Text = "[Dramatic music plays]"
                }
            }
        };

        public static SubtitleDocument CreatePositionedDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Centered at top",
                    Position = new SubtitlePosition { Line = 0, Position = 50, Align = PositionAlignment.Center }
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(3),
                    EndTime = TimeSpan.FromSeconds(5),
                    Text = "Bottom right aligned",
                    Position = new SubtitlePosition { Line = -1, Align = PositionAlignment.End }
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(6),
                    EndTime = TimeSpan.FromSeconds(8),
                    Text = "Left positioned",
                    Position = new SubtitlePosition { Position = 10, Align = PositionAlignment.Start }
                }
            }
        };

        public static SubtitleDocument CreateVoiceLabelDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Hello, how are you?",
                    VoiceLabel = "Alice"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromMilliseconds(2500),
                    EndTime = TimeSpan.FromMilliseconds(4500),
                    Text = "I'm doing great, thanks!",
                    VoiceLabel = "Bob"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(5),
                    EndTime = TimeSpan.FromSeconds(7),
                    Text = "That's wonderful to hear.",
                    VoiceLabel = "Alice"
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
