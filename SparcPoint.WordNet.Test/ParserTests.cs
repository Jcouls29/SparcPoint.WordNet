using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SparcPoint.WordNet;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.Generic;
using System.Linq;

namespace SparcPoint.WordNet.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ParseSenseKey_175th()
        {
            string key = "175th%5:00:00:ordinal:00";
            SenseKey senseKey = SenseKey.Parse(key);

            Assert.AreEqual("175th", senseKey.Lemma);
            Assert.AreEqual(Constants.PartOfSpeech.ADJECTIVE_SATELLITE, senseKey.PartOfSpeech);
            Assert.AreEqual(Constants.LexicographerFiles.ADJ_ALL, senseKey.LexFile);
            Assert.AreEqual("ordinal", senseKey.HeadWord);
            Assert.AreEqual((byte)0, senseKey.HeadId);
        }

        [TestMethod]
        public void ParseSenseKey_Abacus()
        {
            string key = "abacus%1:06:01::";
            SenseKey senseKey = SenseKey.Parse(key);

            Assert.AreEqual("abacus", senseKey.Lemma);
            Assert.AreEqual(Constants.PartOfSpeech.NOUN, senseKey.PartOfSpeech);
            Assert.AreEqual(Constants.LexicographerFiles.NOUN_ARTIFACT, senseKey.LexFile);
            Assert.AreEqual(string.Empty, senseKey.HeadWord);
            Assert.AreEqual((byte)0, senseKey.HeadId);
        }

        [TestMethod]
        public void ParseSenseKey_PerformanceMeasure()
        {
            string key = "abdicable%5:00:00:unwanted:00";

            Stopwatch sw = Stopwatch.StartNew();
            for (var i = 1; i <= 1000000; i++)
            {
                SenseKey senseKey = SenseKey.Parse(key);
            }
            sw.Stop();

            Debug.WriteLine($"Parse Sense Key Performance (per million): Time Elapsed = {sw.Elapsed.ToString()}");
        }

        [TestMethod]
        public async Task ParseSenseIndexFile_IrritableSearch()
        {
            StorageFile senseFile = await FileRetriever.GetSenseIndexFile();
            SenseIndex index = await SenseIndex.ParseFileAsync(senseFile);

            IEnumerable<SenseIndexEntry> found = index.SearchDictionary["irritable"];
            Assert.AreEqual(found.Count(), 3);
            Assert.IsTrue(found.Any(x => x.Key.HeadWord == "ill-natured" && x.Key.HeadId == 00 && x.Offset == 01140041 && x.SenseNumber == 1 && x.TagCount == 1));
            Assert.IsTrue(found.Any(x => x.Key.HeadWord == "sensitive" && x.Key.HeadId == 01 && x.Offset == 02111557 && x.SenseNumber == 3 && x.TagCount == 0));
            Assert.IsTrue(found.Any(x => x.Key.HeadWord == "sensitive" && x.Key.HeadId == 01 && x.Offset == 02111880 && x.SenseNumber == 2 && x.TagCount == 0));
        }

        [TestMethod]
        public async Task ParseSenseIndexFile_ParsePerformance()
        {
            const int PARSE_COUNT = 10;

            StorageFile senseFile = await FileRetriever.GetSenseIndexFile();
            SenseIndex index = null;

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 1; i <= PARSE_COUNT; i++)
            {
                index = await SenseIndex.ParseFileAsync(senseFile);
            }
            sw.Stop();

            TimeSpan averageTime = new TimeSpan(sw.Elapsed.Ticks / PARSE_COUNT);

            Debug.WriteLine($"Sense Index File Information: Dictionary Count = {index.SearchDictionary.Count()}, Entry Count = {index.AllEntries.Count()}");
            Debug.WriteLine($"Parse Sense Index File Performance (per {PARSE_COUNT}): Time Elapsed = {sw.Elapsed.ToString()}, Average Time = {averageTime.ToString()}");
        }

        [TestMethod]
        public async Task GetLexicographerFile_NounAnimal()
        {
            StorageFile file = await FileRetriever.GetLexicographerFile(Constants.LexicographerFiles.NOUN_ANIMAL);
        }

        [TestMethod]
        public async Task GetDataFile_Noun()
        {
            StorageFile file = await FileRetriever.GetSyntacticCategoryDataFile(Constants.PartOfSpeech.NOUN);
        }

        [TestMethod]
        public async Task GetDataEntry_IronVerb()
        {
            // iron%2:35:00:: 01393487 1 2
            SenseIndex index = await SenseIndex.ParseFileAsync();
            IEnumerable<SenseIndexEntry> ironEntries = index.SearchDictionary["iron"];

            SenseIndexEntry entry = ironEntries.Where(x => x.Key.PartOfSpeech == Constants.PartOfSpeech.VERB).First();
            PartOfSpeechDataFileEntry posEntry = await PartOfSpeechDataFile.GetEntryAsync(entry.Key.PartOfSpeech, entry.Offset);

            //// Basic Info ////
            // 01393487 35 v 03 iron 0 iron_out 0 press 3 006 
            Assert.AreEqual(1393487, posEntry.Offset);
            Assert.AreEqual(Constants.LexicographerFiles.VERB_CONTACT, posEntry.LexFile);
            Assert.AreEqual(Constants.PartOfSpeech.VERB, posEntry.PartOfSpeech);
            Assert.AreEqual(3, posEntry.Words.Count());
            Assert.IsTrue(posEntry.Words.Any(x => x.Lemma == "iron" && x.LexId == 0));
            Assert.IsTrue(posEntry.Words.Any(x => x.Lemma == "iron out" && x.LexId == 0));
            Assert.IsTrue(posEntry.Words.Any(x => x.Lemma == "press" && x.LexId == 3));

            //// POINTERS ////
            // @ 01393270 v 0000 
            // * 00371917 v 0000 
            // + 03589998 n 0101 
            // + 03591044 n 0101 
            // + 00582127 n 0101 
            // ~ 01393140 v 0000 
            Assert.AreEqual(6, posEntry.Pointers.Count());
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPERNYM 
                && x.DataFileOffset == 01393270 && x.PartOfSpeech == Constants.PartOfSpeech.VERB 
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.ENTAILMENT 
                && x.DataFileOffset == 00371917 && x.PartOfSpeech == Constants.PartOfSpeech.VERB 
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM 
                && x.DataFileOffset == 03589998 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN 
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM 
                && x.DataFileOffset == 03591044 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN 
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM 
                && x.DataFileOffset == 00582127 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN 
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM 
                && x.DataFileOffset == 01393140 && x.PartOfSpeech == Constants.PartOfSpeech.VERB 
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));

            //// Frames ////
            // 02 + 08 00 + 11 00
            Assert.AreEqual(2, posEntry.Frames.Count());
            Assert.IsTrue(posEntry.Frames.Any(x => x.FrameNumber == 08 && x.WordNumber == 0x00));
            Assert.IsTrue(posEntry.Frames.Any(x => x.FrameNumber == 11 && x.WordNumber == 0x00));

            //// Gloss ////
            // press and smooth with a heated iron; "press your shirts"; "she stood there ironing"
            Assert.AreEqual("press and smooth with a heated iron; \"press your shirts\"; \"she stood there ironing\"", posEntry.Gloss);
        }

        [TestMethod]
        public async Task GetDataEntry_IronNoun()
        {
            // iron%1:06:00:: 03589998 4 0
            SenseIndex index = await SenseIndex.ParseFileAsync();
            IEnumerable<SenseIndexEntry> ironEntries = index.SearchDictionary["iron"];

            SenseIndexEntry entry = ironEntries.Where(x => x.Key.PartOfSpeech == Constants.PartOfSpeech.NOUN && x.Offset == 03589998).First();
            PartOfSpeechDataFileEntry posEntry = await PartOfSpeechDataFile.GetEntryAsync(entry.Key.PartOfSpeech, entry.Offset);

            //// Basic Info ////
            // 03589998 06 n 02 iron 0 smoothing_iron 0
            Assert.AreEqual(03589998, posEntry.Offset);
            Assert.AreEqual(Constants.LexicographerFiles.NOUN_ARTIFACT, posEntry.LexFile);
            Assert.AreEqual(Constants.PartOfSpeech.NOUN, posEntry.PartOfSpeech);
            Assert.AreEqual(2, posEntry.Words.Count());
            Assert.IsTrue(posEntry.Words.Any(x => x.Lemma == "iron" && x.LexId == 0));
            Assert.IsTrue(posEntry.Words.Any(x => x.Lemma == "smoothing iron" && x.LexId == 0));

            //// POINTERS ////
            // @ 03533443 n 0000 
            // + 01393487 v 0101 
            // ~ 03366040 n 0000 
            // ~ 03448699 n 0000 
            // ~ 04316971 n 0000 
            // ~ 04482866 n 0000
            Assert.AreEqual(6, posEntry.Pointers.Count());
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPERNYM
                && x.DataFileOffset == 03533443 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM
                && x.DataFileOffset == 01393487 && x.PartOfSpeech == Constants.PartOfSpeech.VERB
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 03366040 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 03448699 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 04316971 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 04482866 && x.PartOfSpeech == Constants.PartOfSpeech.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));

            //// Frames ////
            Assert.AreEqual(0, posEntry.Frames.Count());

            //// Gloss ////
            // press and smooth with a heated iron; "press your shirts"; "she stood there ironing"
            Assert.AreEqual("home appliance consisting of a flat metal base that is heated and used to smooth cloth", posEntry.Gloss);
        }

        [TestMethod]
        public async Task GetDataEntry_IronNoun_Performance()
        {
            const int GET_COUNT = 1000;

            // iron%1:06:00:: 03589998 4 0
            SenseIndex index = await SenseIndex.ParseFileAsync();
            IEnumerable<SenseIndexEntry> ironEntries = index.SearchDictionary["iron"];

            SenseIndexEntry entry = ironEntries.Where(x => x.Key.PartOfSpeech == Constants.PartOfSpeech.NOUN && x.Offset == 03589998).First();

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 1; i <= GET_COUNT; i++)
            {
                PartOfSpeechDataFileEntry posEntry = await PartOfSpeechDataFile.GetEntryAsync(entry.Key.PartOfSpeech, entry.Offset);
            }
            sw.Stop();

            Debug.WriteLine($"Get Data Entry Performance (per {GET_COUNT}): Time Elapsed = {sw.Elapsed.ToString()}");
        }

        [TestMethod]
        public async Task GetDataEntries_Iron()
        {
            SenseIndex index = await SenseIndex.ParseFileAsync();
            IEnumerable<SenseIndexEntry> ironEntries = index.SearchDictionary["iron"];
            IEnumerable<PartOfSpeechDataFileEntry> posEntries = await PartOfSpeechDataFile.GetEntriesAsync(Constants.PartOfSpeech.NOUN, 
                ironEntries.Where(x => x.Key.PartOfSpeech == Constants.PartOfSpeech.NOUN).
                Select(x => x.Offset).ToArray());

            Assert.AreEqual(4, posEntries.Count());
        }

        [TestMethod]
        public async Task GetDataEntries_Sparse()
        {
            // 00005930 03 n 01 dwarf 0 001 @ 00004475 n 0000 | a plant or animal that is atypically small 
            // 02945804 06 n 01 camcorder 0 001 @ 04412132 n 0000 | a portable television camera and videocassette recorder 
            // 08702414 15 n 01 yard 1 001 @ 08691133 n 0000 | a tract of land where logs are accumulated 
            // 14379048 26 n 01 scleritis 0 001 @ 14359944 n 0000 | inflammation of the sclera
            // 15325294 28 n 05 9/11 0 9-11 0 September_11 0 Sept._11 0 Sep_11 0 003 #p 15237535 n 0000 @i 01249244 n 0000 ;c 00761047 n 0000 | the day in 2001 when Arab suicide bombers hijacked United States airliners and used them as bombs  

            IEnumerable<int> offsets = new int[] { 00005930, 02945804, 08702414, 14379048, 15325294 };
            PartOfSpeechDataFileEntry[] posEntries = (await PartOfSpeechDataFile.GetEntriesAsync(Constants.PartOfSpeech.NOUN, offsets)).ToArray();

            Assert.AreEqual(5, posEntries.Count());
            Assert.AreEqual("dwarf", posEntries[0].Words.First().Lemma);
            Assert.AreEqual("camcorder", posEntries[1].Words.First().Lemma);
            Assert.AreEqual("yard", posEntries[2].Words.First().Lemma);
            Assert.AreEqual("scleritis", posEntries[3].Words.First().Lemma);
            Assert.AreEqual("9/11", posEntries[4].Words.First().Lemma);
        }

        [TestMethod]
        public async Task GetDataEntries_SparsePerformance()
        {
            // 00005930 03 n 01 dwarf 0 001 @ 00004475 n 0000 | a plant or animal that is atypically small 
            // 02945804 06 n 01 camcorder 0 001 @ 04412132 n 0000 | a portable television camera and videocassette recorder 
            // 08702414 15 n 01 yard 1 001 @ 08691133 n 0000 | a tract of land where logs are accumulated 
            // 14379048 26 n 01 scleritis 0 001 @ 14359944 n 0000 | inflammation of the sclera
            // 15325294 28 n 05 9/11 0 9-11 0 September_11 0 Sept._11 0 Sep_11 0 003 #p 15237535 n 0000 @i 01249244 n 0000 ;c 00761047 n 0000 | the day in 2001 when Arab suicide bombers hijacked United States airliners and used them as bombs  

            IEnumerable<int> offsets = new int[] { 00005930, 02945804, 08702414, 14379048, 15325294 };

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 1; i <= 1000; i++)
            {
                PartOfSpeechDataFileEntry[] posEntries = (await PartOfSpeechDataFile.GetEntriesAsync(Constants.PartOfSpeech.NOUN, offsets)).ToArray();
            }
            sw.Stop();

            Debug.WriteLine($"Get Multiple Data Entries Noun Sparse Performance (per 1000): Elapsed = {sw.Elapsed.ToString()}");
        }

        [TestMethod]
        public async Task GetExceptionList_Noun()
        {
            ExceptionList list = await ExceptionList.ParseFileAsync(Constants.PartOfSpeech.NOUN);
            Assert.AreEqual(2054, list.Count());

            Debug.WriteLine($"Noun Exception List: Count = {list.Count()}");
        }

        [TestMethod]
        public async Task GetExceptionList_Verb()
        {
            ExceptionList list = await ExceptionList.ParseFileAsync(Constants.PartOfSpeech.VERB);
            Assert.AreEqual(2401, list.Count());

            Debug.WriteLine($"Verb Exception List: Count = {list.Count()}");
        }

        [TestMethod]
        public async Task SentenceExampleList_Bankroll()
        {
            VerbExampleSentenceList list = await VerbExampleSentenceList.ParseFilesAsync();
            VerbExampleSentenceEntry entry = list.First(x => x.Key.Lemma == "bankroll");

            string[] sentences = entry.ExampleSentences.ToArray();

            Assert.AreEqual(1, sentences.Count());
            Assert.AreEqual("Sam and Sue bankroll the movie", sentences[0]);
        }

        [TestMethod]
        public async Task SentenceExampleList_Pet()
        {
            VerbExampleSentenceList list = await VerbExampleSentenceList.ParseFilesAsync();
            VerbExampleSentenceEntry entry = list.First(x => x.Key.Lemma == "pet");

            string[] sentences = entry.ExampleSentences.ToArray();

            Assert.AreEqual(0, sentences.Count());
        }

        [TestMethod]
        public async Task SentenceExampleList_Embarrass()
        {
            VerbExampleSentenceList list = await VerbExampleSentenceList.ParseFilesAsync();
            VerbExampleSentenceEntry entry = list.First(x => x.Key.Lemma == "embarrass");

            string[] sentences = entry.ExampleSentences.ToArray();

            Assert.AreEqual(2, sentences.Count());
            Assert.AreEqual("The performance is likely to embarrass Sue", sentences[0]);
            Assert.AreEqual("Sam cannot embarrass Sue", sentences[1]);
        }

        // { dripstone, hoodmold, hoodmould, drip,@ (a protective drip that is made of stone) }
        [TestMethod]
        public void SynSetEntry_Parse_dripstone()
        {
            string line = "{ dripstone, hoodmold, hoodmould, drip,@ (a protective drip that is made of stone) }";
            SynsetEntry entry = SynsetEntry.Parse(line);

            Word[] words = entry.Words.ToArray();
            Assert.AreEqual(3, entry.Words.Count());
            Assert.AreEqual("dripstone", words[0].Lemma);
            Assert.AreEqual(0, words[0].LexId);
            Assert.IsNull(words[0].Pointers);
            Assert.IsNull(words[0].Frames);

            Assert.AreEqual("hoodmold", words[1].Lemma);
            Assert.AreEqual(0, words[1].LexId);
            Assert.IsNull(words[1].Pointers);
            Assert.IsNull(words[1].Frames);

            Assert.AreEqual("hoodmould", words[2].Lemma);
            Assert.AreEqual(0, words[2].LexId);
            Assert.IsNull(words[2].Pointers);
            Assert.IsNull(words[2].Frames);

            Pointer[] pointers = entry.Pointers.ToArray();
            Assert.AreEqual(1, pointers.Count());
            Assert.AreEqual("drip", pointers[0].Lemma);
            Assert.AreEqual(Constants.PointSymbol.HYPERNYM, pointers[0].PointerSymbol);
            Assert.AreEqual(0, pointers[0].LexId);

            Assert.AreEqual("a protective drip that is made of stone", entry.Gloss);
        }

        // { [ driver, verb.contact:drive3,+ ] number_one_wood, wood2,@ (a golf club (a wood) with a near vertical face that is used for hitting long shots from the tee) }
        [TestMethod]
        public void SynSetEntry_Parse_driver()
        {
            string line = "{ [ driver, verb.contact:drive3,+ ] number_one_wood, wood2,@ (a golf club (a wood) with a near vertical face that is used for hitting long shots from the tee) }";
            SynsetEntry entry = SynsetEntry.Parse(line);

            Word[] words = entry.Words.ToArray();
            Assert.AreEqual(2, entry.Words.Count());

            Assert.AreEqual("driver", words[0].Lemma);
            Assert.AreEqual(1, words[0].Pointers.Count());
            Pointer[] wordPointers = words[0].Pointers.ToArray();
            Assert.AreEqual(Constants.LexicographerFiles.VERB_CONTACT, wordPointers[0].LexFile);
            Assert.AreEqual("drive", wordPointers[0].Lemma);
            Assert.AreEqual(3, wordPointers[0].LexId);
            Assert.AreEqual(Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM, wordPointers[0].PointerSymbol);

            Assert.AreEqual("number one wood", words[1].Lemma);
            Assert.AreEqual(0, words[1].LexId);
            Assert.IsNull(words[1].Pointers);
            Assert.IsNull(words[1].Frames);

            Pointer[] pointers = entry.Pointers.ToArray();
            Assert.AreEqual(1, pointers.Count());
            Assert.AreEqual("wood", pointers[0].Lemma);
            Assert.AreEqual(2, pointers[0].LexId);
            Assert.AreEqual(Constants.PointSymbol.HYPERNYM, pointers[0].PointerSymbol);

            Assert.AreEqual("a golf club (a wood) with a near vertical face that is used for hitting long shots from the tee", entry.Gloss);
        }

        // { [ crape2, noun.artifact:crape,+ ] [ crepe, noun.artifact:crepe,+ noun.substance:crepe2,+ ] cover,@ frames: 8,11 (cover or drape with crape; "crape the mirror") }
        [TestMethod]
        public void SynSetEntry_Parse_crape2()
        {
            string line = "{ [ crape2, noun.artifact:crape,+ ] [ crepe, noun.artifact:crepe,+ noun.substance:crepe2,+ ] cover,@ frames: 8,11 (cover or drape with crape; \"crape the mirror\") }";
            SynsetEntry entry = SynsetEntry.Parse(line);

            Word[] words = entry.Words.ToArray();
            Assert.AreEqual(2, words.Count());

            Assert.AreEqual("crape", words[0].Lemma);
            Assert.AreEqual(2, words[0].LexId);
            Pointer[] wordPointers = words[0].Pointers.ToArray();
            Assert.AreEqual("crape", wordPointers[0].Lemma);
            Assert.AreEqual(Constants.LexicographerFiles.NOUN_ARTIFACT, wordPointers[0].LexFile);
            Assert.AreEqual(0, wordPointers[0].LexId);
            Assert.AreEqual(Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM, wordPointers[0].PointerSymbol);

            Assert.AreEqual("crepe", words[1].Lemma);
            Assert.AreEqual(0, words[1].LexId);
            wordPointers = words[1].Pointers.ToArray();
            Assert.AreEqual("crepe", wordPointers[0].Lemma);
            Assert.AreEqual(Constants.LexicographerFiles.NOUN_ARTIFACT, wordPointers[0].LexFile);
            Assert.AreEqual(0, wordPointers[0].LexId);
            Assert.AreEqual(Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM, wordPointers[0].PointerSymbol);

            Assert.AreEqual("crepe", wordPointers[1].Lemma);
            Assert.AreEqual(Constants.LexicographerFiles.NOUN_SUBSTANCE, wordPointers[1].LexFile);
            Assert.AreEqual(2, wordPointers[1].LexId);
            Assert.AreEqual(Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM, wordPointers[1].PointerSymbol);

            byte[] frames = entry.Frames.ToArray();
            Assert.AreEqual(2, frames.Count());
            Assert.AreEqual(8, frames[0]);
            Assert.AreEqual(11, frames[1]);

            Pointer[] pointers = entry.Pointers.ToArray();
            Assert.AreEqual(1, pointers.Count());
            Assert.AreEqual("cover", pointers[0].Lemma);
            Assert.AreEqual(0, pointers[0].LexId);
            Assert.AreEqual(Constants.PointSymbol.HYPERNYM, pointers[0].PointerSymbol);

            Assert.AreEqual("cover or drape with crape; \"crape the mirror\"", entry.Gloss);
        }

        // { pit, 
        // [ oppose, adj.all:hostile1^opponent,+ noun.person:opponent2,+ ] 
        // [ match, noun.event:match,+ ] 
        // [play_off, noun.event:playoff,+ ] 
        // confront,@ frames: 9,10 (set into opposition or rivalry; "let them match their best athletes against ours"; "pit a chess player against the Russian champion"; "He plays his two children off against each other") }
        [TestMethod]
        public void SynSetEntry_Parse_pit()
        {
            string line = "{ pit, [ oppose, adj.all:hostile1^opponent,+ noun.person:opponent2,+ ] [ match, noun.event:match,+ ] [play_off, noun.event:playoff,+ ] confront,@ frames: 9,10 (set into opposition or rivalry; \"let them match their best athletes against ours\"; \"pit a chess player against the Russian champion\"; \"He plays his two children off against each other\") }";
            SynsetEntry entry = SynsetEntry.Parse(line);

            Word[] words = entry.Words.ToArray();
            Assert.AreEqual(4, words.Count());

            // Word 1: pit
            Assert.AreEqual("pit", words[0].Lemma);

            // Word 2: oppose
            Assert.AreEqual("oppose", words[1].Lemma);
            Pointer[] wordPointers = words[1].Pointers.ToArray();

            Assert.AreEqual(2, wordPointers.Count());
            AssertPointer(wordPointers[0], "hostile", 1, Constants.LexicographerFiles.ADJ_ALL,
                "opponent", 0, Constants.PointerSymbols["+"]);
            AssertPointer(wordPointers[1], "opponent", 2, Constants.LexicographerFiles.NOUN_PERSON,
                null, 0, Constants.PointerSymbols["+"]);

            // Word 3: match
            Assert.AreEqual("match", words[2].Lemma);
            wordPointers = words[2].Pointers.ToArray();
            Assert.AreEqual(1, wordPointers.Count());
            AssertPointer(wordPointers[0], "match", 0, Constants.LexicographerFiles.NOUN_EVENT, null, 0, Constants.PointerSymbols["+"]);

            // Word 4: play off
            Assert.AreEqual("play off", words[3].Lemma);
            wordPointers = words[3].Pointers.ToArray();
            Assert.AreEqual(1, wordPointers.Count());
            AssertPointer(wordPointers[0], "playoff", 0, Constants.LexicographerFiles.NOUN_EVENT, null, 0, Constants.PointerSymbols["+"]);

            Pointer[] pointers = entry.Pointers.ToArray();
            Assert.AreEqual(1, pointers.Count());
            Assert.AreEqual("confront", pointers[0].Lemma);
            Assert.AreEqual(Constants.PointerSymbols["@"], pointers[0].PointerSymbol);
        }

        private void AssertPointer(Pointer pointer, string lemma, byte lexId, 
            Constants.LexicographerFiles file, string satelliteLemma, byte satelliteLexId, 
            Constants.PointSymbol ptSymbol)
        {
            Assert.AreEqual(lemma, pointer.Lemma);
            Assert.AreEqual(lexId, pointer.LexId);
            Assert.AreEqual(file, pointer.LexFile);
            Assert.AreEqual(satelliteLemma, pointer.SatelliteLemma);
            Assert.AreEqual(satelliteLexId, pointer.SatelliteLexId);
            Assert.AreEqual(ptSymbol, pointer.PointerSymbol);
        }

        [TestMethod]
        public async Task LexicographerFile_ParseWholeFiles_Nouns()
        {
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_ACT, 6657);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_ANIMAL, 7510);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_ARTIFACT, 11605);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_ATTRIBUTE, 3037);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_BODY, 2018);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_COGNITION, 2973);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_COMMUNICATION, 5627);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_EVENT, 1076);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_FEELING, 430);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_FOOD, 2575);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_GROUP, 2624);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_LOCATION, 3222);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_MOTIVE, 42);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_OBJECT, 1546);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_PERSON, 11073);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_PHENOMENON, 642);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_PLANT, 8032);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_POSSESSION, 1062);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_PROCESS, 770);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_QUANTITY, 1276);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_RELATION, 437);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_SHAPE, 344);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_STATE, 3547);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_SUBSTANCE, 2986);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_TIME, 1030);
            await ParseWholeFile(Constants.LexicographerFiles.NOUN_TOPS, 51);
        }

        [TestMethod]
        public async Task LexicographerFile_ParseWholeFiles_Verbs()
        {
            await ParseWholeFile(Constants.LexicographerFiles.VERB_BODY, 546);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_CHANGE, 2388);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_COGNITION, 698);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_COMMUNICATION, 1550);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_COMPETITION, 459);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_CONSUMPTION, 242);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_CONTACT, 2198);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_CREATION, 698);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_EMOTION, 343);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_MOTION, 1411);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_PERCEPTION, 461);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_POSSESSION, 848);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_SOCIAL, 1110);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_STATIVE, 756);
            await ParseWholeFile(Constants.LexicographerFiles.VERB_WEATHER, 81);
        }

        [TestMethod]
        public async Task LexicographerFile_ParseWholeFiles_Adverbs()
        {
            await ParseWholeFile(Constants.LexicographerFiles.ADV_ALL, 3625);
        }

        [TestMethod]
        public async Task LexicographerFile_ParseWholeFiles_AdjOther()
        {
            await ParseWholeFile(Constants.LexicographerFiles.ADJ_PERT, 3665);
            await ParseWholeFile(Constants.LexicographerFiles.ADJ_PPL, 60);
        }

        [TestMethod]
        public async Task LexicographerFile_ParseWholeFiles_AdjAll()
        {
            StorageFile storFile = await FileRetriever.GetLexicographerFile(Constants.LexicographerFiles.ADJ_ALL);
            IEnumerable<SynsetCluster> synsets = await LexicographerFile.GetAllEntriesAdjAsync(storFile);

            Assert.AreEqual(1850, synsets.Count());
        }

        [TestMethod]
        public async Task LexicographerFile_ABLE_AdjAll()
        {
            StorageFile storFile = await FileRetriever.GetLexicographerFile(Constants.LexicographerFiles.ADJ_ALL);
            IEnumerable<SynsetCluster> synsets = await LexicographerFile.GetAllEntriesAdjAsync(storFile);

            SynsetCluster first = synsets.First();
            Assert.AreEqual("ABLE", first.HeadSynset.Words.First().Lemma);
            Assert.AreEqual("UNABLE", first.AdditionalClusters.First().HeadSynset.Words.First().Lemma);
        }

        [TestMethod]
        public async Task LexicographerFile_tribadistic_AdjAll()
        {
            StorageFile storFile = await FileRetriever.GetLexicographerFile(Constants.LexicographerFiles.ADJ_ALL);
            IEnumerable<SynsetCluster> synsets = await LexicographerFile.GetAllEntriesAdjAsync(storFile);

            SynsetCluster last = synsets.Last();
            Assert.AreEqual("tribadistic", last.HeadSynset.Words.First().Lemma);
        }

        public async Task ParseWholeFile(Constants.LexicographerFiles file, int count)
        {
            StorageFile storFile = await FileRetriever.GetLexicographerFile(file);
            IEnumerable<SynsetEntry> synsets = await LexicographerFile.GetAllEntries(storFile);

            Assert.AreEqual(count, synsets.Count());
        }
    }
}
