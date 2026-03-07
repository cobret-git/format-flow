namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Document-level metadata for subtitle files.
    /// </summary>
    public class SubtitleMetadata
    {
        /// <summary>
        /// Document title (VTT header, TTML ttm:title).
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Language code (e.g., "en", "fr-CA").
        /// Corresponds to xml:lang in TTML.
        /// </summary>
        public string? Language { get; init; }
        public Dictionary<string, string> CustomProperties { get; init; } = new();
    }
}
