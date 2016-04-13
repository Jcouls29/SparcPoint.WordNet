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
    public class LexicographerFile
    {
        public static async Task<SynsetEntry> GetEntryAsync(StorageFile file, int byteOffset)
        {
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                classicStream.Seek(byteOffset, SeekOrigin.Begin);
                string line = await reader.ReadLineAsync();
                if (line == null) throw new InvalidOperationException("Line came back null.");

                return SynsetEntry.Parse(line);
            }
        }

        public static async Task<SynsetEntry> GetEntryAsync(Constants.LexicographerFiles fileType, int byteOffset)
        {
            StorageFile file = await FileRetriever.GetLexicographerFile(fileType);
            return await GetEntryAsync(file, byteOffset);
        }
    }

    public struct SynsetEntry
    {
        /// <summary>
        /// Parses Everything but Adjective Clusters in a single line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static SynsetEntry Parse(string line)
        {
            return new SynsetEntry();
        }

        /// <summary>
        /// Parses Adjective Clusters with multiple lines
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static SynsetEntry Parse(string[] lines)
        {
            return new SynsetEntry();
        }
    }

    public struct Word
    {

    }

    public struct Pointer
    {

    }
}
