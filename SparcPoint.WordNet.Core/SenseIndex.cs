using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SparcPoint.WordNet
{
    public class SenseIndex
    {
        public SenseIndex(Dictionary<string, IEnumerable<SenseIndexEntry>> searchDictionary, IEnumerable<SenseIndexEntry> allEntries)
        {
            if (searchDictionary == null) throw new ArgumentNullException(nameof(searchDictionary));
            if (allEntries == null) throw new ArgumentNullException(nameof(allEntries));

            this.SearchDictionary = searchDictionary;
            this.AllEntries = allEntries;
        }

        public Dictionary<string, IEnumerable<SenseIndexEntry>> SearchDictionary { get; }
        public IEnumerable<SenseIndexEntry> AllEntries { get; }

        public static async Task<SenseIndex> ParseFileAsync(StorageFile file)
        {
            List<SenseIndexEntry> entries = new List<SenseIndexEntry>();

            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                while (reader.Peek() >= 0)
                {
                    string line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    SenseIndexEntry nextEntry = SenseIndexEntry.Parse(line);
                    entries.Add(nextEntry);
                }
            }

            Dictionary<string, IEnumerable<SenseIndexEntry>> dictionary = entries.GroupBy(x => x.Key.Lemma).
                ToDictionary(x => x.Key, x => (IEnumerable<SenseIndexEntry>)x);

            return new SenseIndex(dictionary, entries);
        }

        public static async Task<SenseIndex> ParseFileAsync()
        {
            StorageFile senseFile = await FileRetriever.GetSenseIndexFile();
            return await ParseFileAsync(senseFile);
        }
    }

    public struct SenseIndexEntry
    {
        /// <summary>
        /// Sense Key Encoding
        /// </summary>
        public SenseKey Key { get; set; }

        /// <summary>
        /// Byte Offset where the Sense is found in the data file
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Sense Number
        /// </summary>
        public int SenseNumber { get; set; }

        /// <summary>
        /// Number of times the sense is tagged various semantic concordance text
        /// </summary>
        public int TagCount { get; set; }

        public static SenseIndexEntry Parse(string line)
        {
            SenseIndexEntry entry = new SenseIndexEntry();

            int lastIndex = -1;

            string keyField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            entry.Key = SenseKey.Parse(keyField);

            string offsetField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            entry.Offset = int.Parse(offsetField);

            string senseField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            entry.SenseNumber = int.Parse(senseField);

            string tagField = ParseHelper.GetLastField(line, lastIndex);
            entry.TagCount = int.Parse(tagField);

            return entry;
        }
    }
}
