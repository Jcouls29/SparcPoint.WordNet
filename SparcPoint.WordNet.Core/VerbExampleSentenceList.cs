using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SparcPoint.WordNet
{
    public class VerbExampleSentenceList : List<VerbExampleSentenceEntry>
    {
        private VerbExampleSentenceList(IDictionary<byte, string> formats) : base()
        {
            if (formats == null) throw new ArgumentNullException(nameof(formats));
            this.ExampleSentenceFormats = formats;
        }

        internal IDictionary<byte, string> ExampleSentenceFormats { get; } = new Dictionary<byte, string>();

        public static async Task<VerbExampleSentenceList> ParseFilesAsync()
        {
            StorageFile sentsFile = await FileRetriever.GetExampleSentencesFile();
            StorageFile indexFile = await FileRetriever.GetExampleSentencesIndexFile();

            return await ParseFilesAsync(sentsFile, indexFile);
        }

        public static async Task<VerbExampleSentenceList> ParseFilesAsync(StorageFile sentsFile, StorageFile indexFile)
        {
            // Parse Sentences File first for the dictionary
            Dictionary<byte, string> formats = new Dictionary<byte, string>();

            using (IInputStream stream = await sentsFile.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                while (reader.Peek() >= 0)
                {
                    string line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    int lastIndex = -1;
                    string nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                    byte sentNumber = byte.Parse(nextField);

                    nextField = ParseHelper.GetLastField(line, lastIndex);
                    string exampleSent = nextField.Replace("%s", "{0}").Trim();

                    formats.Add(sentNumber, exampleSent);
                }
            }
            VerbExampleSentenceList list = new VerbExampleSentenceList(formats);

            /// Parse Index
            using (IInputStream stream = await indexFile.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                while (reader.Peek() >= 0)
                {
                    string line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    VerbExampleSentenceEntry entry = VerbExampleSentenceEntry.Parse(line, list);
                    list.Add(entry);
                }
            }

            return list;
        }
    }

    public struct VerbExampleSentenceEntry
    {
        internal VerbExampleSentenceEntry(VerbExampleSentenceList list, SenseKey key, IEnumerable<byte> numbers)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (numbers == null) throw new ArgumentNullException(nameof(numbers));
            this.List = list;
            this.Key = key;
            this.SentenceNumbers = numbers;
        }

        private VerbExampleSentenceList List { get; }
        public SenseKey Key { get; }
        internal IEnumerable<byte> SentenceNumbers { get; }

        public IEnumerable<string> ExampleSentences
        {
            get
            {
                var self = this;
                return this.SentenceNumbers.Select(x => string.Format(self.List.ExampleSentenceFormats[x], self.Key.Lemma)).ToArray();
            }
        }

        static internal VerbExampleSentenceEntry Parse(string line, VerbExampleSentenceList list)
        {
            int lastIndex = -1;
            string nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            SenseKey key = SenseKey.Parse(nextField);

            List<byte> numbers = new List<byte>();
            while(ParseHelper.NextSeparatorExists(line, lastIndex, ','))
            {
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ',');

                // Rare, but sometimes a number isn't provided
                if (!string.IsNullOrWhiteSpace(nextField))
                {
                    byte number = byte.Parse(nextField);
                    numbers.Add(number);
                }
            }

            nextField = ParseHelper.GetLastField(line, lastIndex);
            // Rare, but sometimes a number isn't provided
            if (!string.IsNullOrWhiteSpace(nextField))
            {
                byte lastNumber = byte.Parse(nextField);
                numbers.Add(lastNumber);
            }

            return new VerbExampleSentenceEntry(list, key, numbers);
        }
    }
}
