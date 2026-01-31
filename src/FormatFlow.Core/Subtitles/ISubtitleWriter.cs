using System.Text;

namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Writes domain model to subtitle file format
    /// </summary>
    public interface ISubtitleWriter
    {
        /// <summary>
        /// Supported format (e.g., "srt", "vtt")
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Write subtitle document to stream
        /// </summary>
        void Write(SubtitleDocument document, Stream stream, Encoding? encoding = null);

        /// <summary>
        /// Write subtitle document to stream asynchronously
        /// </summary>
        Task WriteAsync(SubtitleDocument document, Stream stream, Encoding? encoding = null, CancellationToken cancellationToken = default);
    }
}
