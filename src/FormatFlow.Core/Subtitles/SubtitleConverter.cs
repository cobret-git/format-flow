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

        /// <summary>
        /// All formats supported for reading or writing
        /// </summary>
        public IReadOnlyList<string> SupportedFormats =>
            _readers.Keys.Union(_writers.Keys, StringComparer.OrdinalIgnoreCase).ToList();

        /// <summary>
        /// Formats that can be read (used as source)
        /// </summary>
        public IReadOnlyList<string> ReadableFormats => _readers.Keys.ToList();

        /// <summary>
        /// Formats that can be written (used as target)
        /// </summary>
        public IReadOnlyList<string> WritableFormats => _writers.Keys.ToList();

        /// <summary>
        /// Check if a format can be read
        /// </summary>
        public bool CanRead(string format) =>
            _readers.ContainsKey(format);

        /// <summary>
        /// Check if a format can be written
        /// </summary>
        public bool CanWrite(string format) =>
            _writers.ContainsKey(format);

        /// <summary>
        /// Check if conversion between formats is supported
        /// </summary>
        public bool CanConvert(string sourceFormat, string targetFormat) =>
            CanRead(sourceFormat) && CanWrite(targetFormat);

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

        /// <summary>
        /// Convert file and return the document (useful for inspection/chaining)
        /// </summary>
        public async Task<SubtitleDocument> ReadAsync(
            Stream inputStream,
            string sourceFormat,
            ConversionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (!_readers.TryGetValue(sourceFormat, out var reader))
                throw new NotSupportedException($"Source format '{sourceFormat}' is not supported");

            var document = await reader.ReadAsync(inputStream, options?.Encoding, cancellationToken);

            if (options?.Transform != null)
                document = options.Transform(document);

            return document;
        }

        /// <summary>
        /// Write document to stream in specified format
        /// </summary>
        public async Task WriteAsync(
            SubtitleDocument document,
            Stream outputStream,
            string targetFormat,
            ConversionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (!_writers.TryGetValue(targetFormat, out var writer))
                throw new NotSupportedException($"Target format '{targetFormat}' is not supported");

            await writer.WriteAsync(document, outputStream, options?.Encoding, cancellationToken);
        }
    }
}
