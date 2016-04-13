using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparcPoint.WordNet
{
    /// <summary>
    /// Sense Key
    /// </summary>
    /// <remarks>
    /// Format: lemma%ss_type:lex_filenum:lex_id:head_word:head_id
    /// </remarks>
    public struct SenseKey
    {
        public string FullKey { get; set; }
        public string Lemma { get; set; }
        public Constants.SynSetType PartOfSpeech { get; set; }
        public Constants.LexicographerFiles LexFile { get; set; }
        public byte LexId { get; set; }
        public string HeadWord { get; set; }
        public byte HeadId { get; set; }

        public static SenseKey Parse(string key)
        {
            SenseKey rtn = new SenseKey();

            // Manual Fast Parsing of String
            // Lemma
            int lastIndex = -1;
            string nextField = ParseHelper.GetNextField(key, ref lastIndex, '%');
            rtn.Lemma = nextField.Replace("_", " ");

            // Synset Type            
            nextField = ParseHelper.GetNextField(key, ref lastIndex, ':');
            rtn.PartOfSpeech = (Constants.SynSetType) Enum.Parse(typeof(Constants.SynSetType), nextField);

            // Lexicographical File
            nextField = ParseHelper.GetNextField(key, ref lastIndex, ':');
            rtn.LexFile = (Constants.LexicographerFiles)Enum.Parse(typeof(Constants.LexicographerFiles), nextField);

            // Lex Id
            nextField = ParseHelper.GetNextField(key, ref lastIndex, ':');
            rtn.LexId = byte.Parse(nextField);

            // Head Word
            nextField = ParseHelper.GetNextField(key, ref lastIndex, ':');
            rtn.HeadWord = nextField;

            // Head Id
            nextField = ParseHelper.GetLastField(key, lastIndex);
            if (nextField == string.Empty)
                rtn.HeadId = 0;
            else
                rtn.HeadId = byte.Parse(nextField);

            rtn.FullKey = key;

            return rtn;
        }
    }
}
