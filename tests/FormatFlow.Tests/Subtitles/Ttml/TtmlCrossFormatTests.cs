using FormatFlow.Core.Subtitles.Sbv;
using FormatFlow.Core.Subtitles.Srt;
using FormatFlow.Core.Subtitles.Ttml;
using FormatFlow.Core.Subtitles.Vtt;
using FormatFlow.Tests.Subtitles.Sbv;
using FormatFlow.Tests.Subtitles.Srt;
using FormatFlow.Tests.Subtitles.Vtt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles.Ttml
{
    /// <summary>
    /// Tests for converting between TTML and other subtitle formats.
    /// Validates the canonical domain model pattern where all formats
    /// convert through SubtitleDocument.
    /// </summary>
    [TestClass]
    public class TtmlCrossFormatTests
    {
        private readonly TtmlReader _ttmlReader = new();
        private readonly TtmlWriter _ttmlWriter = new();
        private readonly VttReader _vttReader = new();
        private readonly VttWriter _vttWriter = new();
        private readonly SrtReader _srtReader = new();
        private readonly SrtWriter _srtWriter = new();
        private readonly SbvReader _sbvReader = new();
        private readonly SbvWriter _sbvWriter = new();

        #region TTML -> VTT

        [TestMethod]
        public void Convert_TtmlToVtt_PreservesBasicContent()
        {
            // Arrange
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);

            // Act - Read TTML
            var document = _ttmlReader.Read(ttmlStream);

            // Act - Write VTT
            using var vttStream = new MemoryStream();
            _vttWriter.Write(document, vttStream);

            // Assert
            var vttContent = TtmlTestData.StreamToString(vttStream);
            Assert.IsTrue(vttContent.StartsWith("WEBVTT"));
            Assert.IsTrue(vttContent.Contains("Lorem ipsum dolor sit amet."));
            Assert.IsTrue(vttContent.Contains("Consectetur adipiscing elit sed do."));
        }

        [TestMethod]
        public void Convert_TtmlToVtt_PreservesTimestamps()
        {
            // Arrange
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var vttStream = new MemoryStream();
            _vttWriter.Write(ttmlDoc, vttStream);
            vttStream.Position = 0;
            var vttDoc = _vttReader.Read(vttStream);

            // Assert
            Assert.AreEqual(ttmlDoc.Entries.Count, vttDoc.Entries.Count);

            for (int i = 0; i < ttmlDoc.Entries.Count; i++)
            {
                Assert.AreEqual(ttmlDoc.Entries[i].StartTime, vttDoc.Entries[i].StartTime,
                    $"StartTime mismatch at entry {i}");
                Assert.AreEqual(ttmlDoc.Entries[i].EndTime, vttDoc.Entries[i].EndTime,
                    $"EndTime mismatch at entry {i}");
            }
        }

        [TestMethod]
        public void Convert_TtmlWithRegions_ToVtt_DropsRegions()
        {
            // Arrange - TTML regions don't map directly to VTT
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.RegionedTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var vttStream = new MemoryStream();
            _vttWriter.Write(ttmlDoc, vttStream);
            vttStream.Position = 0;
            var vttDoc = _vttReader.Read(vttStream);

            // Assert - content preserved, regions may be lost or converted
            Assert.AreEqual(3, vttDoc.Entries.Count);
            Assert.AreEqual("Positioned at top center.", vttDoc.Entries[0].Text);
        }

        #endregion

        #region VTT -> TTML

        [TestMethod]
        public void Convert_VttToTtml_PreservesBasicContent()
        {
            // Arrange
            using var vttStream = VttTestData.ToStream(VttTestData.BasicVtt);

            // Act - Read VTT
            var document = _vttReader.Read(vttStream);

            // Act - Write TTML
            using var ttmlStream = new MemoryStream();
            _ttmlWriter.Write(document, ttmlStream);

            // Assert
            var ttmlContent = TtmlTestData.StreamToString(ttmlStream);
            Assert.IsTrue(ttmlContent.Contains("<tt"));
            Assert.IsTrue(ttmlContent.Contains("Welcome to the Example Subtitle File!"));
        }

        [TestMethod]
        public void Convert_VttToTtml_PreservesTimestamps()
        {
            // Arrange
            using var vttStream = VttTestData.ToStream(VttTestData.BasicVtt);
            var vttDoc = _vttReader.Read(vttStream);

            // Act
            using var ttmlStream = new MemoryStream();
            _ttmlWriter.Write(vttDoc, ttmlStream);
            ttmlStream.Position = 0;
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Assert
            Assert.AreEqual(vttDoc.Entries.Count, ttmlDoc.Entries.Count);

            for (int i = 0; i < vttDoc.Entries.Count; i++)
            {
                // 1ms tolerance for floating point conversion
                Assert.IsTrue(
                    Math.Abs((vttDoc.Entries[i].StartTime - ttmlDoc.Entries[i].StartTime).TotalMilliseconds) < 1,
                    $"StartTime mismatch at entry {i}");
            }
        }

        [TestMethod]
        public void Convert_VttWithPosition_ToTtml_PreservesContent()
        {
            // Arrange
            using var vttStream = VttTestData.ToStream(VttTestData.PositionedVtt);
            var vttDoc = _vttReader.Read(vttStream);

            // Act
            using var ttmlStream = new MemoryStream();
            _ttmlWriter.Write(vttDoc, ttmlStream);
            ttmlStream.Position = 0;
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Assert - text content preserved
            Assert.AreEqual(3, ttmlDoc.Entries.Count);
            Assert.AreEqual("Centered at top", ttmlDoc.Entries[0].Text);
        }

        #endregion

        #region TTML -> SRT

        [TestMethod]
        public void Convert_TtmlToSrt_PreservesBasicContent()
        {
            // Arrange
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var srtStream = new MemoryStream();
            _srtWriter.Write(ttmlDoc, srtStream);

            // Assert
            var srtContent = TtmlTestData.StreamToString(srtStream);
            Assert.IsTrue(srtContent.Contains("1\r\n") || srtContent.Contains("1\n"));
            Assert.IsTrue(srtContent.Contains("Lorem ipsum dolor sit amet."));
            Assert.IsTrue(srtContent.Contains("-->")); // SRT arrow
        }

        [TestMethod]
        public void Convert_TtmlToSrt_UsesCommaForMilliseconds()
        {
            // Arrange
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var srtStream = new MemoryStream();
            _srtWriter.Write(ttmlDoc, srtStream);

            // Assert - SRT uses comma, not dot for milliseconds
            var srtContent = TtmlTestData.StreamToString(srtStream);
            Assert.IsTrue(srtContent.Contains(","), "SRT should use comma for millisecond separator");
        }

        [TestMethod]
        public void Convert_TtmlWithAgents_ToSrt_DropsVoiceLabels()
        {
            // Arrange - SRT doesn't support voice labels
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.AgentTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var srtStream = new MemoryStream();
            _srtWriter.Write(ttmlDoc, srtStream);
            srtStream.Position = 0;
            var srtDoc = _srtReader.Read(srtStream);

            // Assert - content preserved, voice labels dropped
            Assert.AreEqual(3, srtDoc.Entries.Count);
            Assert.IsNull(srtDoc.Entries[0].VoiceLabel);
            Assert.AreEqual("Speaker one says hello.", srtDoc.Entries[0].Text);
        }

        #endregion

        #region SRT -> TTML

        [TestMethod]
        public void Convert_SrtToTtml_PreservesContent()
        {
            // Arrange
            using var srtStream = SrtTestData.ToStream(SrtTestData.BasicSrt);
            var srtDoc = _srtReader.Read(srtStream);

            // Act
            using var ttmlStream = new MemoryStream();
            _ttmlWriter.Write(srtDoc, ttmlStream);
            ttmlStream.Position = 0;
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Assert
            Assert.AreEqual(srtDoc.Entries.Count, ttmlDoc.Entries.Count);

            for (int i = 0; i < srtDoc.Entries.Count; i++)
            {
                Assert.AreEqual(srtDoc.Entries[i].Text.Trim(), ttmlDoc.Entries[i].Text.Trim());
            }
        }

        #endregion

        #region TTML -> SBV

        [TestMethod]
        public void Convert_TtmlToSbv_PreservesBasicContent()
        {
            // Arrange
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var sbvStream = new MemoryStream();
            _sbvWriter.Write(ttmlDoc, sbvStream);

            // Assert
            var sbvContent = TtmlTestData.StreamToString(sbvStream);
            Assert.IsTrue(sbvContent.Contains("Lorem ipsum dolor sit amet."));
            Assert.IsTrue(sbvContent.Contains(",")); // SBV uses comma in timestamps
        }

        [TestMethod]
        public void Convert_TtmlToSbv_DropsFormatting()
        {
            // Arrange - SBV doesn't support styling
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.StyledTtml);
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act
            using var sbvStream = new MemoryStream();
            _sbvWriter.Write(ttmlDoc, sbvStream);
            sbvStream.Position = 0;
            var sbvDoc = _sbvReader.Read(sbvStream);

            // Assert - text content preserved (tags may be stripped or preserved depending on implementation)
            Assert.AreEqual(ttmlDoc.Entries.Count, sbvDoc.Entries.Count);
        }

        #endregion

        #region SBV -> TTML

        [TestMethod]
        public void Convert_SbvToTtml_PreservesContent()
        {
            // Arrange
            using var sbvStream = SbvTestData.ToStream(SbvTestData.BasicSbv);
            var sbvDoc = _sbvReader.Read(sbvStream);

            // Act
            using var ttmlStream = new MemoryStream();
            _ttmlWriter.Write(sbvDoc, ttmlStream);
            ttmlStream.Position = 0;
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Assert
            Assert.AreEqual(sbvDoc.Entries.Count, ttmlDoc.Entries.Count);

            for (int i = 0; i < sbvDoc.Entries.Count; i++)
            {
                Assert.AreEqual(sbvDoc.Entries[i].Text.Trim(), ttmlDoc.Entries[i].Text.Trim());
            }
        }

        #endregion

        #region Full Chain Conversions

        [TestMethod]
        public void Convert_TtmlToVttToSrt_PreservesContent()
        {
            // Arrange
            using var ttmlStream = TtmlTestData.ToStream(TtmlTestData.BasicTtml);
            var originalDoc = _ttmlReader.Read(ttmlStream);

            // Act - TTML -> VTT
            using var vttStream = new MemoryStream();
            _vttWriter.Write(originalDoc, vttStream);
            vttStream.Position = 0;
            var vttDoc = _vttReader.Read(vttStream);

            // Act - VTT -> SRT
            using var srtStream = new MemoryStream();
            _srtWriter.Write(vttDoc, srtStream);
            srtStream.Position = 0;
            var finalDoc = _srtReader.Read(srtStream);

            // Assert
            Assert.AreEqual(originalDoc.Entries.Count, finalDoc.Entries.Count);

            for (int i = 0; i < originalDoc.Entries.Count; i++)
            {
                Assert.AreEqual(
                    originalDoc.Entries[i].Text.Trim(),
                    finalDoc.Entries[i].Text.Trim(),
                    $"Text mismatch at entry {i}");
            }
        }

        [TestMethod]
        public void Convert_SrtToTtmlToVtt_PreservesTimestamps()
        {
            // Arrange
            using var srtStream = SrtTestData.ToStream(SrtTestData.BasicSrt);
            var originalDoc = _srtReader.Read(srtStream);

            // Act - SRT -> TTML
            using var ttmlStream = new MemoryStream();
            _ttmlWriter.Write(originalDoc, ttmlStream);
            ttmlStream.Position = 0;
            var ttmlDoc = _ttmlReader.Read(ttmlStream);

            // Act - TTML -> VTT
            using var vttStream = new MemoryStream();
            _vttWriter.Write(ttmlDoc, vttStream);
            vttStream.Position = 0;
            var finalDoc = _vttReader.Read(vttStream);

            // Assert - timestamps within 1ms tolerance
            Assert.AreEqual(originalDoc.Entries.Count, finalDoc.Entries.Count);

            for (int i = 0; i < originalDoc.Entries.Count; i++)
            {
                Assert.IsTrue(
                    Math.Abs((originalDoc.Entries[i].StartTime - finalDoc.Entries[i].StartTime).TotalMilliseconds) < 1,
                    $"StartTime mismatch at entry {i}");
            }
        }

        #endregion
    }
}
