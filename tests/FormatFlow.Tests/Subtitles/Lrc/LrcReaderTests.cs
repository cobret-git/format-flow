using FormatFlow.Core.Subtitles.Lrc;

namespace FormatFlow.Tests.Subtitles.Lrc
{
    [TestClass]
    public class LrcReaderTests
    {
        private LrcReader _reader = null!;

        [TestInitialize]
        public void Setup() => _reader = new LrcReader();

        // ── Format property ───────────────────────────────────────────────────

        [TestMethod]
        public void Format_ReturnsLrc()
        {
            Assert.AreEqual("lrc", _reader.Format);
        }

        // ── Header parsing ────────────────────────────────────────────────────

        [TestMethod]
        public void Read_TitleTag_PopulatesMetadataTitle()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);
            Assert.AreEqual("{496BDFC8-45CB-4264-9187-D9930B04955D}", doc.Metadata.Title);
        }

        [TestMethod]
        public void Read_ArtistTag_StoredInCustomProperties()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);
            Assert.IsTrue(doc.Metadata.CustomProperties.TryGetValue("ar", out var artist));
            Assert.AreEqual("{3723F51C-A90D-4F3E-8D92-CF94E7A54B3F}", artist);
        }

        [TestMethod]
        public void Read_AlbumTag_StoredInCustomProperties()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);
            Assert.IsTrue(doc.Metadata.CustomProperties.TryGetValue("al", out var album));
            Assert.AreEqual("{15EF5C4E-5ABB-4A80-A721-30000DF7752A}", album);
        }

        [TestMethod]
        public void Read_OffsetZero_DoesNotShiftTimestamps()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);

            // [00:07.29] = 7290 ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(7290), doc.Entries[0].StartTime);
        }

        [TestMethod]
        public void Read_NoHeaders_ReturnsNullTitle()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SingleLine);
            var doc = _reader.Read(stream);
            Assert.IsNull(doc.Metadata.Title);
        }

        // ── Timed line parsing ────────────────────────────────────────────────

        [TestMethod]
        public void Read_SimpleFile_CorrectEntryCount()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);
            Assert.AreEqual(2, doc.Entries.Count);
        }

        [TestMethod]
        public void Read_SimpleFile_CorrectText()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);
            Assert.AreEqual("{5CCB93E3-40E5-4DB7-B9AD-B1E5BA446F6E}", doc.Entries[0].Text);
            Assert.AreEqual("{8D0F37A7-47F3-4502-9BF2-FCA11787E001}", doc.Entries[1].Text);
        }

        [TestMethod]
        public void Read_SingleLine_CorrectStartTime()
        {
            // [00:05.00] = 5000 ms
            using var stream = LrcTestData.ToStream(LrcTestData.SingleLine);
            var doc = _reader.Read(stream);
            Assert.AreEqual(TimeSpan.FromSeconds(5), doc.Entries[0].StartTime);
        }

        [TestMethod]
        public void Read_CentisecondTimestamp_ParsedCorrectly()
        {
            // [00:07.29] should be 7 seconds + 29 centiseconds = 7290 ms
            using var stream = LrcTestData.ToStream("[00:07.29]Test\n");
            var doc = _reader.Read(stream);
            Assert.AreEqual(TimeSpan.FromMilliseconds(7290), doc.Entries[0].StartTime);
        }

        [TestMethod]
        public void Read_MillisecondTimestamp_ParsedCorrectly()
        {
            // [00:05.250] (3 digits) = 5250 ms
            using var stream = LrcTestData.ToStream(LrcTestData.WithMillisecondTimestamps);
            var doc = _reader.Read(stream);
            Assert.AreEqual(TimeSpan.FromMilliseconds(5250), doc.Entries[0].StartTime);
        }

        // ── Multi-timestamp expansion ─────────────────────────────────────────

        [TestMethod]
        public void Read_MultipleTimestampsOnOneLine_ExpandsToSeparateEntries()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.MultiTimestampLine);
            var doc = _reader.Read(stream);
            Assert.AreEqual(3, doc.Entries.Count);
        }

        [TestMethod]
        public void Read_MultipleTimestampsOnOneLine_AllHaveSameText()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.MultiTimestampLine);
            var doc = _reader.Read(stream);
            Assert.IsTrue(doc.Entries.All(e => e.Text == "{73C88ACC-98A7-44FE-9D91-C9756A27FF20}"));
        }

        [TestMethod]
        public void Read_MultipleTimestampsOnOneLine_SortedByStartTime()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.MultiTimestampLine);
            var doc = _reader.Read(stream);
            var times = doc.Entries.Select(e => e.StartTime).ToList();
            CollectionAssert.AreEqual(times.OrderBy(t => t).ToList(), times);
        }

        // ── End time inference ────────────────────────────────────────────────

        [TestMethod]
        public void Read_TwoEntries_FirstEndTimeEqualsSecondStartTime()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = _reader.Read(stream);
            Assert.AreEqual(doc.Entries[1].StartTime, doc.Entries[0].EndTime);
        }

        [TestMethod]
        public void Read_LastEntry_GetsDefaultDuration()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SingleLine);
            var doc = _reader.Read(stream);
            var entry = doc.Entries[0];
            Assert.IsTrue(entry.EndTime > entry.StartTime,
                "Last entry EndTime must be after StartTime");
            // Default is 5 seconds
            Assert.AreEqual(entry.StartTime + TimeSpan.FromSeconds(5), entry.EndTime);
        }

        // ── Offset handling ───────────────────────────────────────────────────

        [TestMethod]
        public void Read_PositiveOffset_ShiftsTimestampsForward()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.WithPositiveOffset);
            var doc = _reader.Read(stream);
            // [00:10.00] + 500 ms offset = 10500 ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(10_500), doc.Entries[0].StartTime);
        }

        [TestMethod]
        public void Read_NegativeOffsetExceedingTimestamp_ClampsToZero()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.WithNegativeOffset);
            var doc = _reader.Read(stream);
            // [00:02.00] - 5000 ms = -3000 ms → clamped to 0
            Assert.AreEqual(TimeSpan.Zero, doc.Entries[0].StartTime);
        }

        // ── Blank/empty entries ───────────────────────────────────────────────

        [TestMethod]
        public void Read_BlankTextEntry_IncludedInEntries()
        {
            // The blank entry contributes to timing even if it has no visible text.
            using var stream = LrcTestData.ToStream(LrcTestData.WithBlankEntry);
            var doc = _reader.Read(stream);
            Assert.AreEqual(3, doc.Entries.Count);
        }

        [TestMethod]
        public void Read_BlankTextEntry_EndTimeOfPrecedingEntryIsBlankStartTime()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.WithBlankEntry);
            var doc = _reader.Read(stream);
            // Entry 0 ("First line") ends when the blank entry starts
            Assert.AreEqual(doc.Entries[1].StartTime, doc.Entries[0].EndTime);
        }

        // ── Robustness ────────────────────────────────────────────────────────

        [TestMethod]
        public void Read_EmptyStream_ReturnsEmptyDocument()
        {
            using var stream = LrcTestData.ToStream("");
            var doc = _reader.Read(stream);
            Assert.AreEqual(0, doc.Entries.Count);
        }

        [TestMethod]
        public void Read_StreamLeftOpen_CanReadAfterParse()
        {
            using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(LrcTestData.SingleLine));
            _reader.Read(ms);
            Assert.IsTrue(ms.CanRead);
        }

        [TestMethod]
        public void Read_UnknownTags_IgnoredGracefully()
        {
            const string lrc = "[xx:unknown tag]\n[00:05.00]Hello\n";
            using var stream = LrcTestData.ToStream(lrc);
            var doc = _reader.Read(stream);
            Assert.AreEqual(1, doc.Entries.Count);
        }

        // ── Async ─────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task ReadAsync_SimpleFile_ReturnsCorrectEntries()
        {
            using var stream = LrcTestData.ToStream(LrcTestData.SimpleWithHeaders);
            var doc = await _reader.ReadAsync(stream);
            Assert.AreEqual(2, doc.Entries.Count);
        }
    }
}
