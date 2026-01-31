namespace FormatFlow.Core.Subtitles
{
    public class SubtitleMetadata
    {
        public string? Title { get; init; }
        public string? Language { get; init; }
        public Dictionary<string, string> CustomProperties { get; init; } = new();
    }
}
