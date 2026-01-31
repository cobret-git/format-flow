namespace FormatFlow.Core.Subtitles
{
    public class SubtitleEntry
    {
        public required TimeSpan StartTime { get; init; }
        public required TimeSpan EndTime { get; init; }
        public required string Text { get; init; }

        // VTT-specific features (optional, ignored by SRT)
        public string? VoiceLabel { get; init; }
        public SubtitlePosition? Position { get; init; }
        public SubtitleStyle? Style { get; init; }
    }
}
