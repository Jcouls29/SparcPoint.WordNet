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
        public static async Task<IEnumerable<SynsetEntry>> GetAllEntries(StorageFile file)
        {
            if (file.Name.Substring(0, 7) == "adj.all") throw new NotImplementedException();

            List<SynsetEntry> rtn = new List<SynsetEntry>();
            int currentLineNumber = 0;
            string currentLine = null;
            string fileName = file.Name;

            try
            {
                using (IInputStream stream = await file.OpenSequentialReadAsync())
                using (Stream classicStream = stream.AsStreamForRead())
                using (StreamReader reader = new StreamReader(classicStream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        currentLineNumber++;
                        currentLine = line;

                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line[0] == '(') continue;

                        SynsetEntry entry = SynsetEntry.Parse(line);
                        rtn.Add(entry);
                    }
                }

                return rtn;
            } catch(Exception ex)
            {
                throw new Exception($"Failed to Get All Entries. [File '{fileName}':{currentLineNumber} = '{currentLine}']", ex);
            }
        }

        public static async Task<SynsetEntry> GetEntryAsync(StorageFile file, int byteOffset)
        {
            if (file.Name.Substring(0, 7) == "adj.all") throw new NotImplementedException();

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

        #region Adjectives (Special Case)
        public static async Task<IEnumerable<SynsetCluster>> GetAllEntriesAdjAsync(StorageFile file)
        {
            if (!(file.Name.Substring(0, 7) == "adj.all")) throw new NotImplementedException();

            List<SynsetCluster> rtn = new List<SynsetCluster>();
            int currentLineNumber = 0;
            string currentLine = null;
            string fileName = file.Name;

            try
            {
                using (IInputStream stream = await file.OpenSequentialReadAsync())
                using (Stream classicStream = stream.AsStreamForRead())
                using (StreamReader reader = new StreamReader(classicStream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        currentLineNumber++;
                        currentLine = line;

                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line[0] == '(') continue;

                        if (line[0] == '{')
                        {
                            SynsetCluster entry = SynsetCluster.Parse(line);
                            rtn.Add(entry);
                        }

                        if (line[0] == '[')
                        {
                            List<string> lines = new List<string>();
                            lines.Add(line);

                            while (!reader.EndOfStream)
                            {
                                line = await reader.ReadLineAsync();
                                currentLineNumber++;
                                currentLine = line;

                                if (string.IsNullOrWhiteSpace(line)) continue;

                                lines.Add(line);

                                if (line.Trim().Last() == ']') break;
                            }
                            SynsetCluster entry = SynsetCluster.Parse(lines.ToArray());
                            rtn.Add(entry);
                        }
                    }
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to Get All Entries. [File '{fileName}':{currentLineNumber} = '{currentLine}']", ex);
            }
        }

        public static async Task<SynsetCluster> GetEntryAdjAsync(StorageFile file, int byteOffset)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public struct SynsetClusterAdditional
    {
        public SynsetEntry HeadSynset { get; set; }
        public IEnumerable<SynsetEntry> SatelliteSynsets { get; set; }
    }

    public struct SynsetCluster
    {
        public SynsetEntry HeadSynset { get; set; }
        public IEnumerable<SynsetEntry> SatelliteSynsets { get; set; }
        public IEnumerable<SynsetClusterAdditional> AdditionalClusters { get; set; }

        public static SynsetCluster Parse(string[] lines)
        {
            SynsetCluster rtn = new SynsetCluster();

            // Ready lines for parsing
            lines[0] = lines[0].Substring(1);
            lines[lines.Length - 1] = lines[lines.Length - 1].Substring(0, lines[lines.Length - 1].Length - 1);

            // Start Head Synset Parsing
            rtn.HeadSynset = SynsetEntry.Parse(lines[0]);

            // Satellite Synsets next
            int lineIndex = 1;
            List<SynsetEntry> satellites = new List<SynsetEntry>();
            for(int i = lineIndex; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                if (lines[i][0] == '-')
                {
                    lineIndex = i + 1;
                    break;
                }

                SynsetEntry satelliteSynset = SynsetEntry.Parse(lines[i]);
                satellites.Add(satelliteSynset);
            }
            rtn.SatelliteSynsets = satellites;

            // Additional Head Synsets/Satellite Synsets
            List<SynsetClusterAdditional> additionalList = new List<SynsetClusterAdditional>();
            int startIndex = lineIndex;
            for (var i = lineIndex; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                if (lines[i][0] == '-')
                {
                    // More than 1 cluster.  Parse and continue
                    string[] subLines = lines.SubArray(startIndex, i - startIndex);
                    SynsetClusterAdditional addl = ParseAdditionalLines(subLines);
                    additionalList.Add(addl);
                    i++;
                    startIndex = i;
                }
            }

            string[] finalLines = lines.SubArray(startIndex, lines.Length - startIndex);
            SynsetClusterAdditional finalAddl = ParseAdditionalLines(finalLines);
            additionalList.Add(finalAddl);

            rtn.AdditionalClusters = additionalList;
            return rtn;
        }

        public static SynsetCluster Parse(string line)
        {
            SynsetCluster cluster = new SynsetCluster();
            cluster.HeadSynset = SynsetEntry.Parse(line);
            return cluster;
        }

        private static SynsetClusterAdditional ParseAdditionalLines(string[] lines)
        {
            SynsetClusterAdditional additionalCluster = new SynsetClusterAdditional();
            additionalCluster.HeadSynset = SynsetEntry.Parse(lines[0]);
            List<SynsetEntry> satellites = new List<SynsetEntry>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                SynsetEntry satelliteSynset = SynsetEntry.Parse(lines[i]);
                satellites.Add(satelliteSynset);
            }

            additionalCluster.SatelliteSynsets = satellites;
            return additionalCluster;
        }
    }

    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] results = new T[length];
            Array.Copy(data, index, results, 0, length);
            return results;
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

            // Ensure space before frames tag
            line = Regex.Replace(line, "(?<!\\s)frames:", " frames:");

            const string pattern = "(\\(.*\\)|\\[.*?\\]|frames:\\s*[\\d,\\s]*|[\\w_\\.\\:\"]+,[^\\s]{1,2}|[\\w_\\.\\:\"]+,)";
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
            if (value.First() == '{') throw new ArgumentException($"Invalid Word String [{value}].");
            string origValue = value;

            value = value.Trim();
            if (value.Last() != ',') throw new ArgumentException($"Invalid Word String [{value}].");
            value = value.Substring(0, value.Length - 1);

            Word rtn = new Word();
            rtn.LexId = getLexIdAndStrip(ref value);
            rtn.Lemma = value.Replace('_', ' ').Replace("\"", string.Empty);

            // Post-Checks
            if (string.IsNullOrEmpty(rtn.Lemma)) throw new Exception($"Returned Lemma is empty [{origValue}].  This indicates an incorrect parsing algorithm.");
            if (rtn.Lemma.Contains('"')) throw new Exception($"Returned Lemma contains a quote character [{origValue}].  This indicates an incorrect parsing algorithm.");
            if (rtn.LexId < 0 || rtn.LexId > 15) throw new Exception($"Returned Lex Id must be between 1 and 15 [{origValue}]. This indicates an incorrect parsing algorithm.");

            return rtn;
        }

        private static string parseGloss(string value)
        {
            return value.Substring(1, value.Length - 2).Trim();
        }

        private static IEnumerable<byte> parseFrames(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            try
            {
                value = value.Replace("frames:", "");
                return value.Split(',').Select(x => byte.Parse(x.Trim())).ToArray();
            } catch (Exception ex)
            {
                throw new Exception($"Failed to parse Frames [{value}].", ex);
            }
            
        }

        // fasten1,@
        // noun.artifact:casket1,+
        // adj.all:hostile1^opponent,+
        private static Pointer parsePointer(string value)
        {
            string origValue = value;

            try
            {
                Pointer rtn = new Pointer();

                int colonIndex = value.IndexOf(':');
                if (colonIndex > -1)
                {
                    Constants.LexicographerFiles fileValue = Constants.ParseFile(value.Substring(0, colonIndex).Trim());
                    rtn.LexFile = fileValue;
                    value = value.Substring(colonIndex + 1).Trim();
                }
                else
                {
                    // Submit the current file as the Lex File
                }

                // Get Pointer
                int commaIndex = value.IndexOf(',');
                string pointerSymbolString = value.Substring(commaIndex + 1);
                Constants.PointSymbol pointerSymbol = Constants.PointerSymbols[pointerSymbolString];
                rtn.PointerSymbol = pointerSymbol;

                value = value.Substring(0, commaIndex);

                int caretIndex = value.IndexOf('^');
                if (caretIndex > -1)
                {
                    string satelliteLemma = value.Substring(caretIndex + 1);
                    rtn.SatelliteLexId = getLexIdAndStrip(ref satelliteLemma);
                    rtn.SatelliteLemma = satelliteLemma.Replace('_', ' ').Replace("\"", string.Empty);
                    value = value.Substring(0, caretIndex);

                    if (string.IsNullOrEmpty(rtn.SatelliteLemma)) throw new Exception($"Returned Lemma is empty [{origValue}].  This indicates an incorrect parsing algorithm.");
                    if (rtn.SatelliteLemma.Contains('"')) throw new Exception($"Returned Satellite Lemma contains a quote character [{origValue}].  This indicates an incorrect parsing algorithm.");
                    if (rtn.SatelliteLexId < 0 || rtn.LexId > 15) throw new Exception($"Returned Satellite Lex Id must be between 1 and 15 [{origValue}]. This indicates an incorrect parsing algorithm.");
                }

                rtn.LexId = getLexIdAndStrip(ref value);
                rtn.Lemma = value.Replace('_', ' ').Replace("\"", string.Empty);

                // Post-Checks
                if (string.IsNullOrEmpty(rtn.Lemma)) throw new Exception($"Returned Lemma is empty [{origValue}].  This indicates an incorrect parsing algorithm.");
                if (rtn.Lemma.Contains('"')) throw new Exception($"Returned Lemma contains a quote character [{origValue}].  This indicates an incorrect parsing algorithm.");
                if (rtn.LexId < 0 || rtn.LexId > 15) throw new Exception($"Returned Lex Id must be between 1 and 15 [{origValue}]. This indicates an incorrect parsing algorithm.");

                return rtn;
            } catch (Exception ex)
            {
                throw new Exception($"Failed to parse pointer [{origValue}].", ex);
            }
        }

        // Assumes only the word/lexId is provided
        // casket1
        // code5"
        private static byte getLexIdAndStrip(ref string value)
        {
            if (value.Last() == '"')
            {
                value = value.Substring(0, value.Length - 1);
                return 0;
            }

            if (char.IsNumber(value[value.Length - 1]))
            {
                if (char.IsNumber(value[value.Length - 2]))
                {
                    string newValue = value.Substring(0, value.Length - 2);
                    byte lexId = byte.Parse(value.Substring(value.Length - 2));
                    value = newValue;
                    return lexId;
                } else
                {
                    string newValue = value.Substring(0, value.Length - 1);
                    byte lexId = byte.Parse(value.Substring(value.Length - 1));
                    value = newValue;
                    return lexId;
                }
            } else
            {
                return 0;
            }
        }

        // [ casket, noun.artifact:casket1,+ noun.artifact:casket,+ ]
        private static Word parseWordPointerSet(string value)
        {
            string origValue = value;
            value = value.Substring(1, value.Length - 2).Trim();

            // Before we split let's make sure the line is formatted properly
            // Sometimes a colon may have spaces around it.  Let's fix this
            value = Regex.Replace(value, "\\s*:\\s*", ":");
            
            try
            {
                // Pull out frames ahead of time
                int framesIndex = value.IndexOf("frames:");
                string framesString = null;
                if (framesIndex > -1)
                {
                    framesString = value.Substring(framesIndex).Replace(" ", "");
                    value = value.Substring(0, framesIndex);
                }

                string[] values = value.Split(' ');

                // First is always the word
                Word word = parseWord(values[0]);

                List<Pointer> pointerList = new List<Pointer>();
                for (int i = 1; i < values.Count(); i++)
                {
                    if (string.IsNullOrWhiteSpace(values[i])) continue;

                    Pointer newPointer = parsePointer(values[i]);
                    pointerList.Add(newPointer);
                }

                if (framesString != null)
                {
                    // Remove Frames from value
                    IEnumerable<byte> frames = parseFrames(framesString);
                    word.Frames = frames;
                }

                word.Pointers = pointerList;
                return word;
            } catch (Exception ex)
            {
                throw new Exception($"Could not parse Word Pointer Set [{origValue}].", ex);
            }

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
    }

    public struct Word
    {
        public string Lemma { get; set; }
        // INFO: Adjective Only
        public char Marker { get; set; }        // p = predicate position, a = prenominal (attributive) position, ip = immediately postnominal position (abbreviated to i)
        public byte LexId { get; set; }
        public IEnumerable<Pointer> Pointers { get; set; }
        public IEnumerable<byte> Frames { get; set; }
    }

    public struct Pointer
    {
        public Constants.LexicographerFiles LexFile { get; set; }
        public string Lemma { get; set; }       // Head SynSet
        public string SatelliteLemma { get; set; }
        public byte LexId { get; set; }
        public byte SatelliteLexId { get; set; }
        public Constants.PointSymbol PointerSymbol { get; set; }
    }
}
