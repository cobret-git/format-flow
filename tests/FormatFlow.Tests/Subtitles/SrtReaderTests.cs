using FormatFlow.Core.Subtitles.Srt;

namespace FormatFlow.Tests.Subtitles
{
    [TestClass]
    public class SrtReaderTests
    {
        private readonly SrtReader _reader = new();

        [TestMethod]
        public void Read_BasicSrt_ParsesCorrectly()
        {
            // Arrange
            using var stream = SrtTestData.ToStream(SrtTestData.BasicSrt);

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
        public void Read_MultiLineText_PreservesNewlines()
        {
            // Arrange
            using var stream = SrtTestData.ToStream(SrtTestData.MultiLineSrt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            var expectedText = "This is the first line.\r\nThis is the second line.\r\nThis is the third line.";
            Assert.AreEqual(expectedText, entry.Text);
        }

        [TestMethod]
        public void Read_LargeTimestamps_ParsesCorrectly()
        {
            // Arrange
            using var stream = SrtTestData.ToStream(SrtTestData.LongDurationSrt);

            // Act
            var document = _reader.Read(stream);

            // Assert
            var entry = document.Entries[0];
            Assert.AreEqual(new TimeSpan(0, 1, 30, 45, 123), entry.StartTime);
            Assert.AreEqual(new TimeSpan(0, 1, 30, 50, 999), entry.EndTime);
        }

        [TestMethod]
        public void Read_EmptyStream_ReturnsEmptyDocument()
        {
            // Arrange
            using var stream = SrtTestData.ToStream("");

            // Act
            var document = _reader.Read(stream);

            // Assert
            Assert.AreEqual(0, document.Entries.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Read_MalformedTimestamp_ThrowsFormatException()
        {
            // Arrange
            const string malformed = """
            1
            invalid timestamp format
            Some text
            
            """;
            using var stream = SrtTestData.ToStream(malformed);

            // Act
            _reader.Read(stream); // Should throw
        }
    }
}
