using FormatFlow.Core.Subtitles.Vtt;

namespace FormatFlow.Core.Subtitles
{
    /// <summary>
    /// Utility class for converting between format-specific position types.
    /// Conversions may be lossy depending on source and target format capabilities.
    /// </summary>
    public static class PositionConverter
    {
        /// <summary>
        /// Convert any position to VttPosition.
        /// Returns null if source is null.
        /// </summary>
        public static VttPosition? ToVttPosition(ISubtitlePosition? position)
        {
            if (position == null)
                return null;

            if (position is VttPosition vtt)
                return vtt;

            // Generic fallback - preserve only what VTT supports
            return new VttPosition
            {
                HorizontalAlign = position.HorizontalAlign
            };
        }

        /// <summary>
        /// Extract VTT-compatible line value from any position.
        /// </summary>
        public static int? GetLine(ISubtitlePosition? position)
        {
            return position switch
            {
                VttPosition vtt => vtt.Line,
                _ => null
            };
        }

        /// <summary>
        /// Extract VTT-compatible position (horizontal %) from any position.
        /// </summary>
        public static int? GetPosition(ISubtitlePosition? position)
        {
            return position switch
            {
                VttPosition vtt => vtt.Position,
                _ => null
            };
        }

        /// <summary>
        /// Extract alignment from any position type.
        /// </summary>
        public static PositionAlignment? GetAlignment(ISubtitlePosition? position)
        {
            return position?.HorizontalAlign;
        }

        /// <summary>
        /// Check if position has any meaningful VTT-compatible settings.
        /// </summary>
        public static bool HasVttSettings(ISubtitlePosition? position)
        {
            if (position == null)
                return false;

            if (position is VttPosition vtt)
            {
                return vtt.Line.HasValue ||
                       vtt.Position.HasValue ||
                       vtt.HorizontalAlign.HasValue ||
                       vtt.Size.HasValue ||
                       vtt.Vertical != null;
            }

            return position.HorizontalAlign.HasValue;
        }
    }
}
