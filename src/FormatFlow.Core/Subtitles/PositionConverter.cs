using FormatFlow.Core.Subtitles.Ttml;
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

        #region TTML Conversions

        /// <summary>
        /// Convert any position to TtmlRegion.
        /// Returns null if source is null.
        /// </summary>
        public static TtmlRegion? ToTtmlRegion(ISubtitlePosition? position, string regionId = "r1")
        {
            if (position == null)
                return null;

            if (position is TtmlRegion ttml)
                return ttml;

            if (position is VttPosition vtt)
                return VttToTtmlRegion(vtt, regionId);

            // Generic fallback - create basic region with alignment only
            return new TtmlRegion
            {
                RegionId = regionId,
                HorizontalAlign = position.HorizontalAlign
            };
        }

        /// <summary>
        /// Convert VttPosition to TtmlRegion with approximate mapping.
        /// </summary>
        private static TtmlRegion VttToTtmlRegion(VttPosition vtt, string regionId)
        {
            // Map VTT line to TTML originY
            string? originY = vtt.Line switch
            {
                0 => "10%",           // Top
                -1 or null => "80%",  // Bottom (default)
                var l when l > 0 => $"{10 + l * 10}%",
                var l when l < -1 => $"{90 + (l + 1) * 10}%",
                _ => null
            };

            // Map VTT position to TTML originX
            string? originX = vtt.Position.HasValue ? $"{vtt.Position}%" : "10%";

            // Map VTT size to TTML extent width
            string? extentWidth = vtt.Size.HasValue ? $"{vtt.Size}%" : "80%";

            return new TtmlRegion
            {
                RegionId = regionId,
                OriginX = originX,
                OriginY = originY,
                ExtentWidth = extentWidth,
                ExtentHeight = "20%",
                HorizontalAlign = vtt.HorizontalAlign
            };
        }

        /// <summary>
        /// Convert TtmlRegion to VttPosition with approximate mapping.
        /// Lossy conversion - TTML regions have more capabilities than VTT cue settings.
        /// </summary>
        public static VttPosition? TtmlToVttPosition(TtmlRegion? region)
        {
            if (region == null)
                return null;

            return new VttPosition
            {
                Line = ParsePercentageToVttLine(region.OriginY),
                Position = ParsePercentage(region.OriginX),
                Size = ParsePercentage(region.ExtentWidth),
                HorizontalAlign = region.HorizontalAlign
            };
        }

        /// <summary>
        /// Parse percentage string to integer (e.g., "50%" → 50).
        /// Returns null for non-percentage values like "100px".
        /// </summary>
        private static int? ParsePercentage(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (value.EndsWith('%') && int.TryParse(value.TrimEnd('%'), out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Convert TTML originY percentage to VTT line number.
        /// </summary>
        private static int? ParsePercentageToVttLine(string? originY)
        {
            var percent = ParsePercentage(originY);
            if (!percent.HasValue)
                return null;

            // Approximate mapping: 10% → 0, 80% → -1, etc.
            return percent.Value switch
            {
                <= 20 => 0,
                >= 70 => -1,
                _ => (percent.Value - 10) / 10
            };
        }

        #endregion
    }
}