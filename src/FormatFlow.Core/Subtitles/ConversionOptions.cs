using System.Text;

namespace FormatFlow.Core.Subtitles
{
    public class ConversionOptions
    {
        public Encoding? Encoding { get; init; }
        public Func<SubtitleDocument, SubtitleDocument>? Transform { get; init; }
    }
}
