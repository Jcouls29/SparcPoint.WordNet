using SparcPoint.WordNet;
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
    public class PartOfSpeechDataFile
    {
        public static async Task<PartOfSpeechDataFileEntry> GetEntryAsync(StorageFile file, int byteOffset)
        {
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            using (StreamReader reader = new StreamReader(classicStream))
            {
                classicStream.Seek(byteOffset, SeekOrigin.Begin);
                string line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) throw new InvalidOperationException("Read Line is null.");

                return PartOfSpeechDataFileEntry.Parse(line);
            }
        }

        public static async Task<IEnumerable<PartOfSpeechDataFileEntry>> GetEntriesAsync(StorageFile file, IEnumerable<int> byteOffsets)
        {
            using (IInputStream stream = await file.OpenSequentialReadAsync())
            using (Stream classicStream = stream.AsStreamForRead())
            //using (StreamReader reader = new StreamReader(classicStream))
            {
                List<PartOfSpeechDataFileEntry> list = new List<PartOfSpeechDataFileEntry>();

                foreach(int nextOffset in byteOffsets.OrderBy(x => x))
                {
                    //classicStream.Position = (long)nextOffset;
                    long deltaOffset = nextOffset - (classicStream.Position - 1);
                    classicStream.Seek(deltaOffset, SeekOrigin.Current);

                    string line = await classicStream.ReadLineAsync((byte)0x0A);
                    if (string.IsNullOrWhiteSpace(line)) throw new InvalidOperationException("Read Line is null.");

                    PartOfSpeechDataFileEntry entry = PartOfSpeechDataFileEntry.Parse(line);
                    list.Add(entry);
                }

                return list;
            }
        }

        public static async Task<PartOfSpeechDataFileEntry> GetEntryAsync(Constants.PartOfSpeech synSetType, int byteOffset)
        {
            StorageFile file = await FileRetriever.GetSyntacticCategoryDataFile(synSetType);
            return await GetEntryAsync(file, byteOffset);
        }

        public static async Task<IEnumerable<PartOfSpeechDataFileEntry>> GetEntriesAsync(Constants.PartOfSpeech synSetType, IEnumerable<int> byteOffsets)
        {
            StorageFile file = await FileRetriever.GetSyntacticCategoryDataFile(synSetType);
            return await GetEntriesAsync(file, byteOffsets);
        }
    }

    public struct PartOfSpeechDataFileEntry
    {
        /// <summary>
        /// Byte Offset within the Data File
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Lexicographer File where Synset is located
        /// </summary>
        public Constants.LexicographerFiles LexFile { get; set; }

        /// <summary>
        /// Part of Speech
        /// </summary>
        public Constants.PartOfSpeech PartOfSpeech { get; set; }

        /// <summary>
        /// Collection of Words in Entry
        /// </summary>
        public IEnumerable<DataWordLexIdPair> Words { get; set; }

        /// <summary>
        /// Collection of pointers from this synset to others 
        /// within corresponding data files (data.noun, data.verb, etc)
        /// </summary>
        public IEnumerable<DataPointer> Pointers { get; set; }

        /// <summary>
        /// Collection of Verb Frames (Sentence Structures) [Verbs Only]
        /// </summary>
        public IEnumerable<FramePair> Frames { get; set; }

        /// <summary>
        /// Example Sentences / Definitions of SynSet
        /// </summary>
        public string Gloss { get; set; }

        public static PartOfSpeechDataFileEntry Parse(string line)
        {
            PartOfSpeechDataFileEntry rtn = new PartOfSpeechDataFileEntry();

            int lastIndex = -1;

            // Offset
            string nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            rtn.Offset = int.Parse(nextField);

            // Lex File
            nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            rtn.LexFile = (Constants.LexicographerFiles)Enum.Parse(typeof(Constants.LexicographerFiles), nextField);

            // Part of Speech
            nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            rtn.PartOfSpeech = Constants.SynSetTypeChar[Convert.ToChar(nextField)];

            // // Words // //
            // Word Count
            nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            int wordCount = int.Parse(nextField, System.Globalization.NumberStyles.HexNumber);

            List<DataWordLexIdPair> words = new List<DataWordLexIdPair>();
            for (int i = 1; i <= wordCount; i++)
            {
                // Word (Lemma)
                DataWordLexIdPair newWordPair = new DataWordLexIdPair();
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                newWordPair.Lemma = nextField.Replace("_", " ");

                // Lex Id
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                newWordPair.LexId = byte.Parse(nextField, System.Globalization.NumberStyles.HexNumber);

                words.Add(newWordPair);
            }
            rtn.Words = words;

            // // Pointers // //
            // Pointer Count
            nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
            int pointerCount = int.Parse(nextField);

            List<DataPointer> pointers = new List<DataPointer>();
            for (int i = 1; i <= pointerCount; i++)
            {
                DataPointer newPointer = new DataPointer();
                // Symbol
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                if (!Constants.PointerSymbols.ContainsKey(nextField)) throw new ArgumentOutOfRangeException(nameof(newPointer.PointerType), $"Pointer Symbol not found [{nextField}].");
                newPointer.PointerType = Constants.PointerSymbols[nextField];

                // Offset
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                newPointer.DataFileOffset = int.Parse(nextField);

                // Part Of Speech
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                newPointer.PartOfSpeech = Constants.SynSetTypeChar[Convert.ToChar(nextField)];

                // Source / Target
                nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                string sourceHex = nextField.Substring(0, 2);
                string targetHex = nextField.Substring(2, 2);
                newPointer.SourceWordNumber = byte.Parse(sourceHex, System.Globalization.NumberStyles.HexNumber);
                newPointer.TargetWordNumber = byte.Parse(targetHex, System.Globalization.NumberStyles.HexNumber);

                pointers.Add(newPointer);
            }
            rtn.Pointers = pointers;

            // Frames
            List<FramePair> frames = new List<FramePair>();
            if (rtn.PartOfSpeech == Constants.PartOfSpeech.VERB)
            {
                if (ParseHelper.NextSeparatorExists(line, lastIndex, " + "))
                {
                    // Frame Count
                    nextField = ParseHelper.GetNextField(line, ref lastIndex, " + ");
                    int frameCount = int.Parse(nextField);

                    for(int i = 1; i < frameCount; i++)
                    {
                        FramePair newFrame = new FramePair();
                        // Frame Number
                        nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                        newFrame.FrameNumber = byte.Parse(nextField);

                        // Word Number
                        nextField = ParseHelper.GetNextField(line, ref lastIndex, " + ");
                        newFrame.WordNumber = byte.Parse(nextField, System.Globalization.NumberStyles.HexNumber);

                        frames.Add(newFrame);
                    }

                    // On the last Entry we must search for the gloss delimiter.
                    FramePair lastFrame = new FramePair();
                    
                    // Frame Number
                    nextField = ParseHelper.GetNextField(line, ref lastIndex, ' ');
                    lastFrame.FrameNumber = byte.Parse(nextField);

                    // Word Number
                    nextField = ParseHelper.GetNextField(line, ref lastIndex, " | ");
                    lastFrame.WordNumber = byte.Parse(nextField, System.Globalization.NumberStyles.HexNumber);

                    frames.Add(lastFrame);
                }
                else
                {
                    // Frame Count is 00 Assumed
                    nextField = ParseHelper.GetNextField(line, ref lastIndex, " | ");
                }
            } else
            {
                // Not a Verb.  Need to move to beginning of Gloss
                // Missing space due to pointer count (or last pointer entry removing it)
                nextField = ParseHelper.GetNextField(line, ref lastIndex, "| ");
            }
            rtn.Frames = frames;

            // Gloss
            rtn.Gloss = ParseHelper.GetLastField(line, lastIndex).Trim();

            return rtn;
        }
    }

    public struct DataWordLexIdPair
    {
        /// <summary>
        /// Word / Lemma with Spaces instead of _
        /// </summary>
        public string Lemma { get; set; }

        /// <summary>
        /// Lexical Id of Word
        /// </summary>
        public byte LexId { get; set; }
    }

    public struct FramePair
    {
        /// <summary>
        /// Frame Number of the Verb Sentence
        /// </summary>
        /// <remarks>
        /// Constants.VerbFrameFormats can be used to get sentence example format
        /// </remarks>
        public byte FrameNumber { get; set; }

        /// <summary>
        /// Word Number in the SynSet frame applies to
        /// </summary>
        public byte WordNumber { get; set; }
    }

    public struct DataPointer
    {
        /// <summary>
        /// Pointer Type (Relationship)
        /// </summary>
        public Constants.PointSymbol PointerType { get; set; }

        /// <summary>
        /// Offset within the corresponding data file (data.noun, data.verb, etc)
        /// </summary>
        public int DataFileOffset { get; set; }

        /// <summary>
        /// Part of Speech of the pointer reference
        /// </summary>
        public Constants.PartOfSpeech PartOfSpeech { get; set; }

        /// <summary>
        /// Word Number in the current SynSet
        /// </summary>
        public byte SourceWordNumber { get; set; }

        /// <summary>
        /// Word Number in the Target (Pointer) SynSet
        /// </summary>
        public byte TargetWordNumber { get; set; }
    }
}
