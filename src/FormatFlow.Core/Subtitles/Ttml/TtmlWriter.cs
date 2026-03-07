using System.Text;
using System.Xml.Linq;

namespace FormatFlow.Core.Subtitles.Ttml
{
    /// <summary>
    /// Writer for TTML (Timed Text Markup Language) subtitle files.
    /// Generates W3C compliant TTML with optional styling and regions.
    /// </summary>
    public class TtmlWriter : ISubtitleWriter
    {
        public string Format => "ttml";

        // TTML namespaces
        private static readonly XNamespace TtmlNs = "http://www.w3.org/ns/ttml";
        private static readonly XNamespace TtsNs = "http://www.w3.org/ns/ttml#styling";
        private static readonly XNamespace TtmNs = "http://www.w3.org/ns/ttml#metadata";

        public void Write(SubtitleDocument document, Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            var doc = BuildDocument(document);

            using var writer = new StreamWriter(stream, encoding, leaveOpen: true);
            doc.Save(writer);
            writer.Flush();
        }

        public Task WriteAsync(SubtitleDocument document, Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Write(document, stream, encoding), cancellationToken);
        }

        private XDocument BuildDocument(SubtitleDocument document)
        {
            // Collect unique regions and styles
            var regions = CollectRegions(document);
            var styles = CollectStyles(document);

            // Build root element with namespaces
            var tt = new XElement(TtmlNs + "tt",
                new XAttribute(XNamespace.Xmlns + "tts", TtsNs),
                new XAttribute(XNamespace.Xmlns + "ttm", TtmNs));

            // Add language if present
            if (!string.IsNullOrEmpty(document.Metadata.Language))
            {
                tt.Add(new XAttribute(XNamespace.Xml + "lang", document.Metadata.Language));
            }

            // Build head section
            var head = BuildHead(document.Metadata, regions, styles);
            if (head.HasElements)
            {
                tt.Add(head);
            }

            // Build body section
            var body = BuildBody(document.Entries);
            tt.Add(body);

            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                tt);
        }

        private static XElement BuildHead(
            SubtitleMetadata metadata,
            Dictionary<string, TtmlRegion> regions,
            Dictionary<string, TtmlStyle> styles)
        {
            var head = new XElement(TtmlNs + "head");

            // Add metadata if title is present
            if (!string.IsNullOrEmpty(metadata.Title))
            {
                var metadataElement = new XElement(TtmlNs + "metadata",
                    new XElement(TtmNs + "title", metadata.Title));
                head.Add(metadataElement);
            }

            // Add styling section if there are styles
            if (styles.Count > 0)
            {
                var styling = new XElement(TtmlNs + "styling");
                foreach (var kvp in styles)
                {
                    var styleElement = BuildStyleElement(kvp.Value);
                    styling.Add(styleElement);
                }
                head.Add(styling);
            }

            // Add layout section if there are regions
            if (regions.Count > 0)
            {
                var layout = new XElement(TtmlNs + "layout");
                foreach (var kvp in regions)
                {
                    var regionElement = BuildRegionElement(kvp.Value);
                    layout.Add(regionElement);
                }
                head.Add(layout);
            }

            return head;
        }

        private static XElement BuildStyleElement(TtmlStyle style)
        {
            var element = new XElement(TtmlNs + "style");

            if (!string.IsNullOrEmpty(style.StyleId))
                element.Add(new XAttribute(XNamespace.Xml + "id", style.StyleId));

            if (!string.IsNullOrEmpty(style.Color))
                element.Add(new XAttribute(TtsNs + "color", style.Color));

            if (!string.IsNullOrEmpty(style.BackgroundColor))
                element.Add(new XAttribute(TtsNs + "backgroundColor", style.BackgroundColor));

            if (!string.IsNullOrEmpty(style.FontFamily))
                element.Add(new XAttribute(TtsNs + "fontFamily", style.FontFamily));

            if (!string.IsNullOrEmpty(style.FontSize))
                element.Add(new XAttribute(TtsNs + "fontSize", style.FontSize));

            if (!string.IsNullOrEmpty(style.FontWeight))
                element.Add(new XAttribute(TtsNs + "fontWeight", style.FontWeight));

            if (!string.IsNullOrEmpty(style.FontStyle))
                element.Add(new XAttribute(TtsNs + "fontStyle", style.FontStyle));

            if (!string.IsNullOrEmpty(style.TextDecoration))
                element.Add(new XAttribute(TtsNs + "textDecoration", style.TextDecoration));

            if (!string.IsNullOrEmpty(style.Direction))
                element.Add(new XAttribute(TtsNs + "direction", style.Direction));

            if (!string.IsNullOrEmpty(style.WritingMode))
                element.Add(new XAttribute(TtsNs + "writingMode", style.WritingMode));

            if (!string.IsNullOrEmpty(style.Opacity))
                element.Add(new XAttribute(TtsNs + "opacity", style.Opacity));

            return element;
        }

        private static XElement BuildRegionElement(TtmlRegion region)
        {
            var element = new XElement(TtmlNs + "region");

            if (!string.IsNullOrEmpty(region.RegionId))
                element.Add(new XAttribute(XNamespace.Xml + "id", region.RegionId));

            // Combine origin components
            if (!string.IsNullOrEmpty(region.OriginX) || !string.IsNullOrEmpty(region.OriginY))
            {
                var originX = region.OriginX ?? "0%";
                var originY = region.OriginY ?? "0%";
                element.Add(new XAttribute(TtsNs + "origin", $"{originX} {originY}"));
            }

            // Combine extent components
            if (!string.IsNullOrEmpty(region.ExtentWidth) || !string.IsNullOrEmpty(region.ExtentHeight))
            {
                var extentWidth = region.ExtentWidth ?? "100%";
                var extentHeight = region.ExtentHeight ?? "100%";
                element.Add(new XAttribute(TtsNs + "extent", $"{extentWidth} {extentHeight}"));
            }

            if (region.DisplayAlign.HasValue)
            {
                var alignStr = region.DisplayAlign.Value switch
                {
                    PositionAlignment.Start => "before",
                    PositionAlignment.Center => "center",
                    PositionAlignment.End => "after",
                    _ => null
                };
                if (alignStr != null)
                    element.Add(new XAttribute(TtsNs + "displayAlign", alignStr));
            }

            if (region.HorizontalAlign.HasValue)
            {
                var alignStr = region.HorizontalAlign.Value switch
                {
                    PositionAlignment.Start => "start",
                    PositionAlignment.Center => "center",
                    PositionAlignment.End => "end",
                    _ => null
                };
                if (alignStr != null)
                    element.Add(new XAttribute(TtsNs + "textAlign", alignStr));
            }

            return element;
        }

        private static XElement BuildBody(IReadOnlyList<SubtitleEntry> entries)
        {
            var body = new XElement(TtmlNs + "body");
            var div = new XElement(TtmlNs + "div");

            foreach (var entry in entries)
            {
                var p = BuildParagraph(entry);
                div.Add(p);
            }

            body.Add(div);
            return body;
        }

        private static XElement BuildParagraph(SubtitleEntry entry)
        {
            var p = new XElement(TtmlNs + "p",
                new XAttribute("begin", FormatTime(entry.StartTime)),
                new XAttribute("end", FormatTime(entry.EndTime)));

            // Add region reference if present
            if (entry.Position is TtmlRegion region && !string.IsNullOrEmpty(region.RegionId))
            {
                p.Add(new XAttribute("region", region.RegionId));
            }

            // Add style reference if present
            if (entry.Style is TtmlStyle style && !string.IsNullOrEmpty(style.StyleId))
            {
                p.Add(new XAttribute("style", style.StyleId));
            }

            // Convert text content (handle line breaks and inline tags)
            var textContent = ConvertTextToTtml(entry.Text);
            foreach (var node in textContent)
            {
                p.Add(node);
            }

            return p;
        }

        private static IEnumerable<object> ConvertTextToTtml(string text)
        {
            var nodes = new List<object>();

            // Split by line breaks first
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    nodes.Add(new XElement(TtmlNs + "br"));
                }

                var line = lines[i];

                // For now, preserve inline tags as-is in text
                // A more sophisticated implementation would convert <b>, <i>, <u> to TTML spans
                nodes.Add(new XText(line));
            }

            return nodes;
        }

        private static string FormatTime(TimeSpan time)
        {
            // Use offset format with seconds for cleaner output
            var totalSeconds = time.TotalSeconds;
            return $"{totalSeconds:F3}s";
        }

        private static Dictionary<string, TtmlRegion> CollectRegions(SubtitleDocument document)
        {
            var regions = new Dictionary<string, TtmlRegion>();

            foreach (var entry in document.Entries)
            {
                if (entry.Position is TtmlRegion region && !string.IsNullOrEmpty(region.RegionId))
                {
                    if (!regions.ContainsKey(region.RegionId))
                    {
                        regions[region.RegionId] = region;
                    }
                }
            }

            return regions;
        }

        private static Dictionary<string, TtmlStyle> CollectStyles(SubtitleDocument document)
        {
            var styles = new Dictionary<string, TtmlStyle>();

            foreach (var entry in document.Entries)
            {
                if (entry.Style is TtmlStyle style && !string.IsNullOrEmpty(style.StyleId))
                {
                    if (!styles.ContainsKey(style.StyleId))
                    {
                        styles[style.StyleId] = style;
                    }
                }
            }

            return styles;
        }
    }
}