namespace FormatFlow.Core.Subtitles
{
    public class SubtitleConverter : ISubtitleConverter
    {
        private readonly Dictionary<string, ISubtitleReader> _readers;
        private readonly Dictionary<string, ISubtitleWriter> _writers;

        public SubtitleConverter(
            IEnumerable<ISubtitleReader> readers,
            IEnumerable<ISubtitleWriter> writers)
        {
            _readers = readers.ToDictionary(r => r.Format, StringComparer.OrdinalIgnoreCase);
            _writers = writers.ToDictionary(w => w.Format, StringComparer.OrdinalIgnoreCase);
        }

        public async Task ConvertAsync(
            Stream inputStream,
            string sourceFormat,
            Stream outputStream,
            string targetFormat,
            ConversionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (!_readers.TryGetValue(sourceFormat, out var reader))
                throw new NotSupportedException($"Source format '{sourceFormat}' is not supported");

            if (!_writers.TryGetValue(targetFormat, out var writer))
                throw new NotSupportedException($"Target format '{targetFormat}' is not supported");

            // Read → Transform → Write
            var document = await reader.ReadAsync(inputStream, options?.Encoding, cancellationToken);

            if (options?.Transform != null)
                document = options.Transform(document);

            await writer.WriteAsync(document, outputStream, options?.Encoding, cancellationToken);
        }

        public IReadOnlyList<string> SupportedFormats =>
            _readers.Keys.Union(_writers.Keys).ToList();
    }
}
