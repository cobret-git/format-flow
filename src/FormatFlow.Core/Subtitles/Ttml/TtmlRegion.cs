namespace FormatFlow.Core.Subtitles.Ttml
{
    /// <summary>
    /// TTML region positioning information.
    /// Represents a named region that defines where subtitles appear on screen.
    /// Multiple cues can reference the same region by ID.
    /// </summary>
    public record TtmlRegion : ISubtitlePosition
    {
        /// <summary>
        /// Region identifier for round-trip fidelity.
        /// When writing TTML, entries with the same RegionId share a single region definition.
        /// </summary>
        public string? RegionId { get; init; }

        /// <summary>
        /// Horizontal origin position (e.g., "10%", "100px").
        /// Corresponds to tts:origin X component.
        /// </summary>
        public string? OriginX { get; init; }

        /// <summary>
        /// Vertical origin position (e.g., "80%", "400px").
        /// Corresponds to tts:origin Y component.
        /// </summary>
        public string? OriginY { get; init; }

        /// <summary>
        /// Region width (e.g., "80%", "600px").
        /// Corresponds to tts:extent width component.
        /// </summary>
        public string? ExtentWidth { get; init; }

        /// <summary>
        /// Region height (e.g., "20%", "100px").
        /// Corresponds to tts:extent height component.
        /// </summary>
        public string? ExtentHeight { get; init; }

        /// <summary>
        /// Vertical alignment within the region (before/center/after).
        /// Corresponds to tts:displayAlign.
        /// </summary>
        public PositionAlignment? DisplayAlign { get; init; }

        /// <summary>
        /// Horizontal text alignment within the region.
        /// Corresponds to tts:textAlign.
        /// </summary>
        public PositionAlignment? HorizontalAlign { get; init; }
    }
}
