using FormatFlow.Core.Subtitles.Srt;

namespace FormatFlow.Tests.Subtitles
{
    [TestClass]
    public class SrtWriterTests
    {
        private readonly SrtWriter _writer = new();

        [TestMethod]
        public void Write_BasicDocument_GeneratesCorrectSrt()
        {
            // Arrange
            var document = SrtTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SrtTestData.StreamToString(stream);
            Assert.AreEqual(SrtTestData.BasicSrt, result);
        }

        [TestMethod]
        public void Write_MultiLineDocument_PreservesNewlines()
        {
            // Arrange
            var document = SrtTestData.CreateMultiLineDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SrtTestData.StreamToString(stream);
            Assert.AreEqual(SrtTestData.MultiLineSrt, result);
        }

        [TestMethod]
        public void Write_SingleEntry_NoTrailingEmptyLine()
        {
            // Arrange
            var document = SrtTestData.CreateSingleEntryDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SrtTestData.StreamToString(stream);
            Assert.AreEqual(SrtTestData.SingleEntrySrt, result);
        }

        [TestMethod]
        public void Write_LargeTimestamps_FormatsCorrectly()
        {
            // Arrange
            var document = SrtTestData.CreateLongDurationDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SrtTestData.StreamToString(stream);
            Assert.AreEqual(SrtTestData.LongDurationSrt, result);
        }

        [TestMethod]
        public void Write_EmptyDocument_GeneratesEmptyFile()
        {
            // Arrange
            var document = SrtTestData.CreateEmptyDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var result = SrtTestData.StreamToString(stream);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void Write_StreamNotClosed_CanReadAfterWrite()
        {
            // Arrange
            var document = SrtTestData.CreateSingleEntryDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert - stream should still be open
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanWrite);
        }
    }
}
