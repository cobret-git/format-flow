using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Sbv;

namespace FormatFlow.Tests.Subtitles.Sbv
{
    [TestClass]
    public class SbvWriterTests
    {
        private readonly SbvWriter _writer = new();

        #region Basic Writing

        [TestMethod]
        public void Write_BasicDocument_GeneratesCorrectSbv()
        {
            // Arrange
            var document = SbvTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("0:00:00.000,0:00:02.500"));
            Assert.IsTrue(result.Contains("Welcome to the Example Subtitle File!"));
        }

        [TestMethod]
        public void Write_BasicDocument_MatchesExpectedFormat()
        {
            // Arrange
            var document = SbvTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.AreEqual(SbvTestData.Normalize(SbvTestData.BasicSbv), SbvTestData.Normalize(result));
        }

        [TestMethod]
        public void Write_SingleEntry_NoTrailingSeparator()
        {
            // Arrange
            var document = SbvTestData.CreateSingleEntryDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.AreEqual(SbvTestData.Normalize(SbvTestData.SingleEntrySbv), SbvTestData.Normalize(result));
        }

        #endregion

        #region No Header

        [TestMethod]
        public void Write_BasicDocument_NoHeader()
        {
            // Arrange
            var document = SbvTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            // SBV has no header - first line should be timestamp
            Assert.IsTrue(result.StartsWith("0:00:00.000,"));
        }

        #endregion

        #region Timestamp Formatting

        [TestMethod]
        public void Write_Timestamps_UsesSbvFormat()
        {
            // Arrange
            var document = SbvTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            // SBV uses comma separator and dot for milliseconds
            Assert.IsTrue(result.Contains("0:00:00.000,0:00:02.500"));
            Assert.IsFalse(result.Contains(" --> ")); // Not SRT/VTT style
        }

        [TestMethod]
        public void Write_LargeTimestamps_FormatsCorrectly()
        {
            // Arrange
            var document = SbvTestData.CreateLongDurationDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("1:30:45.123,1:30:50.999"));
        }

        [TestMethod]
        public void Write_SingleDigitHours_UsesMinimalFormat()
        {
            // Arrange - Hours < 10 should be single digit
            var document = SbvTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("0:00:00.000")); // Single digit hour
            Assert.IsFalse(result.Contains("00:00:00.000")); // Not double digit
        }

        #endregion

        #region Multi-line Text

        [TestMethod]
        public void Write_MultiLineDocument_PreservesNewlines()
        {
            // Arrange
            var document = SbvTestData.CreateMultiLineDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("first line"));
            Assert.IsTrue(result.Contains("second line"));
            Assert.IsTrue(result.Contains("third line"));
        }

        #endregion

        #region VTT Features Ignored

        [TestMethod]
        public void Write_DocumentWithPosition_IgnoresPosition()
        {
            // Arrange - SBV doesn't support positioning
            var document = new SubtitleDocument
            {
                Entries = new[]
                {
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.FromSeconds(2),
                        Text = "Test",
                        Position = new SubtitlePosition { Line = 0, Position = 50, Align = PositionAlignment.Center }
                    }
                }
            };
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsFalse(result.Contains("line:"));
            Assert.IsFalse(result.Contains("position:"));
            Assert.IsFalse(result.Contains("align:"));
        }

        [TestMethod]
        public void Write_DocumentWithVoiceLabel_IgnoresVoiceLabel()
        {
            // Arrange - SBV doesn't support voice labels
            var document = new SubtitleDocument
            {
                Entries = new[]
                {
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.FromSeconds(2),
                        Text = "Hello there",
                        VoiceLabel = "Alice"
                    }
                }
            };
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsFalse(result.Contains("<v Alice>"));
            Assert.IsFalse(result.Contains("</v>"));
            Assert.IsTrue(result.Contains("Hello there"));
        }

        [TestMethod]
        public void Write_DocumentWithTitle_IgnoresTitle()
        {
            // Arrange - SBV doesn't support metadata
            var document = new SubtitleDocument
            {
                Metadata = new SubtitleMetadata { Title = "My Video" },
                Entries = new[]
                {
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.FromSeconds(2),
                        Text = "Test"
                    }
                }
            };
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsFalse(result.Contains("My Video"));
            // First line should be timestamp, not header
            Assert.IsTrue(result.StartsWith("0:00:00.000,"));
        }

        #endregion

        #region Empty Document

        [TestMethod]
        public void Write_EmptyDocument_GeneratesEmptyFile()
        {
            // Arrange
            var document = SbvTestData.CreateEmptyDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.AreEqual("", result);
        }

        #endregion

        #region Stream Behavior

        [TestMethod]
        public void Write_StreamNotClosed_CanReadAfterWrite()
        {
            // Arrange
            var document = SbvTestData.CreateSingleEntryDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert - stream should still be open
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanWrite);
        }

        #endregion

        #region Format Property

        [TestMethod]
        public void Format_ReturnsSbv()
        {
            Assert.AreEqual("sbv", _writer.Format);
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task WriteAsync_BasicDocument_GeneratesCorrectSbv()
        {
            // Arrange
            var document = SbvTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            await _writer.WriteAsync(document, stream);

            // Assert
            var result = SbvTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("0:00:00.000,0:00:02.500"));
        }

        #endregion
    }
}
