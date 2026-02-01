namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Base interface for format-specific positioning information.
    /// Different subtitle formats have varying positioning capabilities.
    /// </summary>
    public interface ISubtitlePosition
    {
        /// <summary>
        /// Horizontal text alignment - common concept across formats
        /// </summary>
        PositionAlignment? HorizontalAlign { get; }
    }
}
