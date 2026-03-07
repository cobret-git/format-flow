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
    public class TtmlReaderTests
    {
        private readonly TtmlReader _reader = new();

        #region Basic Parsing

        [TestMethod]
        public void Read_BasicTtml_ParsesCorrectly()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);

            var first = document.Entries[0];
            Assert.AreEqual(TimeSpan.Zero, first.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), first.EndTime);
            Assert.AreEqual("Lorem ipsum dolor sit amet.", first.Text);
        }

        [TestMethod]
        public void Read_SingleEntry_ParsesCorrectly()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.SingleEntryTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(1, document.Entries.Count);
            Assert.AreEqual("Single subtitle entry only.", document.Entries[0].Text);
        }

        [TestMethod]
        public void Read_EmptyTtml_ReturnsEmptyDocument()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.EmptyTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(0, document.Entries.Count);
        }

        [TestMethod]
        public void Read_MultipleDivs_FlattensAllEntries()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.MultipleDivTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(4, document.Entries.Count);
            Assert.AreEqual("First div, entry one.", document.Entries[0].Text);
            Assert.AreEqual("First div, entry two.", document.Entries[1].Text);
            Assert.AreEqual("Second div, entry one.", document.Entries[2].Text);
            Assert.AreEqual("Second div, entry two.", document.Entries[3].Text);
        }

        #endregion

        #region Timestamp Parsing

        [TestMethod]
        public void Read_OffsetTimeSeconds_ParsesCorrectly()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var second = document.Entries[1];
            Assert.AreEqual(TimeSpan.FromSeconds(3), second.StartTime);
            Assert.AreEqual(TimeSpan.FromSeconds(6), second.EndTime);
        }

        [TestMethod]
        public void Read_ClockTimeFormat_ParsesCorrectly()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.ClockTimeTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);

            // First: 00:00:01.500 -> 00:00:04.200
            var first = document.Entries[0];
            Assert.AreEqual(TimeSpan.FromMilliseconds(1500), first.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(4200), first.EndTime);

            // Second: 00:01:30.000 -> 00:01:35.500
            var second = document.Entries[1];
            Assert.AreEqual(TimeSpan.FromSeconds(90), second.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(95500), second.EndTime);

            // Third: 01:00:00.000 -> 01:00:05.000
            var third = document.Entries[2];
            Assert.AreEqual(TimeSpan.FromHours(1), third.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(1) + TimeSpan.FromSeconds(5), third.EndTime);
        }

        [TestMethod]
        public void Read_ShortClockTimeFormat_ParsesCorrectly()
        {
            // Arrange - MM:SS.mmm format (no hours) - Apple Music style
            using var stream = TtmlTestData.ToStream(TtmlTestData.ShortClockTimeTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(2, document.Entries.Count);

            // First: 00:17.292 = 17.292 seconds
            var first = document.Entries[0];
            Assert.AreEqual(TimeSpan.FromMilliseconds(17292), first.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(21042), first.EndTime);

            // Second: 02:02.359 = 122.359 seconds
            var second = document.Entries[1];
            Assert.AreEqual(TimeSpan.FromMilliseconds(122359), second.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(125968), second.EndTime);
        }

        [TestMethod]
        public void Read_MixedTimeUnits_ParsesCorrectly()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.MixedTimeUnitsTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);

            // Milliseconds: 500ms to 2500ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(500), document.Entries[0].StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), document.Entries[0].EndTime);

            // Seconds: 3s to 6s
            Assert.AreEqual(TimeSpan.FromSeconds(3), document.Entries[1].StartTime);
            Assert.AreEqual(TimeSpan.FromSeconds(6), document.Entries[1].EndTime);

            // Frames at 30fps: 90f = 3s, 180f = 6s
            Assert.AreEqual(TimeSpan.FromSeconds(3), document.Entries[2].StartTime);
            Assert.AreEqual(TimeSpan.FromSeconds(6), document.Entries[2].EndTime);
        }

        [TestMethod]
        public void Read_DurationAttribute_CalculatesEndTime()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.DurationTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(2, document.Entries.Count);

            // First: begin=0s, dur=2.5s -> end=2.5s
            var first = document.Entries[0];
            Assert.AreEqual(TimeSpan.Zero, first.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), first.EndTime);

            // Second: begin=3s, dur=3s -> end=6s
            var second = document.Entries[1];
            Assert.AreEqual(TimeSpan.FromSeconds(3), second.StartTime);
            Assert.AreEqual(TimeSpan.FromSeconds(6), second.EndTime);
        }

        #endregion

        #region Metadata Parsing

        [TestMethod]
        public void Read_TtmlWithMetadata_ExtractsTitleAndLanguage()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.TtmlWithMetadata);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual("Sample Subtitle Document", document.Metadata.Title);
            Assert.AreEqual("en-US", document.Metadata.Language);
        }

        [TestMethod]
        public void Read_BasicTtml_NoMetadata()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.IsNull(document.Metadata.Title);
            Assert.IsNull(document.Metadata.Language);
        }

        #endregion

        #region Styling

        [TestMethod]
        public void Read_StyledTtml_ExtractsInlineFormatting()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.StyledTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(4, document.Entries.Count);
            Assert.AreEqual("Welcome to our <b>tutorial</b> series.", document.Entries[0].Text);
            Assert.AreEqual("Today we learn about <i>video localization</i>.", document.Entries[1].Text);
            Assert.AreEqual("This is <u>underlined</u> text.", document.Entries[2].Text);
        }

        [TestMethod]
        public void Read_StyledTtml_ResolvesStyleReference()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.StyledTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert - fourth entry has style="highlight"
            var fourth = document.Entries[3];
            Assert.IsNotNull(fourth.Style);
            Assert.IsInstanceOfType(fourth.Style, typeof(TtmlStyle));

            var style = (TtmlStyle)fourth.Style;
            Assert.AreEqual("highlight", style.StyleId);
            Assert.AreEqual("#FFFF00", style.Color);
            Assert.AreEqual("#000000", style.BackgroundColor);
        }

        #endregion

        #region Regions

        [TestMethod]
        public void Read_RegionedTtml_ResolvesRegionReferences()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.RegionedTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);

            // First entry: region="top"
            var first = document.Entries[0];
            Assert.IsNotNull(first.Position);
            Assert.IsInstanceOfType(first.Position, typeof(TtmlRegion));
            var topRegion = (TtmlRegion)first.Position;
            Assert.AreEqual("top", topRegion.RegionId);
            Assert.AreEqual("10%", topRegion.OriginX);
            Assert.AreEqual("10%", topRegion.OriginY);
            Assert.AreEqual(PositionAlignment.Center, topRegion.HorizontalAlign);

            // Second entry: region="bottom"
            var second = document.Entries[1];
            Assert.IsNotNull(second.Position);
            var bottomRegion = (TtmlRegion)second.Position;
            Assert.AreEqual("bottom", bottomRegion.RegionId);
            Assert.AreEqual("80%", bottomRegion.OriginY);

            // Third entry: region="left"
            var third = document.Entries[2];
            Assert.IsNotNull(third.Position);
            var leftRegion = (TtmlRegion)third.Position;
            Assert.AreEqual("left", leftRegion.RegionId);
            Assert.AreEqual(PositionAlignment.Start, leftRegion.HorizontalAlign);
        }

        [TestMethod]
        public void Read_NoRegions_PositionIsNull()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.IsNull(document.Entries[0].Position);
        }

        #endregion

        #region Voice Labels / Agents

        [TestMethod]
        public void Read_AgentTtml_ExtractsVoiceLabels()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.AgentTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);
            Assert.AreEqual("person", document.Entries[0].VoiceLabel);
            Assert.AreEqual("other", document.Entries[1].VoiceLabel);
            Assert.AreEqual("person", document.Entries[2].VoiceLabel);
        }

        [TestMethod]
        public void Read_NoAgent_VoiceLabelIsNull()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.IsNull(document.Entries[0].VoiceLabel);
        }

        #endregion

        #region Word-Level Timing and Karaoke

        [TestMethod]
        public void Read_WordLevelTimingTtml_ConcatenatesSpanText()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.WordLevelTimingTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(1, document.Entries.Count);
            Assert.AreEqual("Alfabravocharliedelta echo", document.Entries[0].Text.Replace(" ", "").Replace("echo", " echo").Trim());
            // The spans concatenate without spaces, so we verify the syllables are present
            Assert.IsTrue(document.Entries[0].Text.Contains("Al"));
            Assert.IsTrue(document.Entries[0].Text.Contains("fa"));
            Assert.IsTrue(document.Entries[0].Text.Contains("echo"));
        }

        [TestMethod]
        public void Read_WordLevelTimingTtml_UsesParentTimestamps()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.WordLevelTimingTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert - uses <p> element timing, not individual span timing
            var entry = document.Entries[0];
            Assert.AreEqual(TimeSpan.FromMilliseconds(17292), entry.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(21042), entry.EndTime);
        }

        #endregion

        #region Romanization and Translation Exclusion

        [TestMethod]
        public void Read_RomanizationTtml_ExcludesRomanizationSpans()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.RomanizationTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(1, document.Entries.Count);
            var text = document.Entries[0].Text;

            // Should contain the main text
            Assert.IsTrue(text.Contains("AB"));
            Assert.IsTrue(text.Contains("CD"));
            Assert.IsTrue(text.Contains("EF"));

            // Should NOT contain the romanization
            Assert.IsFalse(text.Contains("a b c d e f"));
        }

        [TestMethod]
        public void Read_TranslationTtml_ExcludesTranslationSpans()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.TranslationTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(1, document.Entries.Count);
            var text = document.Entries[0].Text;

            // Should contain the main text
            Assert.IsTrue(text.Contains("Bonjour"));
            Assert.IsTrue(text.Contains("monde"));

            // Should NOT contain translation or romanization
            Assert.IsFalse(text.Contains("Hello world"));
            Assert.IsFalse(text.Contains("bon zhoor"));
        }

        #endregion

        #region Line Breaks

        [TestMethod]
        public void Read_LineBreakTtml_PreservesNewlines()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.LineBreakTtml);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            Assert.IsTrue(entry.Text.Contains("Line one"));
            Assert.IsTrue(entry.Text.Contains("Line two"));
            Assert.IsTrue(entry.Text.Contains("Line three"));
            // Should have newlines (either \n or \r\n)
            Assert.IsTrue(entry.Text.Contains("\n") || entry.Text.Contains(Environment.NewLine));
        }

        #endregion

        #region Error Cases

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Read_InvalidMissingRoot_ThrowsFormatException()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.InvalidMissingRoot);

            // Act
            _reader.Read(stream); // Should throw
        }

        [TestMethod]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void Read_InvalidMalformedXml_ThrowsXmlException()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.InvalidMalformedXml);

            // Act
            _reader.Read(stream); // Should throw
        }

        #endregion

        #region Format Property

        [TestMethod]
        public void Format_ReturnsTtml()
        {
            Assert.AreEqual("ttml", _reader.Format);
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task ReadAsync_BasicTtml_ParsesCorrectly()
        {
            // Arrange
            using var stream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act
            var document = await _reader.ReadAsync(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);
        }

        #endregion
    }
}
