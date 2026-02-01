using FormatFlow.Core.Subtitles.Srt;
using FormatFlow.Core.Subtitles.Vtt;
using FormatFlow.Tests.Subtitles.Srt;

namespace FormatFlow.Tests.Subtitles.Vtt
{
    [TestClass]
    public class VttRoundTripTests
    {
        private readonly VttReader _vttReader = new();
        private readonly VttWriter _vttWriter = new();

        [TestMethod]
        public void RoundTrip_BasicVtt_PreservesAllData()
        {
            // Arrange
            using var inputStream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act - Read
            var document = _vttReader.Read(inputStream);

            // Act - Write
            using var outputStream = new MemoryStream();
            _vttWriter.Write(document, outputStream);

            // Act - Read again
            outputStream.Position = 0;
            var documentAfterRoundTrip = _vttReader.Read(outputStream);

            // Assert
            Assert.AreEqual(document.Entries.Count, documentAfterRoundTrip.Entries.Count);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                var original = document.Entries[i];
                var roundTrip = documentAfterRoundTrip.Entries[i];

                Assert.AreEqual(original.StartTime, roundTrip.StartTime);
                Assert.AreEqual(original.EndTime, roundTrip.EndTime);
                Assert.AreEqual(original.Text, roundTrip.Text);
            }
        }

        [TestMethod]
        public void RoundTrip_PositionedVtt_PreservesPositions()
        {
            // Arrange
            using var inputStream = VttTestData.ToStream(VttTestData.PositionedVtt);

            // Act
            var document = _vttReader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _vttWriter.Write(document, outputStream);
            outputStream.Position = 0;
            var documentAfterRoundTrip = _vttReader.Read(outputStream);

            // Assert
            for (int i = 0; i < document.Entries.Count; i++)
            {
                var original = document.Entries[i].Position as VttPosition;
                var roundTrip = documentAfterRoundTrip.Entries[i].Position as VttPosition;

                Assert.AreEqual(original?.Line, roundTrip?.Line);
                Assert.AreEqual(original?.Position, roundTrip?.Position);
                Assert.AreEqual(original?.HorizontalAlign, roundTrip?.HorizontalAlign);
            }
        }

        [TestMethod]
        public void RoundTrip_VoiceLabelVtt_PreservesVoiceLabels()
        {
            // Arrange
            using var inputStream = VttTestData.ToStream(VttTestData.VoiceLabelVtt);

            // Act
            var document = _vttReader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _vttWriter.Write(document, outputStream);
            outputStream.Position = 0;
            var documentAfterRoundTrip = _vttReader.Read(outputStream);

            // Assert
            for (int i = 0; i < document.Entries.Count; i++)
            {
                Assert.AreEqual(document.Entries[i].VoiceLabel, documentAfterRoundTrip.Entries[i].VoiceLabel);
            }
        }

        [TestMethod]
        public void RoundTrip_FormattedVtt_PreservesTags()
        {
            // Arrange
            using var inputStream = VttTestData.ToStream(VttTestData.FormattedVtt);

            // Act
            var document = _vttReader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _vttWriter.Write(document, outputStream);
            outputStream.Position = 0;
            var documentAfterRoundTrip = _vttReader.Read(outputStream);

            // Assert
            for (int i = 0; i < document.Entries.Count; i++)
            {
                Assert.AreEqual(document.Entries[i].Text, documentAfterRoundTrip.Entries[i].Text);
            }
        }

