using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Vtt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles
{
    [TestClass]
    public class VttReaderTests
    {
        private readonly VttReader _reader = new();

        #region Basic Parsing

        [TestMethod]
        public void Read_BasicVtt_ParsesCorrectly()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);

            var first = document.Entries[0];
            Assert.AreEqual(TimeSpan.Zero, first.StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), first.EndTime);
            Assert.AreEqual("Welcome to the Example Subtitle File!", first.Text);
        }

        [TestMethod]
        public void Read_SingleEntry_ParsesCorrectly()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.SingleEntryVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(1, document.Entries.Count);
            Assert.AreEqual("Single subtitle entry", document.Entries[0].Text);
        }

        [TestMethod]
        public void Read_NoCueIdentifiers_ParsesCorrectly()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.NoCueIdVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(2, document.Entries.Count);
            Assert.AreEqual("First subtitle without ID", document.Entries[0].Text);
            Assert.AreEqual("Second subtitle without ID", document.Entries[1].Text);
        }

        #endregion

        #region Header Parsing

        [TestMethod]
        public void Read_VttWithTitle_ExtractsTitle()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.VttWithTitle);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual("My Video Title", document.Metadata.Title);
        }

        [TestMethod]
        public void Read_BasicVtt_NoTitle()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.IsNull(document.Metadata.Title);
        }

        #endregion

        #region Timestamp Parsing

        [TestMethod]
        public void Read_LargeTimestamps_ParsesCorrectly()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.LongDurationVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            Assert.AreEqual(new TimeSpan(0, 1, 30, 45, 123), entry.StartTime);
            Assert.AreEqual(new TimeSpan(0, 1, 30, 50, 999), entry.EndTime);
        }

        [TestMethod]
        public void Read_VttTimestampFormat_UsesDotSeparator()
        {
            // Arrange - VTT uses . not , for milliseconds
            const string vtt = """
                WEBVTT

                1
                00:00:01.500 --> 00:00:02.750
                Test
                """;
            using var stream = VttTestData.ToStream(vtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(TimeSpan.FromMilliseconds(1500), document.Entries[0].StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2750), document.Entries[0].EndTime);
        }

        #endregion

        #region Formatting Tags

        [TestMethod]
        public void Read_FormattedVtt_PreservesTags()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.FormattedVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(5, document.Entries.Count);
            Assert.AreEqual("Welcome to our <b>tutorial</b> series.", document.Entries[0].Text);
            Assert.AreEqual("Today we'll learn about <i>video localization</i>.", document.Entries[1].Text);
            Assert.IsTrue(document.Entries[2].Text.Contains("<font color=\"yellow\">"));
            Assert.AreEqual("This text is <u>underlined</u> for emphasis.", document.Entries[3].Text);
            Assert.AreEqual("[Dramatic music plays]", document.Entries[4].Text);
        }

        #endregion

        #region Position Cue Settings

        [TestMethod]
        public void Read_PositionedVtt_ParsesCueSettings()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.PositionedVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);

            // First: line:0 position:50% align:center
            var first = document.Entries[0];
            Assert.IsNotNull(first.Position);
            Assert.AreEqual(0, first.Position.Line);
            Assert.AreEqual(50, first.Position.Position);
            Assert.AreEqual(PositionAlignment.Center, first.Position.Align);

            // Second: line:-1 align:end
            var second = document.Entries[1];
            Assert.IsNotNull(second.Position);
            Assert.AreEqual(-1, second.Position.Line);
            Assert.AreEqual(PositionAlignment.End, second.Position.Align);

            // Third: position:10% align:start
            var third = document.Entries[2];
            Assert.IsNotNull(third.Position);
            Assert.AreEqual(10, third.Position.Position);
            Assert.AreEqual(PositionAlignment.Start, third.Position.Align);
        }

        [TestMethod]
        public void Read_NoPositionSettings_PositionIsNull()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.IsNull(document.Entries[0].Position);
        }

        #endregion

        #region Voice Labels

        [TestMethod]
        public void Read_VoiceLabelVtt_ExtractsVoiceLabels()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.VoiceLabelVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);
            Assert.AreEqual("Alice", document.Entries[0].VoiceLabel);
            Assert.AreEqual("Bob", document.Entries[1].VoiceLabel);
            Assert.AreEqual("Alice", document.Entries[2].VoiceLabel);
        }

        [TestMethod]
        public void Read_NoVoiceLabel_VoiceLabelIsNull()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.IsNull(document.Entries[0].VoiceLabel);
        }

        [TestMethod]
        public void Read_VoiceLabelVtt_PreservesVoiceTagInText()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.VoiceLabelVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert - voice tag should remain in text for round-trip
            Assert.IsTrue(document.Entries[0].Text.Contains("<v Alice>"));
        }

        #endregion

        #region Comments

        [TestMethod]
        public void Read_VttWithComments_SkipsComments()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.VttWithComments);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(2, document.Entries.Count);
            Assert.AreEqual("First subtitle", document.Entries[0].Text);
            Assert.AreEqual("Second subtitle", document.Entries[1].Text);
        }

        #endregion

        #region Multi-line Text

        [TestMethod]
        public void Read_MultiLineText_PreservesNewlines()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.MultiLineVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            Assert.IsTrue(entry.Text.Contains("first line"));
            Assert.IsTrue(entry.Text.Contains("second line"));
            Assert.IsTrue(entry.Text.Contains("third line"));
        }

        #endregion

        #region Error Cases

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Read_MissingHeader_ThrowsFormatException()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.InvalidMissingHeader);

            // Act
            _reader.Read(stream); // Should throw
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Read_MalformedTimestamp_ThrowsFormatException()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.InvalidTimestamp);

            // Act
            _reader.Read(stream); // Should throw
        }

        [TestMethod]
        public void Read_EmptyVttWithHeader_ReturnsEmptyDocument()
        {
            // Arrange
            const string emptyVtt = "WEBVTT\n\n";
            using var stream = VttTestData.ToStream(emptyVtt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(0, document.Entries.Count);
        }

        #endregion

        #region Format Property

        [TestMethod]
        public void Format_ReturnsVtt()
        {
            Assert.AreEqual("vtt", _reader.Format);
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task ReadAsync_BasicVtt_ParsesCorrectly()
        {
            // Arrange
            using var stream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act
            var document = await _reader.ReadAsync(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);
        }

        #endregion
    }
}
