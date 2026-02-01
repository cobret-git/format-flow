using FormatFlow.Core.Subtitles.Sbv;

namespace FormatFlow.Tests.Subtitles.Sbv
{
    [TestClass]
    public class SbvReaderTests
    {
        private readonly SbvReader _reader = new();

        #region Basic Parsing

        [TestMethod]
        public void Read_BasicSbv_ParsesCorrectly()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.BasicSbv);

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
            using var stream = SbvTestData.ToStream(SbvTestData.SingleEntrySbv);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(1, document.Entries.Count);
            Assert.AreEqual("Single subtitle entry", document.Entries[0].Text);
        }

        [TestMethod]
        public void Read_MultiLineText_PreservesNewlines()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.MultiLineSbv);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            Assert.IsTrue(entry.Text.Contains("first line"));
            Assert.IsTrue(entry.Text.Contains("second line"));
            Assert.IsTrue(entry.Text.Contains("third line"));
        }

        #endregion

        #region Timestamp Parsing

        [TestMethod]
        public void Read_LargeTimestamps_ParsesCorrectly()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.LongDurationSbv);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            Assert.AreEqual(new TimeSpan(0, 1, 30, 45, 123), entry.StartTime);
            Assert.AreEqual(new TimeSpan(0, 1, 30, 50, 999), entry.EndTime);
        }

        [TestMethod]
        public void Read_DoubleDigitHours_ParsesCorrectly()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.DoubleDigitHourSbv);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(2, document.Entries.Count);
            Assert.AreEqual(TimeSpan.Zero, document.Entries[0].StartTime);
            Assert.AreEqual(new TimeSpan(0, 1, 30, 45, 0), document.Entries[1].StartTime);
        }

        [TestMethod]
        public void Read_SingleDigitHour_ParsesCorrectly()
        {
            // Arrange - SBV commonly uses single digit hours
            const string sbv = """
                0:00:01.500,0:00:02.750
                Test
                """;
            using var stream = SbvTestData.ToStream(sbv);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(TimeSpan.FromMilliseconds(1500), document.Entries[0].StartTime);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2750), document.Entries[0].EndTime);
        }

        #endregion

        #region Text Content

        [TestMethod]
        public void Read_TextWithCommas_ParsesCorrectly()
        {
            // Arrange - Commas in text shouldn't conflict with timestamp separator
            using var stream = SbvTestData.ToStream(SbvTestData.TextWithCommasSbv);

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(2, document.Entries.Count);
            Assert.AreEqual("Hello, how are you?", document.Entries[0].Text);
            Assert.AreEqual("Fine, thanks, and you?", document.Entries[1].Text);
        }

        #endregion

        #region No VTT-Specific Features

        [TestMethod]
        public void Read_BasicSbv_NoPositionOrVoiceLabel()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.BasicSbv);

            // Act
            var document = _reader.Read(stream);

            // Assert - SBV doesn't support these features
            Assert.IsNull(document.Entries[0].Position);
            Assert.IsNull(document.Entries[0].VoiceLabel);
            Assert.IsNull(document.Entries[0].Style);
        }

        [TestMethod]
        public void Read_BasicSbv_NoMetadata()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.BasicSbv);

            // Act
            var document = _reader.Read(stream);

            // Assert - SBV has no header/metadata
            Assert.IsNull(document.Metadata.Title);
        }

        #endregion

        #region Error Cases

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Read_MalformedTimestamp_ThrowsFormatException()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.InvalidTimestamp);

            // Act
            _reader.Read(stream); // Should throw
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Read_WrongSeparator_ThrowsFormatException()
        {
            // Arrange - Uses --> instead of ,
            using var stream = SbvTestData.ToStream(SbvTestData.WrongSeparator);

            // Act
            _reader.Read(stream); // Should throw
        }

        [TestMethod]
        public void Read_EmptyStream_ReturnsEmptyDocument()
        {
            // Arrange
            using var stream = SbvTestData.ToStream("");

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(0, document.Entries.Count);
        }

        #endregion

        #region Format Property

        [TestMethod]
        public void Format_ReturnsSbv()
        {
            Assert.AreEqual("sbv", _reader.Format);
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task ReadAsync_BasicSbv_ParsesCorrectly()
        {
            // Arrange
            using var stream = SbvTestData.ToStream(SbvTestData.BasicSbv);

            // Act
            var document = await _reader.ReadAsync(stream);

            // Assert
            Assert.AreEqual(3, document.Entries.Count);
        }

        #endregion
    }
}