        [DataTestMethod]
        [DataRow(nameof(VttTestData.BasicVtt))]
        [DataRow(nameof(VttTestData.SingleEntryVtt))]
        [DataRow(nameof(VttTestData.LongDurationVtt))]
        public void RoundTrip_VariousVttFormats_OutputMatchesInput(string testDataProperty)
        {
            // Arrange
            var vttContent = typeof(VttTestData)
                .GetField(testDataProperty)!
                .GetValue(null) as string;

            using var inputStream = VttTestData.ToStream(vttContent!);

            // Act
            var document = _vttReader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _vttWriter.Write(document, outputStream);

            var result = VttTestData.StreamToString(outputStream);

            // Assert
            Assert.AreEqual(vttContent, VttTestData.Normalize(result));
        }
    }

    [TestClass]
    public class CrossFormatConversionTests
    {
        private readonly SrtReader _srtReader = new();
        private readonly SrtWriter _srtWriter = new();
        private readonly VttReader _vttReader = new();
        private readonly VttWriter _vttWriter = new();

        #region VTT to SRT

        [TestMethod]
        public void VttToSrt_BasicConversion_PreservesTimingsAndText()
        {
            // Arrange
            using var vttStream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act - Read VTT
            var document = _vttReader.Read(vttStream);

            // Act - Write as SRT
            using var srtStream = new MemoryStream();
            _srtWriter.Write(document, srtStream);

            // Act - Read SRT back
            srtStream.Position = 0;
            var srtDocument = _srtReader.Read(srtStream);

            // Assert
            Assert.AreEqual(document.Entries.Count, srtDocument.Entries.Count);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                Assert.AreEqual(document.Entries[i].StartTime, srtDocument.Entries[i].StartTime);
                Assert.AreEqual(document.Entries[i].EndTime, srtDocument.Entries[i].EndTime);
                Assert.AreEqual(document.Entries[i].Text, srtDocument.Entries[i].Text);
            }
        }

        [TestMethod]
        public void VttToSrt_FormattedText_PreservesCommonTags()
        {
            // Arrange - VTT with formatting that SRT also supports
            using var vttStream = VttTestData.ToStream(VttTestData.FormattedVtt);

            // Act
            var document = _vttReader.Read(vttStream);
            using var srtStream = new MemoryStream();
            _srtWriter.Write(document, srtStream);

            var result = VttTestData.StreamToString(srtStream);

            // Assert - Common tags should be preserved
            Assert.IsTrue(result.Contains("<b>tutorial</b>"));
            Assert.IsTrue(result.Contains("<i>video localization</i>"));
            Assert.IsTrue(result.Contains("<u>underlined</u>"));
        }

        [TestMethod]
        public void VttToSrt_VttSpecificFeatures_DropsGracefully()
        {
            // Arrange - VTT with position settings (not supported in SRT)
            using var vttStream = VttTestData.ToStream(VttTestData.PositionedVtt);

            // Act
            var document = _vttReader.Read(vttStream);
            using var srtStream = new MemoryStream();
            _srtWriter.Write(document, srtStream);

            // Act - Read back
            srtStream.Position = 0;
            var srtDocument = _srtReader.Read(srtStream);

            // Assert - Text preserved, positions dropped (not supported in SRT)
            Assert.AreEqual(document.Entries.Count, srtDocument.Entries.Count);
            Assert.AreEqual(document.Entries[0].Text, srtDocument.Entries[0].Text);
            // SRT doesn't support positions, so it won't have this metadata
            Assert.IsNull(srtDocument.Entries[0].Position);
        }

        [TestMethod]
        public void VttToSrt_TimestampFormatChange_ConvertsCorrectly()
        {
            // Arrange
            using var vttStream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act
            var document = _vttReader.Read(vttStream);
            using var srtStream = new MemoryStream();
            _srtWriter.Write(document, srtStream);

            var result = VttTestData.StreamToString(srtStream);

            // Assert - SRT uses comma, not dot
            Assert.IsTrue(result.Contains("00:00:00,000")); // SRT format with comma
            Assert.IsFalse(result.Contains("00:00:00.000")); // VTT format with dot
        }

        #endregion

        #region SRT to VTT

        [TestMethod]
        public void SrtToVtt_BasicConversion_PreservesTimingsAndText()
        {
            // Arrange
            using var srtStream = SrtTestData.ToStream(SrtTestData.BasicSrt);

            // Act - Read SRT
            var document = _srtReader.Read(srtStream);

            // Act - Write as VTT
            using var vttStream = new MemoryStream();
            _vttWriter.Write(document, vttStream);

            // Act - Read VTT back
            vttStream.Position = 0;
            var vttDocument = _vttReader.Read(vttStream);

            // Assert
            Assert.AreEqual(document.Entries.Count, vttDocument.Entries.Count);

            for (int i = 0; i < document.Entries.Count; i++)
            {
                Assert.AreEqual(document.Entries[i].StartTime, vttDocument.Entries[i].StartTime);
                Assert.AreEqual(document.Entries[i].EndTime, vttDocument.Entries[i].EndTime);
                Assert.AreEqual(document.Entries[i].Text, vttDocument.Entries[i].Text);
            }
        }

        [TestMethod]
        public void SrtToVtt_AddsWebvttHeader()
        {
            // Arrange
            using var srtStream = SrtTestData.ToStream(SrtTestData.BasicSrt);

            // Act
            var document = _srtReader.Read(srtStream);
            using var vttStream = new MemoryStream();
            _vttWriter.Write(document, vttStream);

            var result = VttTestData.StreamToString(vttStream);

            // Assert
            Assert.IsTrue(result.StartsWith("WEBVTT"));
        }

        [TestMethod]
        public void SrtToVtt_MultiLineText_PreservesNewlines()
        {
            // Arrange
            using var srtStream = SrtTestData.ToStream(SrtTestData.MultiLineSrt);

            // Act
            var document = _srtReader.Read(srtStream);
            using var vttStream = new MemoryStream();
            _vttWriter.Write(document, vttStream);

            vttStream.Position = 0;
            var vttDocument = _vttReader.Read(vttStream);

            // Assert
            Assert.IsTrue(vttDocument.Entries[0].Text.Contains("first line"));
            Assert.IsTrue(vttDocument.Entries[0].Text.Contains("second line"));
        }

        #endregion

        #region Full Conversion Pipeline

        [TestMethod]
        public void FullPipeline_SrtToVttToSrt_PreservesContent()
        {
            // Arrange
            using var originalSrtStream = SrtTestData.ToStream(SrtTestData.BasicSrt);

            // Act - SRT → VTT
            var document = _srtReader.Read(originalSrtStream);
            using var vttStream = new MemoryStream();
            _vttWriter.Write(document, vttStream);

            // Act - VTT → SRT
            vttStream.Position = 0;
            var vttDocument = _vttReader.Read(vttStream);
            using var finalSrtStream = new MemoryStream();
            _srtWriter.Write(vttDocument, finalSrtStream);

            // Act - Read final SRT
            finalSrtStream.Position = 0;
            var finalDocument = _srtReader.Read(finalSrtStream);

            // Assert - Content should match original
            originalSrtStream.Position = 0;
            var originalDocument = _srtReader.Read(originalSrtStream);

            Assert.AreEqual(originalDocument.Entries.Count, finalDocument.Entries.Count);
            for (int i = 0; i < originalDocument.Entries.Count; i++)
            {
                Assert.AreEqual(originalDocument.Entries[i].StartTime, finalDocument.Entries[i].StartTime);
                Assert.AreEqual(originalDocument.Entries[i].EndTime, finalDocument.Entries[i].EndTime);
                Assert.AreEqual(originalDocument.Entries[i].Text, finalDocument.Entries[i].Text);
            }
        }

        #endregion
    }
}
