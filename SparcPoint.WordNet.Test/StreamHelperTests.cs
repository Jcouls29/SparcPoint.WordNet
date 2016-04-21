using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SparcPoint.WordNet.Test
{
    [TestClass]
    public class StreamHelperTests
    {
        [TestMethod]
        public async Task ReadLines_Single()
        {
            StorageFile file = await FileRetriever.GetExampleSentencesFile();
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            {
                string line = await classicStream.ReadLineAsync(0x0A);
                Assert.AreEqual("1 The children %s to the playground", line);
            }
        }

        [TestMethod]
        public async Task ReadLines_5Lines()
        {
            StorageFile file = await FileRetriever.GetExampleSentencesFile();
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            {
                string line1 = await classicStream.ReadLineAsync(0x0A);
                Assert.AreEqual("1 The children %s to the playground", line1);

                string line2 = await classicStream.ReadLineAsync(0x0A);
                Assert.AreEqual("10 The cars %s down the avenue", line2);

                string line3 = await classicStream.ReadLineAsync(0x0A);
                Assert.AreEqual("100 These glasses %s easily", line3);

                string line4 = await classicStream.ReadLineAsync(0x0A);
                Assert.AreEqual("101 These fabrics %s easily", line4);

                string line5 = await classicStream.ReadLineAsync(0x0A);
                Assert.AreEqual("102 They %s their earnings this year", line5);
            }
        }

        [TestMethod]
        public async Task ReadWholeFileCompare()
        {
            StorageFile file = await FileRetriever.GetExampleSentencesFile();

            // Stream Reader
            List<string> oldList = new List<string>();
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                do
                {
                    string line = await reader.ReadLineAsync();
                    oldList.Add(line);
                } while (!reader.EndOfStream);
            }

            // New Way
            List<string> newList = new List<string>();
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            {
                string line = await classicStream.ReadLineAsync(0x0A); ;
                do
                {
                    newList.Add(line);
                    line = await classicStream.ReadLineAsync(0x0A);
                } while (line != null);
            }

            Assert.AreEqual(oldList.Count(), newList.Count());
            for (int i = 0; i < oldList.Count(); i++)
            {
                Assert.AreEqual(oldList[i], newList[i]);
            }
        }

        [TestMethod]
        public async Task ReadWholeFileCompare_Performance()
        {
            StorageFile file = await FileRetriever.GetExampleSentencesFile();

            // Stream Reader
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                Stopwatch sw = Stopwatch.StartNew();
                string line = null;
                do
                {
                    line = await reader.ReadLineAsync();
                } while (!reader.EndOfStream);
                sw.Stop();

                Debug.WriteLine($"StreamReader ReadLine: Elapsed = {sw.Elapsed.ToString()}");
            }

            // New Way
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            {
                Stopwatch sw = Stopwatch.StartNew();
                string line = null;
                do
                {
                    line = await classicStream.ReadLineAsync(0x0A);
                } while (line != null);
                sw.Stop();

                Debug.WriteLine($"Custom ReadLine: Elapsed = {sw.Elapsed.ToString()}");
            }
        }
    }
}
