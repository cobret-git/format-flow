namespace FormatFlow.Core.Subtitles
{
    public class SubtitleDocument
    {
        public SubtitleMetadata Metadata { get; init; } = new();
        public IReadOnlyList<SubtitleEntry> Entries { get; init; } = Array.Empty<SubtitleEntry>();
    }
}
