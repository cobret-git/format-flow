namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Common inline styling that can be represented across multiple formats.
    /// Maps to HTML-like tags: &lt;b&gt;, &lt;i&gt;, &lt;u&gt;, &lt;font color="..."&gt;
    /// </summary>
    public record InlineStyle : ISubtitleStyle
    {
        /// <summary>
        /// Text color
        /// </summary>
        public string? Color { get; init; }

        /// <summary>
        /// Background color behind text
        /// </summary>
        public string? BackgroundColor { get; init; }

        /// <summary>
        /// Bold text (&lt;b&gt; tag)
        /// </summary>
        public bool Bold { get; init; }

        /// <summary>
        /// Italic text (&lt;i&gt; tag)
        /// </summary>
        public bool Italic { get; init; }

        /// <summary>
        /// Underlined text (&lt;u&gt; tag)
        /// </summary>
        public bool Underline { get; init; }
    }
}
