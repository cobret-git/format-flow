namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Base interface for format-specific styling information.
    /// Different subtitle formats have varying styling capabilities.
    /// </summary>
    public interface ISubtitleStyle
    {
        /// <summary>
        /// Text color (format varies: "white", "#FFFFFF", "rgba(255,255,255,1)")
        /// </summary>
        string? Color { get; }

        /// <summary>
        /// Background color behind text
        /// </summary>
        string? BackgroundColor { get; }
    }
}
