using FormatFlow.Core.Subtitles;
using FormatFlow.Core.Subtitles.Lrc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormatFlow.Tests.Subtitles.Lrc
{
    /// <summary>
    /// Round-trip tests for LRC.
    ///
    /// True round-trip fidelity is intentionally limited for LRC because the format
    /// has no end times — they are inferred during reading and discarded during writing.
    /// What we can guarantee is that <em>start times</em> and <em>text</em> survive
    /// a write → re-read cycle intact.
    /// </summary>
    [TestClass]
    public class LrcRoundTripTests
    {
        private readonly LrcReader _reader = new();
        private readonly LrcWriter _writer = new();

        // ── Helpers ───────────────────────────────────────────────────────────

        private SubtitleDocument WriteAndReread(SubtitleDocument doc)
        {
            using var stream = new MemoryStream();
            _writer.Write(doc, stream);
            stream.Position = 0;
            return _reader.Read(stream);
        }

        // ── Start-time preservation ───────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_StartTimes_PreservedExactly()
        {
            var original = LrcTestData.CreateBasicDocument();
            var result = WriteAndReread(original);

            // Filter out blank entries that the writer skipped
            var origTimes = original.Entries.Where(e => !string.IsNullOrWhiteSpace(e.Text))
                                              .Select(e => e.StartTime).ToList();
            var resultTimes = result.Entries.Select(e => e.StartTime).ToList();

            CollectionAssert.AreEqual(origTimes, resultTimes);
        }

        [TestMethod]
        public void RoundTrip_StartTimes_RoundedToCentiseconds()
        {
            // A document whose timestamps have sub-centisecond precision should round-trip
            // with centisecond accuracy (loss of at most 9 ms).
            var doc = new SubtitleDocument
            {
                Metadata = new SubtitleMetadata(),
                Entries =
                [
                    new SubtitleEntry
                    {
                        StartTime = TimeSpan.FromMilliseconds(7_295), // 7.295 s
                        EndTime   = TimeSpan.FromSeconds(9),
                        Text      = "Centisecond precision"
                    }
                ]
            };

            var result = WriteAndReread(doc);

            // 7295 ms → written as [00:07.29] (centiseconds truncated) → read back as 7290 ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(7_290), result.Entries[0].StartTime);
        }

        // ── Text preservation ─────────────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_Text_PreservedExactly()
        {
            var original = LrcTestData.CreateBasicDocument();
            var result = WriteAndReread(original);

            var origTexts = original.Entries.Where(e => !string.IsNullOrWhiteSpace(e.Text))
                                              .Select(e => e.Text).ToList();
            var resultTexts = result.Entries.Select(e => e.Text).ToList();

            CollectionAssert.AreEqual(origTexts, resultTexts);
        }

        // ── Metadata preservation ─────────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_Title_Preserved()
        {
            var original = LrcTestData.CreateDocumentWithCustomProperties();
            var result = WriteAndReread(original);
            Assert.AreEqual(original.Metadata.Title, result.Metadata.Title);
        }

        [TestMethod]
        public void RoundTrip_ArtistAndAlbum_Preserved()
        {
            var original = LrcTestData.CreateDocumentWithCustomProperties();
            var result = WriteAndReread(original);

            Assert.IsTrue(result.Metadata.CustomProperties.TryGetValue("ar", out var artist));
            Assert.AreEqual("{3723F51C-A90D-4F3E-8D92-CF94E7A54B3F}", artist);

            Assert.IsTrue(result.Metadata.CustomProperties.TryGetValue("al", out var album));
            Assert.AreEqual("{15EF5C4E-5ABB-4A80-A721-30000DF7752A}", album);
        }

        // ── Blank-entry suppression ───────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_BlankEntries_NotReproduced()
        {
            var original = LrcTestData.CreateDocumentWithBlankEntries();
            var result = WriteAndReread(original);

            // Blank entries are dropped by the writer; result should have only non-blank text.
            Assert.IsTrue(result.Entries.All(e => !string.IsNullOrWhiteSpace(e.Text)));
        }

        // ── Full realistic file ───────────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_RealisticFile_AllStartTimesPreserved()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.Realistic);
            var original = _reader.Read(stream);
            var result = WriteAndReread(original);

            var origTimes = original.Entries.Select(e => e.StartTime).ToList();
            var resultTimes = result.Entries.Select(e => e.StartTime).ToList();

            // Allow centisecond rounding (≤ 9 ms difference per entry)
            Assert.AreEqual(origTimes.Count, resultTimes.Count);
            for (int i = 0; i < origTimes.Count; i++)
            {
                var diff = Math.Abs((origTimes[i] - resultTimes[i]).TotalMilliseconds);
                Assert.IsTrue(diff <= 9, $"Entry {i} time drift {diff} ms exceeds centisecond tolerance");
            }
        }
    }
}
