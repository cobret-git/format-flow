using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FormatFlow.Core.Subtitles.Ttml
{
    /// <summary>
    /// Reader for TTML (Timed Text Markup Language) subtitle files.
    /// Supports W3C TTML standard including Netflix/streaming service variants.
    /// </summary>
    public class TtmlReader : ISubtitleReader
    {
        public string Format => "ttml";

        // TTML namespaces
        private static readonly XNamespace TtmlNs = "http://www.w3.org/ns/ttml";
        private static readonly XNamespace TtsNs = "http://www.w3.org/ns/ttml#styling";
        private static readonly XNamespace TtmNs = "http://www.w3.org/ns/ttml#metadata";

        // Regex for parsing time expressions
        // Supports HH:MM:SS.mmm and MM:SS.mmm formats
        private static readonly Regex ClockTimeRegex = new(
            @"^(?:(\d{2,}):)?(\d{2}):(\d{2})(?:\.(\d+))?$",
            RegexOptions.Compiled);

        private static readonly Regex OffsetTimeRegex = new(
            @"^(\d+(?:\.\d+)?)(h|m|s|ms|f|t)$",
            RegexOptions.Compiled);

        public SubtitleDocument Read(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            XDocument doc;
            using (var reader = new StreamReader(stream, encoding, leaveOpen: true))
            {
                doc = XDocument.Load(reader);
            }

            var root = doc.Root;
            if (root == null || root.Name.LocalName != "tt")
            {
                throw new FormatException("Invalid TTML file: missing <tt> root element");
            }

            // Parse frame rate if present (needed for frame-based timing)
            var frameRate = ParseFrameRate(root);

            // Parse metadata and agents
            var (metadata, agents) = ParseMetadataAndAgents(root);

            // Parse regions from <layout>
            var regions = ParseRegions(root);

            // Parse styles from <styling>
            var styles = ParseStyles(root);

            // Parse body content
            var entries = ParseBody(root, regions, styles, agents, frameRate);

            return new SubtitleDocument
            {
                Metadata = metadata,
                Entries = entries
            };
        }

        public Task<SubtitleDocument> ReadAsync(Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Read(stream, encoding), cancellationToken);
        }

        private static (SubtitleMetadata Metadata, Dictionary<string, string> Agents) ParseMetadataAndAgents(XElement root)
        {
            var head = root.Element(TtmlNs + "head");
            var metadataElement = head?.Element(TtmlNs + "metadata");

            string? title = null;
            string? language = root.Attribute(XNamespace.Xml + "lang")?.Value;
            var agents = new Dictionary<string, string>();

            if (metadataElement != null)
            {
                title = metadataElement.Element(TtmNs + "title")?.Value;

                // Parse agents (ttm:agent elements define speakers)
                foreach (var agent in metadataElement.Elements(TtmNs + "agent"))
                {
                    var id = agent.Attribute(XNamespace.Xml + "id")?.Value;
                    var type = agent.Attribute("type")?.Value;
                    if (!string.IsNullOrEmpty(id))
                    {
                        // Use type as a label, or default to the id
                        agents[id] = type ?? id;
                    }
                }
            }

            var metadata = new SubtitleMetadata
            {
                Title = title,
                Language = language
            };

            return (metadata, agents);
        }

        private static Dictionary<string, TtmlRegion> ParseRegions(XElement root)
        {
            var regions = new Dictionary<string, TtmlRegion>();

            var layout = root.Element(TtmlNs + "head")?.Element(TtmlNs + "layout");
            if (layout == null)
                return regions;

            foreach (var region in layout.Elements(TtmlNs + "region"))
            {
                var id = region.Attribute(XNamespace.Xml + "id")?.Value;
                if (string.IsNullOrEmpty(id))
                    continue;

                var origin = region.Attribute(TtsNs + "origin")?.Value;
                var extent = region.Attribute(TtsNs + "extent")?.Value;

                string? originX = null, originY = null;
                if (!string.IsNullOrEmpty(origin))
                {
                    var parts = origin.Split(' ');
                    if (parts.Length >= 2)
                    {
                        originX = parts[0];
                        originY = parts[1];
                    }
                }

                string? extentWidth = null, extentHeight = null;
                if (!string.IsNullOrEmpty(extent))
                {
                    var parts = extent.Split(' ');
                    if (parts.Length >= 2)
                    {
                        extentWidth = parts[0];
                        extentHeight = parts[1];
                    }
                }

                var displayAlign = ParseAlignment(region.Attribute(TtsNs + "displayAlign")?.Value);
                var textAlign = ParseAlignment(region.Attribute(TtsNs + "textAlign")?.Value);

                regions[id] = new TtmlRegion
                {
                    RegionId = id,
                    OriginX = originX,
                    OriginY = originY,
                    ExtentWidth = extentWidth,
                    ExtentHeight = extentHeight,
                    DisplayAlign = displayAlign,
                    HorizontalAlign = textAlign
                };
            }

            return regions;
        }

        private static Dictionary<string, TtmlStyle> ParseStyles(XElement root)
        {
            var styles = new Dictionary<string, TtmlStyle>();

            var styling = root.Element(TtmlNs + "head")?.Element(TtmlNs + "styling");
            if (styling == null)
                return styles;

            foreach (var style in styling.Elements(TtmlNs + "style"))
            {
                var id = style.Attribute(XNamespace.Xml + "id")?.Value;
                if (string.IsNullOrEmpty(id))
                    continue;

                styles[id] = new TtmlStyle
                {
                    StyleId = id,
                    Color = style.Attribute(TtsNs + "color")?.Value,
                    BackgroundColor = style.Attribute(TtsNs + "backgroundColor")?.Value,
                    FontFamily = style.Attribute(TtsNs + "fontFamily")?.Value,
                    FontSize = style.Attribute(TtsNs + "fontSize")?.Value,
                    FontWeight = style.Attribute(TtsNs + "fontWeight")?.Value,
                    FontStyle = style.Attribute(TtsNs + "fontStyle")?.Value,
                    TextDecoration = style.Attribute(TtsNs + "textDecoration")?.Value,
                    Direction = style.Attribute(TtsNs + "direction")?.Value,
                    WritingMode = style.Attribute(TtsNs + "writingMode")?.Value,
                    Opacity = style.Attribute(TtsNs + "opacity")?.Value
                };
            }

            return styles;
        }

        private static List<SubtitleEntry> ParseBody(
            XElement root,
            Dictionary<string, TtmlRegion> regions,
            Dictionary<string, TtmlStyle> styles,
            Dictionary<string, string> agents,
            double frameRate)
        {
            var entries = new List<SubtitleEntry>();

            var body = root.Element(TtmlNs + "body");
            if (body == null)
                return entries;

            // Process all <p> elements in all <div> elements
            foreach (var div in body.Elements(TtmlNs + "div"))
            {
                foreach (var p in div.Elements(TtmlNs + "p"))
                {
                    var entry = ParseParagraph(p, regions, styles, agents, frameRate);
                    if (entry != null)
                        entries.Add(entry);
                }
            }

            // Also handle <p> directly under <body> (less common but valid)
            foreach (var p in body.Elements(TtmlNs + "p"))
            {
                var entry = ParseParagraph(p, regions, styles, agents, frameRate);
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private static SubtitleEntry? ParseParagraph(
            XElement p,
            Dictionary<string, TtmlRegion> regions,
            Dictionary<string, TtmlStyle> styles,
            Dictionary<string, string> agents,
            double frameRate)
        {
            var beginAttr = p.Attribute("begin")?.Value;
            var endAttr = p.Attribute("end")?.Value;
            var durAttr = p.Attribute("dur")?.Value;

            if (string.IsNullOrEmpty(beginAttr))
                return null;

            var startTime = ParseTime(beginAttr, frameRate);

            TimeSpan endTime;
            if (!string.IsNullOrEmpty(endAttr))
            {
                endTime = ParseTime(endAttr, frameRate);
            }
            else if (!string.IsNullOrEmpty(durAttr))
            {
                endTime = startTime + ParseTime(durAttr, frameRate);
            }
            else
            {
                return null; // No end time specified
            }

            // Extract text content (including nested spans, excluding romanization/translation)
            var text = ExtractText(p);

            // Resolve region reference
            TtmlRegion? region = null;
            var regionRef = p.Attribute("region")?.Value;
            if (!string.IsNullOrEmpty(regionRef) && regions.TryGetValue(regionRef, out var r))
            {
                region = r;
            }

            // Resolve style reference
            TtmlStyle? style = null;
            var styleRef = p.Attribute("style")?.Value;
            if (!string.IsNullOrEmpty(styleRef) && styles.TryGetValue(styleRef, out var s))
            {
                style = s;
            }

            // Resolve agent reference to voice label
            string? voiceLabel = null;
            var agentRef = p.Attribute(TtmNs + "agent")?.Value;
            if (!string.IsNullOrEmpty(agentRef) && agents.TryGetValue(agentRef, out var agentName))
            {
                voiceLabel = agentName;
            }

            return new SubtitleEntry
            {
                StartTime = startTime,
                EndTime = endTime,
                Text = text,
                VoiceLabel = voiceLabel,
                Position = region,
                Style = style
            };
        }

        private static string ExtractText(XElement element)
        {
            var sb = new StringBuilder();

            foreach (var node in element.Nodes())
            {
                if (node is XText text)
                {
                    sb.Append(text.Value);
                }
                else if (node is XElement child)
                {
                    var localName = child.Name.LocalName;

                    if (localName == "span")
                    {
                        // Check for special roles that should be skipped
                        var role = child.Attribute(TtmNs + "role")?.Value;
                        if (role == "x-roman" || role == "x-translation" || role == "x-bg")
                        {
                            // Skip romanization, translation, and background vocal spans
                            continue;
                        }

                        var fontWeight = child.Attribute(TtsNs + "fontWeight")?.Value;
                        var fontStyle = child.Attribute(TtsNs + "fontStyle")?.Value;
                        var textDecoration = child.Attribute(TtsNs + "textDecoration")?.Value;

                        var innerText = ExtractText(child);

                        // Skip empty spans (word-level timing spans with no text)
                        if (string.IsNullOrEmpty(innerText))
                            continue;

                        if (fontWeight == "bold")
                            innerText = $"<b>{innerText}</b>";
                        if (fontStyle == "italic")
                            innerText = $"<i>{innerText}</i>";
                        if (textDecoration == "underline")
                            innerText = $"<u>{innerText}</u>";

                        sb.Append(innerText);
                    }
                    else if (localName == "br")
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        // Recursively extract text from other elements
                        sb.Append(ExtractText(child));
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private static TimeSpan ParseTime(string timeExpr, double frameRate)
        {
            // Try clock-time format: HH:MM:SS.mmm or MM:SS.mmm
            var clockMatch = ClockTimeRegex.Match(timeExpr);
            if (clockMatch.Success)
            {
                // Group 1 is hours (optional), Group 2 is minutes, Group 3 is seconds
                var hours = clockMatch.Groups[1].Success ? int.Parse(clockMatch.Groups[1].Value) : 0;
                var minutes = int.Parse(clockMatch.Groups[2].Value);
                var seconds = int.Parse(clockMatch.Groups[3].Value);

                int milliseconds = 0;
                if (clockMatch.Groups[4].Success)
                {
                    var fraction = clockMatch.Groups[4].Value;
                    // Pad or truncate to 3 digits for milliseconds
                    if (fraction.Length > 3)
                        fraction = fraction.Substring(0, 3);
                    else
                        fraction = fraction.PadRight(3, '0');
                    milliseconds = int.Parse(fraction);
                }

                return new TimeSpan(0, hours, minutes, seconds, milliseconds);
            }

            // Try offset-time format: 10s, 500ms, 1.5h, 100f
            var offsetMatch = OffsetTimeRegex.Match(timeExpr);
            if (offsetMatch.Success)
            {
                var value = double.Parse(offsetMatch.Groups[1].Value);
                var unit = offsetMatch.Groups[2].Value;

                return unit switch
                {
                    "h" => TimeSpan.FromHours(value),
                    "m" => TimeSpan.FromMinutes(value),
                    "s" => TimeSpan.FromSeconds(value),
                    "ms" => TimeSpan.FromMilliseconds(value),
                    "f" => TimeSpan.FromSeconds(value / frameRate),
                    "t" => TimeSpan.FromTicks((long)value), // tick
                    _ => throw new FormatException($"Unknown time unit: {unit}")
                };
            }

            throw new FormatException($"Invalid TTML time expression: {timeExpr}");
        }

        private static double ParseFrameRate(XElement root)
        {
            // Look for ttp:frameRate attribute
            var ttpNs = XNamespace.Get("http://www.w3.org/ns/ttml#parameter");
            var frameRateAttr = root.Attribute(ttpNs + "frameRate")?.Value;

            if (!string.IsNullOrEmpty(frameRateAttr) && double.TryParse(frameRateAttr, out var fr))
                return fr;

            return 30.0; // Default frame rate
        }

        private static PositionAlignment? ParseAlignment(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "start" or "left" or "before" => PositionAlignment.Start,
                "center" or "middle" => PositionAlignment.Center,
                "end" or "right" or "after" => PositionAlignment.End,
                _ => null
            };
        }
    }
}