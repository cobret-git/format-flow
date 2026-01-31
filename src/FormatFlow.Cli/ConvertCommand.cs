using FormatFlow.Core.Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Cli
{
    public class ConvertCommand
    {
        private readonly SubtitleConverter _converter;
        private readonly IConversionReporter _reporter;

        public ConvertCommand(SubtitleConverter converter, IConversionReporter reporter)
        {
            _converter = converter;
            _reporter = reporter;
        }

        public async Task<int> ExecuteAsync(ConvertOptions options)
        {
            var files = ResolveFiles(options);

            if (files.Count == 0)
            {
                _reporter.Warning("No matching files found");
                return 0;
            }

            _reporter.Info($"Found {files.Count} file(s) to convert");

            var results = new ConversionSummary();

            foreach (var file in files)
            {
                await ConvertFileAsync(file, options, results);
            }

            _reporter.Summary(results);
            return results.Failed > 0 ? 1 : 0;
        }

        private List<string> ResolveFiles(ConvertOptions options)
        {
            var path = options.Path;

            // Single file
            if (File.Exists(path))
            {
                return new List<string> { path };
            }

            // Directory
            if (!Directory.Exists(path))
            {
                _reporter.Error($"Path not found: {path}");
                return new List<string>();
            }

            var searchOption = options.Recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            var pattern = options.FromFormat != null
                ? $"*.{options.FromFormat}"
                : "*.*";

            return Directory.GetFiles(path, pattern, searchOption)
                .Where(f => IsSubtitleFile(f, options.FromFormat))
                .OrderBy(f => f)
                .ToList();
        }

        private bool IsSubtitleFile(string filePath, string? format)
        {
            var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

            if (format != null)
                return ext.Equals(format, StringComparison.OrdinalIgnoreCase);

            return _converter.SupportedFormats.Contains(ext);
        }

        private async Task ConvertFileAsync(string inputPath, ConvertOptions options, ConversionSummary summary)
        {
            var sourceFormat = options.FromFormat
                ?? Path.GetExtension(inputPath).TrimStart('.').ToLowerInvariant();

            var outputPath = Path.ChangeExtension(inputPath, options.ToFormat);

            // Skip if same format
            if (sourceFormat.Equals(options.ToFormat, StringComparison.OrdinalIgnoreCase))
            {
                _reporter.Skip(inputPath, "Same format");
                summary.Skipped++;
                return;
            }

            // Check if output exists
            if (File.Exists(outputPath) && !options.Overwrite)
            {
                _reporter.Skip(inputPath, "Output exists (use --overwrite)");
                summary.Skipped++;
                return;
            }

            try
            {
                await using var inputStream = File.OpenRead(inputPath);
                await using var outputStream = File.Create(outputPath);

                await _converter.ConvertAsync(
                    inputStream,
                    sourceFormat,
                    outputStream,
                    options.ToFormat);

                _reporter.Success(inputPath, outputPath);
                summary.Succeeded++;
            }
            catch (Exception ex)
            {
                _reporter.Failure(inputPath, ex.Message);
                summary.Failed++;
            }
        }
    }
}
