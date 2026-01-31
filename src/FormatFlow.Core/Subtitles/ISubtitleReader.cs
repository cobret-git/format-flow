using System.Text;

namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Reads subtitle file from stream into domain model
    /// </summary>
    public interface ISubtitleReader
    {
        /// <summary>
        /// Supported format (e.g., "srt", "vtt")
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Parse subtitle file from stream
        /// </summary>
        SubtitleDocument Read(Stream stream, Encoding? encoding = null);

        /// <summary>
        /// Parse subtitle file from stream asynchronously
        /// </summary>
        Task<SubtitleDocument> ReadAsync(Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default);
    }
}
