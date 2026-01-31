namespace FormatFlow.Core.Subtitles
{
    public interface ISubtitleConverter
    {
        /// <summary>
        /// Convert subtitle file from one format to another
        /// </summary>
        Task ConvertAsync(
            Stream inputStream,
            string sourceFormat,
            Stream outputStream,
            string targetFormat,
            ConversionOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}
