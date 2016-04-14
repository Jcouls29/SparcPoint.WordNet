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
    /// <summary>
    /// Inflected Forms of Words and their Bases
    /// </summary>
    public class ExceptionList :List<ExceptionListEntry>
    {
        public static async Task<ExceptionList> ParseFileAsync(Constants.PartOfSpeech pos)
        {
            StorageFile file = await FileRetriever.GetSyntacticCategoryExceptionFile(pos);
            return await ParseFileAsync(file, pos);
        }

        public static async Task<ExceptionList> ParseFileAsync(StorageFile file, Constants.PartOfSpeech pos)
        {
            ExceptionList entries = new ExceptionList();

            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                while (reader.Peek() >= 0)
                {
                    string line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    ExceptionListEntry nextEntry = ExceptionListEntry.Parse(line, pos);
                    entries.Add(nextEntry);
                }
            }

            return entries;
        }
    }

    public struct ExceptionListEntry
    {
        public Constants.PartOfSpeech PartOfSpeech { get; set; }
        public string InflectedForm { get; set; }
        public string BaseForm { get; set; }

        public static ExceptionListEntry Parse(string line, Constants.PartOfSpeech pos)
        {
            ExceptionListEntry entry = new ExceptionListEntry();
            entry.PartOfSpeech = pos;

            int lastIndex = -1;
            string nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            entry.InflectedForm = nextField;

            nextField = ParseHelper.GetLastField(line, lastIndex);
            entry.BaseForm = nextField;

            return entry;
        }
    }
}
