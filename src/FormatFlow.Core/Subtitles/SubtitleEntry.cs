namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// A single subtitle cue with timing, text, and optional format-specific features.
    /// </summary>
    public class SubtitleEntry
    {
        /// <summary>
        /// When the subtitle should appear
        /// </summary>
        public required TimeSpan StartTime { get; init; }

        /// <summary>
        /// When the subtitle should disappear
        /// </summary>
        public required TimeSpan EndTime { get; init; }

        /// <summary>
        /// The subtitle text content (may contain inline formatting tags)
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// Speaker identifier (VTT voice labels, TTML agents)
        /// </summary>
        public string? VoiceLabel { get; init; }

        /// <summary>
        /// Positioning information (format-specific: VttPosition, TtmlRegion, etc.)
        /// Null for formats without positioning support (SRT, SBV)
        /// </summary>
        public ISubtitlePosition? Position { get; init; }

        /// <summary>
        /// Style information (format-specific: InlineStyle, TtmlStyle, etc.)
        /// Null for formats without style metadata (SBV)
        /// Note: Inline tags in Text are separate from this metadata
        /// </summary>
        public ISubtitleStyle? Style { get; init; }
    }
}
