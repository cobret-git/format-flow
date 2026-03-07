using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Lrc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles.Lrc
{
    [TestClass]
    public class LrcWriterTests
    {
        private LrcWriter _writer = null!;

        [TestInitialize]
        public void Setup() => _writer = new LrcWriter();

        // ── Format property ───────────────────────────────────────────────────

        [TestMethod]
        public void Format_ReturnsLrc()
        {
            Assert.AreEqual("lrc", _writer.Format);
        }

        // ── Header output ─────────────────────────────────────────────────────

        [TestMethod]
        public void Write_DocumentWithTitle_EmitsTitleTag()
        {
            var doc = LrcTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[ti:Test Song]"));
        }

        [TestMethod]
        public void Write_DocumentWithArtist_EmitsArtistTag()
        {
            var doc = LrcTestData.CreateDocumentWithCustomProperties();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[ar:{3723F51C-A90D-4F3E-8D92-CF94E7A54B3F}]"));
        }

        [TestMethod]
        public void Write_DocumentWithAlbum_EmitsAlbumTag()
        {
            var doc = LrcTestData.CreateDocumentWithCustomProperties();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[al:{15EF5C4E-5ABB-4A80-A721-30000DF7752A}]"));
        }

        [TestMethod]
        public void Write_AlwaysEmitsOffsetZero()
        {
            var doc = LrcTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[offset:0]"));
        }

        [TestMethod]
        public void Write_HeaderTagsInConventionalOrder()
        {
            // Expected order: ti → ar → al → offset
            var doc = LrcTestData.CreateDocumentWithCustomProperties();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            var tiIndex = result.IndexOf("[ti:", StringComparison.Ordinal);
            var arIndex = result.IndexOf("[ar:", StringComparison.Ordinal);
            var alIndex = result.IndexOf("[al:", StringComparison.Ordinal);
            var offsetIndex = result.IndexOf("[offset:", StringComparison.Ordinal);

            Assert.IsTrue(tiIndex < arIndex, "[ti] must precede [ar]");
            Assert.IsTrue(arIndex < alIndex, "[ar] must precede [al]");
            Assert.IsTrue(alIndex < offsetIndex, "[al] must precede [offset]");
        }

        [TestMethod]
        public void Write_DocumentWithoutTitle_NoTitleTag()
        {
            var doc = LrcTestData.CreateEmptyDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsFalse(result.Contains("[ti:"));
        }

        // ── Timestamp formatting ──────────────────────────────────────────────

        [TestMethod]
        public void Write_Timestamp_UsesCentisecondFormat()
        {
            // StartTime = 00:07.00 → [00:07.00]
            var doc = LrcTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[00:07.00]"), $"Expected [00:07.00] in:\n{result}");
        }

        [TestMethod]
        public void Write_LongTrack_MinutesExceedSixty()
        {
            var doc = LrcTestData.CreateLongDurationDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[125:30.00]"), $"Expected [125:30.00] in:\n{result}");
        }

        [TestMethod]
        public void Write_MillisecondPrecision_TruncatesToCentiseconds()
        {
            // 9999 ms → 9 seconds + 999 ms → centiseconds = 99 (truncated)
            var doc = new SubtitleDocument
            {
                Metadata = new SubtitleMetadata(),
                Entries =
                [
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.FromMilliseconds(9999),
                        EndTime   = TimeSpan.FromSeconds(15),
                        Text      = "Truncation test"
                    }
                ]
            };
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[00:09.99]"), $"Expected [00:09.99] in:\n{result}");
        }

        // ── Blank entry handling ──────────────────────────────────────────────

        [TestMethod]
        public void Write_BlankTextEntries_SkippedInOutput()
        {
            var doc = LrcTestData.CreateDocumentWithBlankEntries();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            // Only two timed lines should appear (First line and Third line)
            var timedLines = result.Split('\n')
                .Where(l => System.Text.RegularExpressions.Regex.IsMatch(l, @"^\[\d+:\d{2}\.\d{2}\]"))
                .ToList();

            Assert.AreEqual(2, timedLines.Count);
        }

        // ── Empty document ────────────────────────────────────────────────────

        [TestMethod]
        public void Write_EmptyDocument_EmitsOnlyHeader()
        {
            var doc = LrcTestData.CreateEmptyDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[offset:0]"));

            var timedLines = result.Split('\n')
                .Where(l => System.Text.RegularExpressions.Regex.IsMatch(l, @"^\[\d+:\d{2}\.\d{2}\]"))
                .ToList();

            Assert.AreEqual(0, timedLines.Count);
        }

        // ── Stream behaviour ──────────────────────────────────────────────────

        [TestMethod]
        public void Write_StreamNotClosed_CanReadAfterWrite()
        {
            var doc = LrcTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            _writer.Write(doc, stream);

            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanWrite);
        }

        // ── Async ─────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task WriteAsync_BasicDocument_ContainsTitleTag()
        {
            var doc = LrcTestData.CreateBasicDocument();
            using var stream = new MemoryStream();

            await _writer.WriteAsync(doc, stream);

            var result = LrcTestData.StreamToString(stream);
            Assert.IsTrue(result.Contains("[ti:Test Song]"));
        }
    }
}
