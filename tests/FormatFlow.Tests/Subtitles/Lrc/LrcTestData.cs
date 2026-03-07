using FormatFlow.Core.Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles.Lrc
{
    /// <summary>
    /// Reusable test fixtures and factory helpers for LRC reader/writer tests.
    /// Mirrors the patterns used by SrtTestData, VttTestData, etc.
    /// </summary>
    internal static class LrcTestData
    {
        // ── Raw LRC strings ───────────────────────────────────────────────────

        /// <summary>A minimal valid LRC file with header tags and two timed lines.</summary>
        public const string SimpleWithHeaders = """
            [ti:{496BDFC8-45CB-4264-9187-D9930B04955D}]
            [ar:{3723F51C-A90D-4F3E-8D92-CF94E7A54B3F}]
            [al:{15EF5C4E-5ABB-4A80-A721-30000DF7752A}]
            [offset:0]

            [00:07.29]{5CCB93E3-40E5-4DB7-B9AD-B1E5BA446F6E}
            [00:09.34]{8D0F37A7-47F3-4502-9BF2-FCA11787E001}
            """;

        /// <summary>A single timed line with no header section.</summary>
        public const string SingleLine = "[00:05.00]Hello world\n";

        /// <summary>One line with three timestamps — should expand to three entries.</summary>
        public const string MultiTimestampLine =
            "[01:20.88][01:26.66][01:32.65]{73C88ACC-98A7-44FE-9D91-C9756A27FF20}\n";

        /// <summary>File with a positive offset (+500 ms).</summary>
        public const string WithPositiveOffset = """
            [offset:500]
            [00:10.00]Late line
            """;

        /// <summary>File with a negative offset that clamps one entry to 00:00.00.</summary>
        public const string WithNegativeOffset = """
            [offset:-5000]
            [00:02.00]Early line
            [00:10.00]Normal line
            """;

        /// <summary>File with a blank-text entry (should be skipped by writer).</summary>
        public const string WithBlankEntry = """
            [00:05.00]First line
            [00:10.00] 
            [00:15.00]Third line
            """;

        /// <summary>File using 3-digit millisecond fractions (extended LRC).</summary>
        public const string WithMillisecondTimestamps =
            "[00:05.250]Extended timestamp line\n";

        /// <summary>A complete resembling the samples.</summary>
        public const string Realistic = """
            [ti:{496BDFC8-45CB-4264-9187-D9930B04955D}]
            [ar:{3723F51C-A90D-4F3E-8D92-CF94E7A54B3F}]
            [al:{15EF5C4E-5ABB-4A80-A721-30000DF7752A}]
            [offset:0]

            [00:07.29][00:54.17]{5CCB93E3-40E5-4DB7-B9AD-B1E5BA446F6E}
            [00:09.34][00:56.07]{8D0F37A7-47F3-4502-9BF2-FCA11787E001}
            [00:11.12][00:58.09]{3F916B64-1DAD-4873-B122-A6F26B440392}
            """;

        // ── Stream helpers ────────────────────────────────────────────────────

        public static Stream ToStream(string content, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return new MemoryStream(encoding.GetBytes(content));
        }

        public static string StreamToString(MemoryStream stream)
        {
            stream.Position = 0;
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        // ── SubtitleDocument factories ────────────────────────────────────────

        public static SubtitleDocument CreateBasicDocument() => new()
        {
            Metadata = new SubtitleMetadata { Title = "Test Song" },
            Entries =
            [
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(7),
                    EndTime   = TimeSpan.FromSeconds(9),
                    Text      = "{5CCB93E3-40E5-4DB7-B9AD-B1E5BA446F6E}"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(9),
                    EndTime   = TimeSpan.FromSeconds(11),
                    Text      = "{8D0F37A7-47F3-4502-9BF2-FCA11787E001}"
                }
            ]
        };

        public static SubtitleDocument CreateDocumentWithCustomProperties() => new()
        {
            Metadata = new SubtitleMetadata
            {
                Title = "{496BDFC8-45CB-4264-9187-D9930B04955D}",
                CustomProperties = new Dictionary<string, string>
                {
                    ["ar"] = "{3723F51C-A90D-4F3E-8D92-CF94E7A54B3F}",
                    ["al"] = "{15EF5C4E-5ABB-4A80-A721-30000DF7752A}"
                }
            },
            Entries =
            [
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(7),
                    EndTime   = TimeSpan.FromSeconds(9),
                    Text      = "{5CCB93E3-40E5-4DB7-B9AD-B1E5BA446F6E}"
                }
            ]
        };

        public static SubtitleDocument CreateDocumentWithBlankEntries() => new()
        {
            Metadata = new SubtitleMetadata(),
            Entries =
            [
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(5),
                    EndTime   = TimeSpan.FromSeconds(10),
                    Text      = "First line"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(10),
                    EndTime   = TimeSpan.FromSeconds(15),
                    Text      = ""                          // blank — must be skipped by writer
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(15),
                    EndTime   = TimeSpan.FromSeconds(20),
                    Text      = "Third line"
                }
            ]
        };

        public static SubtitleDocument CreateEmptyDocument() => new()
        {
            Metadata = new SubtitleMetadata(),
            Entries = Array.Empty<SubtitleEntry>()
        };

        public static SubtitleDocument CreateLongDurationDocument() => new()
        {
            Metadata = new SubtitleMetadata(),
            Entries =
            [
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromMinutes(125) + TimeSpan.FromSeconds(30),
                    EndTime   = TimeSpan.FromMinutes(125) + TimeSpan.FromSeconds(35),
                    Text      = "Long track line"
                }
            ]
        };
    }
}
