using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Vtt;

namespace FormatFlow.Tests.Subtitles
{
    [TestClass]
    public class VttWriterTests
    {
        private readonly VttWriter _writer = new();

        #region Basic Writing

        [TestMethod]
        public void Write_BasicDocument_GeneratesCorrectVtt()
        {
            // Arrange
            var document = VttTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.StartsWith("WEBVTT"));
            Assert.IsTrue(result.Contains("00:00:00.000 --> 00:00:02.500"));
            Assert.IsTrue(result.Contains("Welcome to the Example Subtitle File!"));
        }

        [TestMethod]
        public void Write_BasicDocument_MatchesExpectedFormat()
        {
            // Arrange
            var document = VttTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.AreEqual(VttTestData.Normalize(VttTestData.BasicVtt), VttTestData.Normalize(result));
        }

        [TestMethod]
        public void Write_SingleEntry_NoTrailingSeparator()
        {
            // Arrange
            var document = VttTestData.CreateSingleEntryDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.AreEqual(VttTestData.Normalize(VttTestData.SingleEntryVtt), VttTestData.Normalize(result));
        }

        #endregion

        #region Header

        [TestMethod]
        public void Write_DocumentWithTitle_IncludesTitleInHeader()
        {
            // Arrange
            var document = VttTestData.CreateDocumentWithTitle();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.StartsWith("WEBVTT My Video Title"));
        }

        [TestMethod]
        public void Write_DocumentWithoutTitle_BasicHeader()
        {
            // Arrange
            var document = VttTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            var firstLine = result.Split('\n')[0].Trim();
            Assert.AreEqual("WEBVTT", firstLine);
        }

        #endregion

        #region Timestamp Formatting

        [TestMethod]
        public void Write_Timestamps_UsesDotNotComma()
        {
            // Arrange
            var document = VttTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("00:00:00.000"));
            Assert.IsFalse(result.Contains("00:00:00,000")); // No SRT-style commas
        }

        [TestMethod]
        public void Write_LargeTimestamps_FormatsCorrectly()
        {
            // Arrange
            var document = VttTestData.CreateLongDurationDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("01:30:45.123 --> 01:30:50.999"));
        }

        #endregion

        #region Position Settings

        [TestMethod]
        public void Write_PositionedDocument_IncludesCueSettings()
        {
            // Arrange
            var document = VttTestData.CreatePositionedDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("line:0"));
            Assert.IsTrue(result.Contains("position:50%"));
            Assert.IsTrue(result.Contains("align:center"));
            Assert.IsTrue(result.Contains("align:end"));
            Assert.IsTrue(result.Contains("align:start"));
        }

        [TestMethod]
        public void Write_PositionedDocument_SettingsAfterArrow()
        {
            // Arrange
            var document = new SubtitleDocument
            {
                Entries = new[]
                {
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.FromSeconds(2),
                        Text = "Test",
                        Position = new SubtitlePosition { Line = 5 }
                    }
                }
            };
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("--> 00:00:02.000 line:5"));
        }

        [TestMethod]
        public void Write_NoPosition_NoExtraCueSettings()
        {
            // Arrange
            var document = VttTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            // Should not have trailing space after timestamp
            Assert.IsFalse(result.Contains("--> 00:00:02.500 \n"));
        }

        #endregion

        #region Voice Labels

        [TestMethod]
        public void Write_VoiceLabelDocument_WrapsTextWithVoiceTag()
        {
            // Arrange
            var document = VttTestData.CreateVoiceLabelDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("<v Alice>"));
            Assert.IsTrue(result.Contains("<v Bob>"));
            Assert.IsTrue(result.Contains("</v>"));
        }

        [TestMethod]
        public void Write_TextAlreadyHasVoiceTag_DoesNotDoubleWrap()
        {
            // Arrange
            var document = new SubtitleDocument
            {
                Entries = new[]
                {
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.FromSeconds(2),
                        Text = "<v Alice>Already tagged</v>",
                        VoiceLabel = "Alice"
                    }
                }
            };
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            var voiceTagCount = result.Split("<v ").Length - 1;
            Assert.AreEqual(1, voiceTagCount); // Only one voice tag
        }

        #endregion

        #region Formatting Preservation

        [TestMethod]
        public void Write_FormattedDocument_PreservesTags()
        {
            // Arrange
            var document = VttTestData.CreateFormattedDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("<b>tutorial</b>"));
            Assert.IsTrue(result.Contains("<i>video localization</i>"));
            Assert.IsTrue(result.Contains("<u>underlined</u>"));
        }

        #endregion

        #region Multi-line Text

        [TestMethod]
        public void Write_MultiLineDocument_PreservesNewlines()
        {
            // Arrange
            var document = VttTestData.CreateMultiLineDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("first line"));
            Assert.IsTrue(result.Contains("second line"));
            Assert.IsTrue(result.Contains("third line"));
        }

        #endregion

        #region Empty Document

        [TestMethod]
        public void Write_EmptyDocument_GeneratesHeaderOnly()
        {
            // Arrange
            var document = VttTestData.CreateEmptyDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.AreEqual("WEBVTT\r\n\r\n", result);
        }

        #endregion

        #region Stream Behavior

        [TestMethod]
        public void Write_StreamNotClosed_CanReadAfterWrite()
        {
            // Arrange
            var document = VttTestData.CreateSingleEntryDocument();
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
        public void Format_ReturnsVtt()
        {
            Assert.AreEqual("vtt", _writer.Format);
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task WriteAsync_BasicDocument_GeneratesCorrectVtt()
        {
            // Arrange
            var document = VttTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            await _writer.WriteAsync(document, stream);

            // Assert
            var result = VttTestData.StreamToString(stream);
            Assert.IsTrue(result.StartsWith("WEBVTT"));
        }

        #endregion
    }
}
