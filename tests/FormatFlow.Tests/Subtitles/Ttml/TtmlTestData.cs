using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Ttml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles.Ttml
{
    public static class TtmlTestData
    {
        /// <summary>
        /// Basic valid TTML with 3 entries using offset time format
        /// </summary>
        public const string BasicTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <head></head>
            <body>
            <div>
            <p begin="0s" end="2.5s">Lorem ipsum dolor sit amet.</p>
            <p begin="3s" end="6s">Consectetur adipiscing elit sed do.</p>
            <p begin="7s" end="10.5s">Eiusmod tempor incididunt ut labore.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with clock time format (HH:MM:SS.mmm)
        /// </summary>
        public const string ClockTimeTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            <p begin="00:00:01.500" end="00:00:04.200">First entry with clock time.</p>
            <p begin="00:01:30.000" end="00:01:35.500">Entry at one minute thirty.</p>
            <p begin="01:00:00.000" end="01:00:05.000">Entry at one hour mark.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with short clock time format (MM:SS.mmm) - Apple Music style
        /// </summary>
        public const string ShortClockTimeTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            <p begin="00:17.292" end="00:21.042">Alfa bravo charlie delta echo.</p>
            <p begin="02:02.359" end="02:05.968">Foxtrot golf hotel india juliet.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with title and language metadata
        /// </summary>
        public const string TtmlWithMetadata = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" 
                xmlns:ttm="http://www.w3.org/ns/ttml#metadata"
                xml:lang="en-US">
            <head>
            <metadata>
            <ttm:title>Sample Subtitle Document</ttm:title>
            </metadata>
            </head>
            <body>
            <div>
            <p begin="0s" end="2s">Entry with metadata present.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with styling (bold, italic, underline)
        /// </summary>
        public const string StyledTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling">
            <head>
            <styling>
            <style xml:id="bold" tts:fontWeight="bold"/>
            <style xml:id="italic" tts:fontStyle="italic"/>
            <style xml:id="highlight" tts:color="#FFFF00" tts:backgroundColor="#000000"/>
            </styling>
            </head>
            <body>
            <div>
            <p begin="1s" end="4s">Welcome to our <span tts:fontWeight="bold">tutorial</span> series.</p>
            <p begin="4.5s" end="7.2s">Today we learn about <span tts:fontStyle="italic">video localization</span>.</p>
            <p begin="8s" end="11.3s">This is <span tts:textDecoration="underline">underlined</span> text.</p>
            <p begin="12s" end="15s" style="highlight">Highlighted entry.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with regions (positioning)
        /// </summary>
        public const string RegionedTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling">
            <head>
            <layout>
            <region xml:id="top" tts:origin="10% 10%" tts:extent="80% 20%" tts:displayAlign="before" tts:textAlign="center"/>
            <region xml:id="bottom" tts:origin="10% 80%" tts:extent="80% 20%" tts:displayAlign="after" tts:textAlign="center"/>
            <region xml:id="left" tts:origin="5% 40%" tts:extent="40% 20%" tts:textAlign="start"/>
            </layout>
            </head>
            <body>
            <div>
            <p begin="0s" end="2s" region="top">Positioned at top center.</p>
            <p begin="3s" end="5s" region="bottom">Positioned at bottom center.</p>
            <p begin="6s" end="8s" region="left">Positioned at left.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with agents (voice labels) - Apple Music style
        /// </summary>
        public const string AgentTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:ttm="http://www.w3.org/ns/ttml#metadata">
            <head>
            <metadata>
            <ttm:agent type="person" xml:id="v1"/>
            <ttm:agent type="other" xml:id="v2"/>
            </metadata>
            </head>
            <body>
            <div>
            <p begin="0s" end="2s" ttm:agent="v1">Speaker one says hello.</p>
            <p begin="2.5s" end="4.5s" ttm:agent="v2">Speaker two responds.</p>
            <p begin="5s" end="7s" ttm:agent="v1">Speaker one continues.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with word-level timing spans (karaoke style) - Apple Music lyrics
        /// </summary>
        public const string WordLevelTimingTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:ttm="http://www.w3.org/ns/ttml#metadata">
            <head>
            <metadata>
            <ttm:agent type="person" xml:id="v1"/>
            </metadata>
            </head>
            <body>
            <div>
            <p begin="00:17.292" end="00:21.042" ttm:agent="v1">
            <span begin="00:17.292" end="00:17.998">Al</span>
            <span begin="00:17.998" end="00:18.304">fa</span>
            <span begin="00:18.304" end="00:18.705">bra</span>
            <span begin="00:18.705" end="00:18.951">vo</span>
            <span begin="00:18.951" end="00:19.331">char</span>
            <span begin="00:19.331" end="00:19.587">lie</span>
            <span begin="00:19.587" end="00:20.034">del</span>
            <span begin="00:20.034" end="00:20.231">ta</span>
            <span begin="00:20.231" end="00:21.042">echo</span>
            </p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with romanization spans (should be excluded from text)
        /// </summary>
        public const string RomanizationTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:ttm="http://www.w3.org/ns/ttml#metadata">
            <body>
            <div>
            <p begin="00:01.737" end="00:06.722">
            <span begin="00:01.737" end="00:02.175">AB</span>
            <span begin="00:02.175" end="00:02.592">CD</span>
            <span begin="00:02.592" end="00:02.896">EF</span>
            <span ttm:role="x-roman">a b c d e f</span>
            </p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with translation spans (should be excluded from text)
        /// </summary>
        public const string TranslationTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:ttm="http://www.w3.org/ns/ttml#metadata">
            <body>
            <div>
            <p begin="00:01.737" end="00:06.722">
            <span begin="00:01.737" end="00:02.500">Bonjour</span>
            <span begin="00:02.500" end="00:03.500">monde</span>
            <span ttm:role="x-translation" xml:lang="en">Hello world</span>
            <span ttm:role="x-roman">bon zhoor mond</span>
            </p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with duration attribute instead of end
        /// </summary>
        public const string DurationTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            <p begin="0s" dur="2.5s">Entry using duration attribute.</p>
            <p begin="3s" dur="3s">Another with duration.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with multiple divs
        /// </summary>
        public const string MultipleDivTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div begin="0s" end="30s">
            <p begin="0s" end="2s">First div, entry one.</p>
            <p begin="3s" end="5s">First div, entry two.</p>
            </div>
            <div begin="30s" end="60s">
            <p begin="30s" end="32s">Second div, entry one.</p>
            <p begin="33s" end="35s">Second div, entry two.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with line breaks
        /// </summary>
        public const string LineBreakTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            <p begin="0s" end="3s">Line one<br/>Line two<br/>Line three</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// TTML with various time units (ms, h, f)
        /// </summary>
        public const string MixedTimeUnitsTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" ttp:frameRate="30">
            <body>
            <div>
            <p begin="500ms" end="2500ms">Entry in milliseconds.</p>
            <p begin="3s" end="6s">Entry in seconds.</p>
            <p begin="90f" end="180f">Entry in frames (3s to 6s at 30fps).</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// Single entry TTML
        /// </summary>
        public const string SingleEntryTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            <p begin="0s" end="2s">Single subtitle entry only.</p>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// Empty TTML (valid structure, no entries)
        /// </summary>
        public const string EmptyTtml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            </div>
            </body>
            </tt>
            """;

        /// <summary>
        /// Invalid TTML - missing tt root element
        /// </summary>
        public const string InvalidMissingRoot = """
            <?xml version="1.0" encoding="UTF-8"?>
            <body>
            <div>
            <p begin="0s" end="2s">Invalid structure.</p>
            </div>
            </body>
            """;

        /// <summary>
        /// Invalid TTML - malformed XML
        /// </summary>
        public const string InvalidMalformedXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml">
            <body>
            <div>
            <p begin="0s" end="2s">Unclosed tag
            </div>
            </body>
            </tt>
            """;

        // ===== Helper Methods =====

        /// <summary>
        /// Converts string content to a readable stream.
        /// </summary>
        public static Stream ToStream(string content) =>
            new MemoryStream(Encoding.UTF8.GetBytes(content));

        /// <summary>
        /// Converts a stream to string content.
        /// </summary>
        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Normalizes content by trimming trailing whitespace.
        /// </summary>
        public static string Normalize(string content) => content.TrimEnd();

        // ===== Document Factory Methods =====

        /// <summary>
        /// Creates a basic SubtitleDocument with 3 entries.
        /// </summary>
        public static SubtitleDocument CreateBasicDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromMilliseconds(2500),
                    Text = "Lorem ipsum dolor sit amet."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(3),
                    EndTime = TimeSpan.FromSeconds(6),
                    Text = "Consectetur adipiscing elit sed do."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(7),
                    EndTime = TimeSpan.FromMilliseconds(10500),
                    Text = "Eiusmod tempor incididunt ut labore."
                }
            }
        };

        /// <summary>
        /// Creates a document with metadata (title and language).
        /// </summary>
        public static SubtitleDocument CreateDocumentWithMetadata() => new()
        {
            Metadata = new SubtitleMetadata
            {
                Title = "Sample Subtitle Document",
                Language = "en-US"
            },
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Entry with metadata present."
                }
            }
        };

        /// <summary>
        /// Creates a document with TTML regions for positioning.
        /// </summary>
        public static SubtitleDocument CreateDocumentWithRegions() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Positioned at top center.",
                    Position = new TtmlRegion
                    {
                        RegionId = "top",
                        OriginX = "10%",
                        OriginY = "10%",
                        ExtentWidth = "80%",
                        ExtentHeight = "20%",
                        DisplayAlign = PositionAlignment.Start,
                        HorizontalAlign = PositionAlignment.Center
                    }
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(3),
                    EndTime = TimeSpan.FromSeconds(5),
                    Text = "Positioned at bottom center.",
                    Position = new TtmlRegion
                    {
                        RegionId = "bottom",
                        OriginX = "10%",
                        OriginY = "80%",
                        ExtentWidth = "80%",
                        ExtentHeight = "20%",
                        DisplayAlign = PositionAlignment.End,
                        HorizontalAlign = PositionAlignment.Center
                    }
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(6),
                    EndTime = TimeSpan.FromSeconds(8),
                    Text = "Positioned at left.",
                    Position = new TtmlRegion
                    {
                        RegionId = "left",
                        OriginX = "5%",
                        OriginY = "40%",
                        ExtentWidth = "40%",
                        ExtentHeight = "20%",
                        HorizontalAlign = PositionAlignment.Start
                    }
                }
            }
        };

        /// <summary>
        /// Creates a document with TTML styles.
        /// </summary>
        public static SubtitleDocument CreateDocumentWithStyles() => new()
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
                    Text = "Today we learn about <i>video localization</i>."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(8),
                    EndTime = TimeSpan.FromMilliseconds(11300),
                    Text = "This is <u>underlined</u> text."
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(12),
                    EndTime = TimeSpan.FromSeconds(15),
                    Text = "Highlighted entry.",
                    Style = new TtmlStyle
                    {
                        StyleId = "highlight",
                        Color = "#FFFF00",
                        BackgroundColor = "#000000"
                    }
                }
            }
        };

        /// <summary>
        /// Creates a document with voice labels (agents).
        /// </summary>
        public static SubtitleDocument CreateDocumentWithVoiceLabels() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Speaker one says hello.",
                    VoiceLabel = "person"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromMilliseconds(2500),
                    EndTime = TimeSpan.FromMilliseconds(4500),
                    Text = "Speaker two responds.",
                    VoiceLabel = "other"
                },
                new SubtitleEntry
                {
                    StartTime = TimeSpan.FromSeconds(5),
                    EndTime = TimeSpan.FromSeconds(7),
                    Text = "Speaker one continues.",
                    VoiceLabel = "person"
                }
            }
        };

        /// <summary>
        /// Creates a document with multi-line text.
        /// </summary>
        public static SubtitleDocument CreateMultiLineDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(3),
                    Text = "Line one\r\nLine two\r\nLine three"
                }
            }
        };

        /// <summary>
        /// Creates a single entry document.
        /// </summary>
        public static SubtitleDocument CreateSingleEntryDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromSeconds(2),
                    Text = "Single subtitle entry only."
                }
            }
        };

        /// <summary>
        /// Creates a document with long duration timestamps.
        /// </summary>
        public static SubtitleDocument CreateLongDurationDocument() => new()
        {
            Entries = new[]
            {
                new SubtitleEntry
                {
                    StartTime = new TimeSpan(0, 1, 30, 45, 123),
                    EndTime = new TimeSpan(0, 1, 30, 50, 999),
                    Text = "Entry at movie ending credits."
                }
            }
        };

        /// <summary>
        /// Creates an empty document.
        /// </summary>
        public static SubtitleDocument CreateEmptyDocument() => new()
        {
            Entries = Array.Empty<SubtitleEntry>()
        };
    }
}
