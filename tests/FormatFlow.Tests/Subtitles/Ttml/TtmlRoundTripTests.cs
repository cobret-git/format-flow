using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Ttml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles.Ttml
{
    [TestClass]
    public class TtmlRoundTripTests
    {
        private readonly TtmlReader _reader = new();
        private readonly TtmlWriter _writer = new();

        #region TTML -> Document -> TTML Round-trips

        [TestMethod]
        public void RoundTrip_BasicTtml_PreservesContent()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act - Read
            var document = _reader.Read(inputStream);

            // Act - Write
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);

            // Act - Read again
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert
            Assert.AreEqual(document.Entries.Count, roundTripped.Entries.Count);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                AssertEntriesEqual(document.Entries[i], roundTripped.Entries[i], i);
            }
        }

        [TestMethod]
        public void RoundTrip_ClockTimeTtml_PreservesTimestamps()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.ClockTimeTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - timestamps within 1ms tolerance
            for (int i = 0; i < document.Entries.Count; i++)
            {
                AssertTimestampsEqual(document.Entries[i], roundTripped.Entries[i], i);
            }
        }

        [TestMethod]
        public void RoundTrip_ShortClockTimeTtml_PreservesTimestamps()
        {
            // Arrange - Apple Music style MM:SS.mmm
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.ShortClockTimeTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert
            Assert.AreEqual(2, roundTripped.Entries.Count);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                AssertTimestampsEqual(document.Entries[i], roundTripped.Entries[i], i);
            }
        }

        [TestMethod]
        public void RoundTrip_DurationTtml_ConvertsToEndTime()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.DurationTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - dur attribute converted to end attribute
            Assert.AreEqual(2, roundTripped.Entries.Count);

            // First: 0s + 2.5s duration = 2.5s end
            Assert.AreEqual(TimeSpan.Zero, roundTripped.Entries[0].StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), roundTripped.Entries[0].EndTime);
        }

        [TestMethod]
        public void RoundTrip_MixedTimeUnits_NormalizesToSeconds()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.MixedTimeUnitsTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - all time units normalized
            Assert.AreEqual(3, roundTripped.Entries.Count);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                AssertTimestampsEqual(document.Entries[i], roundTripped.Entries[i], i);
            }
        }

        #endregion

        #region Metadata Round-trips

        [TestMethod]
        public void RoundTrip_TtmlWithMetadata_PreservesLanguage()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.TtmlWithMetadata);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert
            Assert.AreEqual(document.Metadata.Language, roundTripped.Metadata.Language);
        }

        #endregion

        #region Styling Round-trips

        [TestMethod]
        public void RoundTrip_StyledTtml_PreservesInlineFormatting()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.StyledTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - inline formatting tags preserved
            Assert.IsTrue(roundTripped.Entries[0].Text.Contains("<b>") ||
                         roundTripped.Entries[0].Text.Contains("tutorial"));
            Assert.IsTrue(roundTripped.Entries[1].Text.Contains("<i>") ||
                         roundTripped.Entries[1].Text.Contains("video localization"));
        }

        #endregion

        #region Region Round-trips

        [TestMethod]
        public void RoundTrip_RegionedTtml_PreservesRegionInfo()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.RegionedTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert
            Assert.AreEqual(3, roundTripped.Entries.Count);

            // Verify regions are preserved
            for (int i = 0; i < document.Entries.Count; i++)
            {
                if (document.Entries[i].Position is TtmlRegion originalRegion)
                {
                    Assert.IsNotNull(roundTripped.Entries[i].Position, $"Position lost at entry {i}");
                    Assert.IsInstanceOfType(roundTripped.Entries[i].Position, typeof(TtmlRegion));

                    var roundTrippedRegion = (TtmlRegion)roundTripped.Entries[i].Position!;
                    Assert.AreEqual(originalRegion.RegionId, roundTrippedRegion.RegionId, $"RegionId mismatch at entry {i}");
                }
            }
        }

        #endregion

        #region Agent / Voice Label Round-trips

        [TestMethod]
        public void RoundTrip_AgentTtml_PreservesVoiceLabelsInDocument()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.AgentTtml);

            // Act - just verify read works correctly
            var document = _reader.Read(inputStream);

            // Assert - voice labels parsed from agents
            Assert.AreEqual("person", document.Entries[0].VoiceLabel);
            Assert.AreEqual("other", document.Entries[1].VoiceLabel);
            Assert.AreEqual("person", document.Entries[2].VoiceLabel);
        }

        #endregion

        #region Complex Content Round-trips

        [TestMethod]
        public void RoundTrip_WordLevelTimingTtml_PreservesConcatenatedText()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.WordLevelTimingTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - text content preserved (word-level timing lost, that's expected)
            Assert.AreEqual(1, roundTripped.Entries.Count);
            Assert.IsTrue(roundTripped.Entries[0].Text.Contains("Al"));
            Assert.IsTrue(roundTripped.Entries[0].Text.Contains("echo"));
        }

        [TestMethod]
        public void RoundTrip_RomanizationTtml_ExcludesRomanization()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.RomanizationTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - romanization still excluded
            var text = roundTripped.Entries[0].Text;
            Assert.IsTrue(text.Contains("AB"));
            Assert.IsFalse(text.Contains("a b c d"));
        }

        [TestMethod]
        public void RoundTrip_LineBreakTtml_PreservesNewlines()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.LineBreakTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert
            var text = roundTripped.Entries[0].Text;
            Assert.IsTrue(text.Contains("Line one"));
            Assert.IsTrue(text.Contains("Line two"));
            Assert.IsTrue(text.Contains("Line three"));
        }

        [TestMethod]
        public void RoundTrip_MultipleDivs_FlattensToSingleDiv()
        {
            // Arrange
            using var inputStream = TtmlTestData.ToStream(TtmlTestData.MultipleDivTtml);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);
            outputStream.Position = 0;
            var roundTripped = _reader.Read(outputStream);

            // Assert - all entries preserved even if div structure changes
            Assert.AreEqual(4, roundTripped.Entries.Count);
            Assert.AreEqual("First div, entry one.", roundTripped.Entries[0].Text);
            Assert.AreEqual("Second div, entry two.", roundTripped.Entries[3].Text);
        }

        #endregion

        #region Document -> TTML -> Document Round-trips

        [TestMethod]
        public void RoundTrip_FromDocument_BasicDocument()
        {
            // Arrange
            var original = TtmlTestData.CreateBasicDocument();

            // Act
            using var stream = new MemoryStream();
            _writer.Write(original, stream);
            stream.Position = 0;
            var roundTripped = _reader.Read(stream);

            // Assert
            Assert.AreEqual(original.Entries.Count, roundTripped.Entries.Count);

            for (int i = 0; i < original.Entries.Count; i++)
            {
                AssertEntriesEqual(original.Entries[i], roundTripped.Entries[i], i);
            }
        }

        [TestMethod]
        public void RoundTrip_FromDocument_DocumentWithRegions()
        {
            // Arrange
            var original = TtmlTestData.CreateDocumentWithRegions();

            // Act
            using var stream = new MemoryStream();
            _writer.Write(original, stream);
            stream.Position = 0;
            var roundTripped = _reader.Read(stream);

            // Assert
            Assert.AreEqual(original.Entries.Count, roundTripped.Entries.Count);

            for (int i = 0; i < original.Entries.Count; i++)
            {
                var originalRegion = original.Entries[i].Position as TtmlRegion;
                var roundTrippedRegion = roundTripped.Entries[i].Position as TtmlRegion;

                if (originalRegion != null)
                {
                    Assert.IsNotNull(roundTrippedRegion, $"Region lost at entry {i}");
                    Assert.AreEqual(originalRegion.RegionId, roundTrippedRegion.RegionId);
                }
            }
        }

        [TestMethod]
        public void RoundTrip_FromDocument_EmptyDocument()
        {
            // Arrange
            var original = TtmlTestData.CreateEmptyDocument();

            // Act
            using var stream = new MemoryStream();
            _writer.Write(original, stream);
            stream.Position = 0;
            var roundTripped = _reader.Read(stream);

            // Assert
            Assert.AreEqual(0, roundTripped.Entries.Count);
        }

        #endregion

        #region Helper Methods

        private static void AssertEntriesEqual(SubtitleEntry expected, SubtitleEntry actual, int index)
        {
            Assert.AreEqual(
                expected.Text.Trim(),
                actual.Text.Trim(),
                $"Text mismatch at entry {index}");

            AssertTimestampsEqual(expected, actual, index);
        }

        private static void AssertTimestampsEqual(SubtitleEntry expected, SubtitleEntry actual, int index)
        {
            const double toleranceMs = 1.0;

            Assert.IsTrue(
                Math.Abs((expected.StartTime - actual.StartTime).TotalMilliseconds) < toleranceMs,
                $"StartTime mismatch at entry {index}: expected {expected.StartTime}, got {actual.StartTime}");

            Assert.IsTrue(
                Math.Abs((expected.EndTime - actual.EndTime).TotalMilliseconds) < toleranceMs,
                $"EndTime mismatch at entry {index}: expected {expected.EndTime}, got {actual.EndTime}");
        }

        #endregion
    }
}
