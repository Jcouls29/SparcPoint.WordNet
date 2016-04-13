﻿using System;
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
            Assert.AreEqual(Constants.SynSetType.ADJECTIVE_SATELLITE, senseKey.PartOfSpeech);
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
            Assert.AreEqual(Constants.SynSetType.NOUN, senseKey.PartOfSpeech);
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
            StorageFile file = await FileRetriever.GetSyntacticCategoryDataFile(Constants.SynSetType.NOUN);
        }

        [TestMethod]
        public async Task GetDataEntry_IronVerb()
        {
            // iron%2:35:00:: 01393487 1 2
            SenseIndex index = await SenseIndex.ParseFileAsync();
            IEnumerable<SenseIndexEntry> ironEntries = index.SearchDictionary["iron"];

            SenseIndexEntry entry = ironEntries.Where(x => x.Key.PartOfSpeech == Constants.SynSetType.VERB).First();
            PartOfSpeechDataFileEntry posEntry = await PartOfSpeechDataFile.GetEntryAsync(entry.Key.PartOfSpeech, entry.Offset);

            //// Basic Info ////
            // 01393487 35 v 03 iron 0 iron_out 0 press 3 006 
            Assert.AreEqual(1393487, posEntry.Offset);
            Assert.AreEqual(Constants.LexicographerFiles.VERB_CONTACT, posEntry.LexFile);
            Assert.AreEqual(Constants.SynSetType.VERB, posEntry.PartOfSpeech);
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
                && x.DataFileOffset == 01393270 && x.PartOfSpeech == Constants.SynSetType.VERB 
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.ENTAILMENT 
                && x.DataFileOffset == 00371917 && x.PartOfSpeech == Constants.SynSetType.VERB 
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM 
                && x.DataFileOffset == 03589998 && x.PartOfSpeech == Constants.SynSetType.NOUN 
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM 
                && x.DataFileOffset == 03591044 && x.PartOfSpeech == Constants.SynSetType.NOUN 
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM 
                && x.DataFileOffset == 00582127 && x.PartOfSpeech == Constants.SynSetType.NOUN 
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM 
                && x.DataFileOffset == 01393140 && x.PartOfSpeech == Constants.SynSetType.VERB 
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

            SenseIndexEntry entry = ironEntries.Where(x => x.Key.PartOfSpeech == Constants.SynSetType.NOUN && x.Offset == 03589998).First();
            PartOfSpeechDataFileEntry posEntry = await PartOfSpeechDataFile.GetEntryAsync(entry.Key.PartOfSpeech, entry.Offset);

            //// Basic Info ////
            // 03589998 06 n 02 iron 0 smoothing_iron 0
            Assert.AreEqual(03589998, posEntry.Offset);
            Assert.AreEqual(Constants.LexicographerFiles.NOUN_ARTIFACT, posEntry.LexFile);
            Assert.AreEqual(Constants.SynSetType.NOUN, posEntry.PartOfSpeech);
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
                && x.DataFileOffset == 03533443 && x.PartOfSpeech == Constants.SynSetType.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.DERIVATIONALLY_RELATED_FORM
                && x.DataFileOffset == 01393487 && x.PartOfSpeech == Constants.SynSetType.VERB
                && x.SourceWordNumber == 01 && x.TargetWordNumber == 01));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 03366040 && x.PartOfSpeech == Constants.SynSetType.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 03448699 && x.PartOfSpeech == Constants.SynSetType.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 04316971 && x.PartOfSpeech == Constants.SynSetType.NOUN
                && x.SourceWordNumber == 00 && x.TargetWordNumber == 00));
            Assert.IsTrue(posEntry.Pointers.Any(x => x.PointerType == Constants.PointSymbol.HYPONYM
                && x.DataFileOffset == 04482866 && x.PartOfSpeech == Constants.SynSetType.NOUN
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

            SenseIndexEntry entry = ironEntries.Where(x => x.Key.PartOfSpeech == Constants.SynSetType.NOUN && x.Offset == 03589998).First();

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 1; i <= GET_COUNT; i++)
            {
                PartOfSpeechDataFileEntry posEntry = await PartOfSpeechDataFile.GetEntryAsync(entry.Key.PartOfSpeech, entry.Offset);
            }
            sw.Stop();

            Debug.WriteLine($"Get Data Entry Performance (per {GET_COUNT}): Time Elapsed = {sw.Elapsed.ToString()}");
        }

    }
}