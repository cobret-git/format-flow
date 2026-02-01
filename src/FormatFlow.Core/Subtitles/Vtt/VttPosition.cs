namespace FormatFlow.Core.Subtitles.Vtt
{
    /// <summary>
    /// VTT cue positioning settings.
    /// Represents line, position, and alignment cue settings.
    /// </summary>
    public record VttPosition : ISubtitlePosition
    {
        /// <summary>
        /// Line position. Positive values count from top, negative from bottom.
        /// Can also be a percentage when followed by % in the file.
        /// </summary>
        public int? Line { get; init; }

        /// <summary>
        /// Horizontal position as percentage (0-100).
        /// </summary>
        public int? Position { get; init; }

        /// <summary>
        /// Text alignment within the cue box.
        /// </summary>
        public PositionAlignment? HorizontalAlign { get; init; }

        /// <summary>
        /// Size of the cue box as percentage (0-100).
        /// </summary>
        public int? Size { get; init; }

        /// <summary>
        /// Vertical text setting ("rl" for right-to-left, "lr" for left-to-right).
        /// Null means horizontal text (default).
        /// </summary>
        public string? Vertical { get; init; }
    }
}
