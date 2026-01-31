using FormatFlow.Core.Subtitles.Srt;

namespace FormatFlow.Tests.Subtitles
{
    [TestClass]
    public class SrtRoundTripTests
    {
        private readonly SrtReader _reader = new();
        private readonly SrtWriter _writer = new();

        [TestMethod]
        public void RoundTrip_BasicSrt_PreservesAllData()
        {
            // Arrange
            using var inputStream = SrtTestData.ToStream(SrtTestData.BasicSrt);

            // Act - Read
            var document = _reader.Read(inputStream);

            // Act - Write
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);

            // Act - Read again
            outputStream.Position = 0;
            var documentAfterRoundTrip = _reader.Read(outputStream);

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
        public void RoundTrip_MultiLineSrt_PreservesNewlines()
        {
            // Arrange
            using var inputStream = SrtTestData.ToStream(SrtTestData.MultiLineSrt);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);

            var result = SrtTestData.StreamToString(outputStream);

            // Assert
            Assert.AreEqual(SrtTestData.MultiLineSrt, result);
        }

        [DataTestMethod]
        [DataRow(nameof(SrtTestData.BasicSrt))]
        [DataRow(nameof(SrtTestData.MultiLineSrt))]
        [DataRow(nameof(SrtTestData.SingleEntrySrt))]
        [DataRow(nameof(SrtTestData.LongDurationSrt))]
        public void RoundTrip_VariousSrtFormats_OutputMatchesInput(string testDataProperty)
        {
            // Arrange - Use reflection to get test data dynamically
            var srtContent = typeof(SrtTestData)
                .GetField(testDataProperty)!
                .GetValue(null) as string;

            using var inputStream = SrtTestData.ToStream(srtContent!);

            // Act
            var document = _reader.Read(inputStream);
            using var outputStream = new MemoryStream();
            _writer.Write(document, outputStream);

            var result = SrtTestData.StreamToString(outputStream);

            // Assert
            Assert.AreEqual(srtContent, result);
        }
    }
}
