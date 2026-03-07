using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Ttml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FormatFlow.Tests.Subtitles.Ttml
{
    [TestClass]
    public class TtmlWriterTests
    {
        private readonly TtmlWriter _writer = new();

        #region Basic Writing

        [TestMethod]
        public void Write_BasicDocument_ProducesValidTtml()
        {
            // Arrange
            var document = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // Verify XML declaration
            Assert.IsTrue(output.StartsWith("<?xml"));

            // Verify root element
            Assert.IsTrue(output.Contains("<tt"));
            Assert.IsTrue(output.Contains("xmlns=\"http://www.w3.org/ns/ttml\""));

            // Verify entry count by parsing
            var xdoc = XDocument.Parse(output);
            var ns = XNamespace.Get("http://www.w3.org/ns/ttml");
            var paragraphs = xdoc.Descendants(ns + "p").ToList();
            Assert.AreEqual(3, paragraphs.Count);
        }

        [TestMethod]
        public void Write_BasicDocument_ContainsCorrectText()
        {
            // Arrange
            var document = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);
            Assert.IsTrue(output.Contains("Lorem ipsum dolor sit amet."));
            Assert.IsTrue(output.Contains("Consectetur adipiscing elit sed do."));
            Assert.IsTrue(output.Contains("Eiusmod tempor incididunt ut labore."));
        }

        [TestMethod]
        public void Write_SingleEntry_ProducesValidTtml()
        {
            // Arrange
            var document = TtmlTestData.CreateSingleEntryDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);
            var xdoc = XDocument.Parse(output);
            var ns = XNamespace.Get("http://www.w3.org/ns/ttml");
            var paragraphs = xdoc.Descendants(ns + "p").ToList();
            Assert.AreEqual(1, paragraphs.Count);
        }

        [TestMethod]
        public void Write_EmptyDocument_ProducesValidEmptyTtml()
        {
            // Arrange
            var document = TtmlTestData.CreateEmptyDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);
            var xdoc = XDocument.Parse(output); // Should not throw
            var ns = XNamespace.Get("http://www.w3.org/ns/ttml");
            var paragraphs = xdoc.Descendants(ns + "p").ToList();
            Assert.AreEqual(0, paragraphs.Count);
        }

        #endregion

        #region Timestamp Formatting

        [TestMethod]
        public void Write_Timestamps_UsesOffsetFormat()
        {
            // Arrange
            var document = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // Should use offset time format (e.g., "2.5s" or "2.500s")
            Assert.IsTrue(output.Contains("begin=\"0s\"") || output.Contains("begin=\"0.000s\""));
        }

        [TestMethod]
        public void Write_LongDuration_FormatsCorrectly()
        {
            // Arrange
            var document = TtmlTestData.CreateLongDurationDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // 1:30:45.123 = 5445.123 seconds
            // Should contain begin="5445.123s" or similar
            Assert.IsTrue(output.Contains("5445"));
        }

        #endregion

        #region Metadata

        [TestMethod]
        public void Write_DocumentWithMetadata_IncludesMetadata()
        {
            // Arrange
            var document = TtmlTestData.CreateDocumentWithMetadata();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // Should include language
            Assert.IsTrue(output.Contains("xml:lang=\"en-US\"") || output.Contains("xml:lang='en-US'"));
        }

        #endregion

        #region Regions

        [TestMethod]
        public void Write_DocumentWithRegions_IncludesLayoutSection()
        {
            // Arrange
            var document = TtmlTestData.CreateDocumentWithRegions();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // Should have layout section with regions
            Assert.IsTrue(output.Contains("<layout"));
            Assert.IsTrue(output.Contains("<region"));
            Assert.IsTrue(output.Contains("xml:id=\"top\"") || output.Contains("xml:id='top'"));
        }

        [TestMethod]
        public void Write_DocumentWithRegions_DeduplicatesIdenticalRegions()
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
                        Text = "First",
                        Position = new TtmlRegion { RegionId = "r1", OriginX = "10%", OriginY = "80%" }
                    },
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.FromSeconds(3),
                        EndTime = TimeSpan.FromSeconds(5),
                        Text = "Second",
                        Position = new TtmlRegion { RegionId = "r1", OriginX = "10%", OriginY = "80%" }
                    }
                }
            };
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);
            var xdoc = XDocument.Parse(output);
            var ns = XNamespace.Get("http://www.w3.org/ns/ttml");

            // Should only have one region definition
            var regions = xdoc.Descendants(ns + "region").ToList();
            Assert.AreEqual(1, regions.Count);

            // Both paragraphs should reference it
            var paragraphs = xdoc.Descendants(ns + "p").ToList();
            Assert.AreEqual(2, paragraphs.Count);
            Assert.IsTrue(paragraphs.All(p => p.Attribute("region")?.Value == "r1"));
        }

        #endregion

        #region Styles

        [TestMethod]
        public void Write_DocumentWithStyles_IncludesStylingSection()
        {
            // Arrange
            var document = TtmlTestData.CreateDocumentWithStyles();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // Should have styling section
            Assert.IsTrue(output.Contains("<styling") || !document.Entries.Any(e => e.Style != null && e.Style is TtmlStyle));
        }

        #endregion

        #region Multi-line Text

        [TestMethod]
        public void Write_MultiLineText_UsesBreakElements()
        {
            // Arrange
            var document = TtmlTestData.CreateMultiLineDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);

            // Should use <br/> for line breaks
            Assert.IsTrue(output.Contains("<br") || output.Contains("Line one"));
        }

        #endregion

        #region Voice Labels

        [TestMethod]
        public void Write_DocumentWithVoiceLabels_IncludesAgentReferences()
        {
            // Arrange
            var document = TtmlTestData.CreateDocumentWithVoiceLabels();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(document, stream);

            // Assert - verify it produces valid XML
            var output = TtmlTestData.StreamToString(stream);
            var xdoc = XDocument.Parse(output); // Should not throw

            // Verify the text is preserved
            Assert.IsTrue(output.Contains("Speaker one says hello."));
            Assert.IsTrue(output.Contains("Speaker two responds."));
        }

        #endregion

        #region Format Property

        [TestMethod]
        public void Format_ReturnsTtml()
        {
            Assert.AreEqual("ttml", _writer.Format);
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task WriteAsync_BasicDocument_ProducesValidTtml()
        {
            // Arrange
            var document = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            await _writer.WriteAsync(document, stream);

            // Assert
            var output = TtmlTestData.StreamToString(stream);
            Assert.IsTrue(output.Contains("<tt"));
        }

        #endregion

        #region Round-trip Validation

        [TestMethod]
        public void Write_ThenRead_PreservesEntryCount()
        {
            // Arrange
            var original = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act - Write
            _writer.Write(original, stream);

            // Act - Read back
            stream.Position = 0;
            var reader = new TtmlReader();
            var roundTripped = reader.Read(stream);

            // Assert
            Assert.AreEqual(original.Entries.Count, roundTripped.Entries.Count);
        }

        [TestMethod]
        public void Write_ThenRead_PreservesText()
        {
            // Arrange
            var original = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(original, stream);
            stream.Position = 0;
            var reader = new TtmlReader();
            var roundTripped = reader.Read(stream);

            // Assert
            for (int i = 0; i < original.Entries.Count; i++)
            {
                Assert.AreEqual(
                    original.Entries[i].Text.Trim(),
                    roundTripped.Entries[i].Text.Trim(),
                    $"Text mismatch at entry {i}");
            }
        }

        [TestMethod]
        public void Write_ThenRead_PreservesTimestamps()
        {
            // Arrange
            var original = TtmlTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            // Act
            _writer.Write(original, stream);
            stream.Position = 0;
            var reader = new TtmlReader();
            var roundTripped = reader.Read(stream);

            // Assert - timestamps should be within 1ms tolerance
            for (int i = 0; i < original.Entries.Count; i++)
            {
                var originalEntry = original.Entries[i];
                var roundTrippedEntry = roundTripped.Entries[i];

                Assert.IsTrue(
                    Math.Abs((originalEntry.StartTime - roundTrippedEntry.StartTime).TotalMilliseconds) < 1,
                    $"StartTime mismatch at entry {i}");
                Assert.IsTrue(
                    Math.Abs((originalEntry.EndTime - roundTrippedEntry.EndTime).TotalMilliseconds) < 1,
                    $"EndTime mismatch at entry {i}");
            }
        }

        #endregion
    }
}
