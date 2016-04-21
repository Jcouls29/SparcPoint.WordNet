using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

                if (line[0] == '(') throw new InvalidOperationException("A comment line has been pointed to.");
                if (string.IsNullOrWhiteSpace(line)) throw new InvalidOperationException("Empty line has been pointed to.");

                // If an opening bracket is found then read lines until the last bracket is found.
                // TODO: Adjective(s) need to be handled in a different routine.  It's a special case
                if (line[0] == '[')
                {
                    string finalLine = line;
                    int openBrackets = 1;
                    while (openBrackets > 0)
                    {
                        line = await reader.ReadLineAsync();
                        finalLine += line;
                        openBrackets = openBrackets + line.Count(x => x == '[') - line.Count(x => x == ']');
                    }
                    return SynsetEntry.Parse(finalLine);
                }
                else
                {
                    return SynsetEntry.Parse(line);
                }
            }
        }

        public static async Task<SynsetEntry> GetEntryAsync(Constants.LexicographerFiles fileType, int byteOffset)
        {
            StorageFile file = await FileRetriever.GetLexicographerFile(fileType);
            return await GetEntryAsync(file, byteOffset);
        }
    }

    // { words pointers (gloss) }
    // { words pointers frames (gloss) }
    //
    // WORDS
    // word[lex_id],
    // [ word[lex_id], pointers ]
    // [ word, [pointers] frames ] (verb-only)
    // frames => frames: [f_num],...
    //
    // POINTERS
    // [lex_filename:]word[lex_id],pointer_symbol
    // [lex_filename:]word[lex_id]^word[lex_id],pointer_symbol
    //
    // Examples
    // { indefinite_quantity, noun.Tops:measure,@ (an estimated quantity) }
    // { coapt, fasten1,@ frames: 8,21 (fit tightly and fasten) }
    // { [ casket, noun.artifact:casket1,+ noun.artifact:casket,+ ] enclose,@ frames: 8,9 (enclose in a casket) }
    // { first, number_one, number_1", ordinal_number,@ (the first element in a countable series; "the first of the month") }
    public struct SynsetEntry
    {
        public IEnumerable<Word> Words { get; set; }
        public IEnumerable<Pointer> Pointers { get; set; }
        public IEnumerable<byte> Frames { get; set; }
        public string Gloss { get; set; }

        // INFO: How do we handle adjectives?

        /// <summary>
        /// Parses Everything but Adjective Clusters in a single line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static SynsetEntry Parse(string line)
        {
            SynsetEntry entry = new SynsetEntry();

            List<Word> wordList = new List<Word>();
            List<Pointer> pointerList = new List<Pointer>();

            const string pattern = "(\\(.*\\)|\\[.*?\\]|frames: [\\d,]*|[\\w_\\.\\:\"]+,[^\\s]{1,2}|[\\w_\\.\\:\"]+,)";
            foreach(Match match in Regex.Matches(line, pattern))
            {
                string value = match.Value;
                if (isWord(value)) wordList.Add(parseWord(value));
                else if (isPointer(value)) pointerList.Add(parsePointer(value));
                else if (isWordPointerSet(value)) wordList.Add(parseWordPointerSet(value));
                else if (isFrames(value)) entry.Frames = parseFrames(value);
                else if (isGloss(value)) entry.Gloss = parseGloss(value);
                else throw new Exception("Invalid entry in the synset database.");
            }

            entry.Words = wordList;
            entry.Pointers = pointerList;

            return entry;
        }

        // word[lex_id],
        // word[#]",
        private static Word parseWord(string value)
        {
            value = value.Trim();
            if (value.Last() != ',') throw new ArgumentException("Invalid Word String");
            value = value.Substring(0, value.Length - 1);

            int quoteIndex = value.IndexOf('"');
            if (quoteIndex > -1)
            {
                // Quote Found
                string word = value.Substring(0, quoteIndex).Replace('_', ' ');
                return new Word() { Lemma = word, LexId = 0, Pointers = null, Frames = null };
            } else
            {
                // Quote Not Found
                if (char.IsNumber(value[value.Length - 1]))
                {
                    if (char.IsNumber(value[value.Length - 2]))
                    {
                        return new Word() { Lemma = value.Substring(0, value.Length - 2).Replace('_', ' '), LexId = byte.Parse(value.Substring(value.Length - 2)), Pointers = null, Frames = null };
                    } else
                    {
                        return new Word() { Lemma = value.Substring(0, value.Length - 1).Replace('_', ' '), LexId = byte.Parse(value.Substring(value.Length - 1)), Pointers = null, Frames = null };
                    }
                } else
                {
                    return new Word() { Lemma = value.Replace('_', ' '), LexId = 0, Pointers = null, Frames = null };
                }
            }
        }

        private static string parseGloss(string value)
        {
            return value.Substring(1, value.Length - 2).Trim();
        }

        private static IEnumerable<byte> parseFrames(string value)
        {
            string frameList = value.Substring(7);
            return frameList.Split(',').Select(x => byte.Parse(x)).ToArray();
        }

        // fasten1,@
        // noun.artifact:casket1,+
        private static Pointer parsePointer(string value)
        {
            Pointer rtn = new Pointer();

            int colonIndex = value.IndexOf(':');
            if (colonIndex > -1)
            {
                Constants.LexicographerFiles fileValue = Constants.ParseFile(value.Substring(0, colonIndex));
                rtn.LexFile = fileValue;
                value = value.Substring(colonIndex + 1);
            } else
            {
                // Submit the current file as the Lex File
            }

            // Get Pointer
            int commaIndex = value.IndexOf(',');
            string pointerSymbolString = value.Substring(commaIndex + 1);
            Constants.PointSymbol pointerSymbol = Constants.PointerSymbols[pointerSymbolString];
            rtn.PointerSymbol = pointerSymbol;

            value = value.Substring(0, commaIndex);
            int quoteIndex = value.IndexOf('"');
            if (quoteIndex > -1)
            {
                // Quote Found
                string word = value.Substring(0, quoteIndex).Replace('_', ' ');
                rtn.Lemma = word;
            }
            else
            {
                // Quote Not Found
                if (char.IsNumber(value[value.Length - 1]))
                {
                    if (char.IsNumber(value[value.Length - 2]))
                    {
                        rtn.Lemma = value.Substring(0, value.Length - 2).Replace('_', ' ');
                        rtn.LexId = byte.Parse(value.Substring(value.Length - 2));
                    }
                    else
                    {
                        rtn.Lemma = value.Substring(0, value.Length - 1).Replace('_', ' ');
                        rtn.LexId = byte.Parse(value.Substring(value.Length - 1));
                    }
                }
                else
                {
                    rtn.Lemma = value.Replace('_', ' ');
                }
            }

            return rtn;
        }

        // [ casket, noun.artifact:casket1,+ noun.artifact:casket,+ ]
        private static Word parseWordPointerSet(string value)
        {
            value = value.Substring(2, value.Length - 4);
            string[] values = value.Split(' ');

            // First is always the word
            Word word = parseWord(values[0]);

            List<Pointer> pointerList = new List<Pointer>();
            for (int i = 1; i < values.Count(); i++)
            {
                if (values[i] == "frames:")
                {
                    IEnumerable<byte> frames = parseFrames(values[i] + ' ' + values[i + 1]);
                    word.Frames = frames;
                    break;
                }

                Pointer newPointer = parsePointer(values[i]);
                pointerList.Add(newPointer);
            }

            word.Pointers = pointerList;
            return word;
        }

        private static bool isWord(string match)
        {
            if (match[0] == '(' || match[0] == '[') return false;
            if (match.Last() == ',') return true;
            if (match.StartsWith("frames:")) return false;

            return false;
        }

        private static bool isGloss(string match)
        {
            if (match[0] == '(') return true;
            return false;
        }

        private static bool isWordPointerSet(string match)
        {
            if (match[0] == '[') return true;
            return false;
        }

        private static bool isPointer(string match)
        {
            if (match[0] == '[' || match[0] == '(') return false;
            if (match.Last() == ',') return false;
            if (match.StartsWith("frames:")) return false;
            return true;
        }

        private static bool isFrames(string match)
        {
            if (match.StartsWith("frames:")) return true;
            return false;
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
        public string Lemma { get; set; }
        // INFO: Adjective Only
        // public char Marker { get; set; }        // p = predicate position, a = prenominal (attributive) position, ip = immediately postnominal position (abbreviated to i)
        public byte LexId { get; set; }
        public IEnumerable<Pointer> Pointers { get; set; }
        public IEnumerable<byte> Frames { get; set; }
    }

    public struct Pointer
    {
        public Constants.LexicographerFiles LexFile { get; set; }
        public string Lemma { get; set; }
        public byte LexId { get; set; }
        public Constants.PointSymbol PointerSymbol { get; set; }
    }
}
