namespace FormatFlow.Core.Subtitles.Ttml
{
    /// <summary>
    /// TTML styling information.
    /// Represents the rich styling capabilities of TTML including typography,
    /// colors, and text layout properties.
    /// </summary>
    public record TtmlStyle : ISubtitleStyle
    {
        /// <summary>
        /// Style identifier for round-trip fidelity.
        /// When writing TTML, entries with the same StyleId share a single style definition.
        /// </summary>
        public string? StyleId { get; init; }

        /// <summary>
        /// Text foreground color (e.g., "white", "#FFFFFF", "rgba(255,255,255,1)").
        /// Corresponds to tts:color.
        /// </summary>
        public string? Color { get; init; }

        /// <summary>
        /// Background color behind text (e.g., "black", "#000000", "transparent").
        /// Corresponds to tts:backgroundColor.
        /// </summary>
        public string? BackgroundColor { get; init; }

        /// <summary>
        /// Font family name (e.g., "Arial", "monospace", "sansSerif").
        /// Corresponds to tts:fontFamily.
        /// </summary>
        public string? FontFamily { get; init; }

        /// <summary>
        /// Font size (e.g., "100%", "24px", "1.5em").
        /// Corresponds to tts:fontSize.
        /// </summary>
        public string? FontSize { get; init; }

        /// <summary>
        /// Font weight (e.g., "normal", "bold").
        /// Corresponds to tts:fontWeight.
        /// </summary>
        public string? FontWeight { get; init; }

        /// <summary>
        /// Font style (e.g., "normal", "italic", "oblique").
        /// Corresponds to tts:fontStyle.
        /// </summary>
        public string? FontStyle { get; init; }

        /// <summary>
        /// Text decoration (e.g., "none", "underline", "lineThrough").
        /// Corresponds to tts:textDecoration.
        /// </summary>
        public string? TextDecoration { get; init; }

        /// <summary>
        /// Text direction (e.g., "ltr", "rtl").
        /// Corresponds to tts:direction.
        /// </summary>
        public string? Direction { get; init; }

        /// <summary>
        /// Writing mode (e.g., "lrtb", "rltb", "tbrl").
        /// Corresponds to tts:writingMode.
        /// </summary>
        public string? WritingMode { get; init; }

        /// <summary>
        /// Element opacity (e.g., "1", "0.5").
        /// Corresponds to tts:opacity.
        /// </summary>
        public string? Opacity { get; init; }
    }
}