using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparcPoint.WordNet
{
    public static class Constants
    {
        public enum PartOfSpeech: byte
        {
            UNKNOWN = 0,
            NOUN = 1,
            VERB = 2,
            ADJECTIVE = 3,
            ADVERB = 4,
            ADJECTIVE_SATELLITE = 5
        }

        public static Dictionary<char, PartOfSpeech> SynSetTypeChar = new Dictionary<char, PartOfSpeech>()
        {
            {'n', PartOfSpeech.NOUN },
            {'v', PartOfSpeech.VERB },
            {'a', PartOfSpeech.ADJECTIVE },
            {'s', PartOfSpeech.ADJECTIVE_SATELLITE },
            {'r', PartOfSpeech.ADVERB }
        };

        public enum SyntacticCategory: byte
        {
            UNKNOWN = 0,
            NOUN = 1,
            VERB = 2,
            ADJECTIVE = 3,
            ADVERB = 4
        }

        public enum PointSymbol: byte
        {
            ANTONYM,
            HYPERNYM,
            INSTANCE_HYPERNYM,
            HYPONYM,
            INSTANCE_HYPONYM,
            MEMBER_HOLONYM,
            SUBSTANCE_HOLONYM,
            PART_HOLONYM,
            MEMBER_MERONYM,
            SUBSTANCE_MERONYM,
            PART_MERONYM,
            ATTRIBUTE,
            DERIVATIONALLY_RELATED_FORM,
            DOMAIN_OF_SYNSET_TOPIC,
            MEMBER_OF_THIS_DOMAIN_TOPIC,
            DOMAIN_OF_SYNSET_REGION,
            MEMBER_OF_THIS_DOMAIN_REGION,
            DOMAIN_OF_SYNSET_USAGE,
            MEMBER_OF_THIS_DOMAIN_USAGE,
            ENTAILMENT,
            CAUSE,
            ALSO_SEE,
            VERB_GROUP,
            SIMILAR_TO,
            PARTICIPLE_OF_VERB,
            PERTAINYM,
            DERIVED_FROM_ADJECTIVE,
        }

        public static Dictionary<string, PointSymbol> PointerSymbols = new Dictionary<string, PointSymbol>()
        {
            {"!", PointSymbol.ANTONYM },
            {"@", PointSymbol.HYPERNYM },
            {"@i", PointSymbol.INSTANCE_HYPERNYM },
            {"~", PointSymbol.HYPONYM },
            {"~i", PointSymbol.INSTANCE_HYPONYM },
            {"#m", PointSymbol.MEMBER_HOLONYM },
            {"#s", PointSymbol.SUBSTANCE_HOLONYM },
            {"#p", PointSymbol.PART_HOLONYM },
            {"%m", PointSymbol.MEMBER_MERONYM },
            {"%s", PointSymbol.SUBSTANCE_MERONYM },
            {"%p", PointSymbol.PART_MERONYM },
            {"=", PointSymbol.ATTRIBUTE },
            {"+", PointSymbol.DERIVATIONALLY_RELATED_FORM },
            {";c", PointSymbol.DOMAIN_OF_SYNSET_TOPIC },
            {"-c", PointSymbol.MEMBER_OF_THIS_DOMAIN_TOPIC },
            {";r", PointSymbol.DOMAIN_OF_SYNSET_REGION },
            {"-r", PointSymbol.MEMBER_OF_THIS_DOMAIN_REGION },
            {";u", PointSymbol.DOMAIN_OF_SYNSET_USAGE },
            {"-u", PointSymbol.MEMBER_OF_THIS_DOMAIN_USAGE },
            {"*", PointSymbol.ENTAILMENT },
            {">", PointSymbol.CAUSE },
            {"^", PointSymbol.ALSO_SEE },
            {"$", PointSymbol.VERB_GROUP },
            {"<", PointSymbol.PARTICIPLE_OF_VERB },
            {"\\", PointSymbol.PERTAINYM },     // Also Known as Derived From Adjective
            {"&", PointSymbol.SIMILAR_TO }
        };

        public static Dictionary<byte, string> VerbFrameFormats = new Dictionary<byte, string>()
        {
            {1, "Something {0}s" },
            {2, "Somebody {0}s" },
            {3, "It is {0}ing" },
            {4, "Something is {0}ing PP" },
            {5, "Something {0}s something Adjective/Noun" },
            {6, "Something {0}s Adjective/Noun" },
            {7, "Somebody {0}s Adjective" },
            {8, "Somebody {0}s something" },
            {9, "Somebody {0}s somebody" },
            {10, "Something {0}s somebody" },
            {11, "Something {0}s something" },
            {12, "Something {0}s to somebody" },
            {13, "Somebody {0}s on something" },
            {14, "Somebody {0}s somebody something" },
            {15, "Somebody {0}s something to somebody" },
            {16, "Somebody {0}s something from somebody" },
            {17, "Somebody {0}s somebody with something" },
            {18, "Somebody {0}s somebody of something" },
            {19, "Somebody {0}s something on somebody" },
            {20, "Somebody {0}s somebody PP" },
            {21, "Somebody {0}s something PP" },
            {22, "Somebody {0}s PP" },
            {23, "Somebody's (body part) {0}s" },
            {24, "Somebody {0}s somebody to INFINITIVE" },
            {25, "Somebody {0}s somebody INFINITIVE" },
            {26, "Somebody {0}s that CLAUSE" },
            {27, "Somebody {0}s to somebody" },
            {28, "Somebody {0}s to INFINITIVE" },
            {29, "Somebody {0}s whether INFINITIVE" },
            {30, "Somebody {0}s somebody into V-ing something" },
            {31, "Somebody {0}s something with something" },
            {32, "Somebody {0}s INFINITIVE" },
            {33, "Somebody {0}s VERB-ing" },
            {34, "It {0}s that CLAUSE" },
            {35, "Something {0}s INFINITIVE" },
        };

        /// <summary>
        /// Lexicographer File Enumeration
        /// </summary>
        /// <remarks>
        /// Names of the enums take the form of [POS]_[EXT] which translates
        /// to the file: 
        /// 
        /// ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/dbfiles/[pos].[ext]
        /// 
        /// </remarks>
        public enum LexicographerFiles: byte
        {
            ADJ_ALL = 00,
            ADJ_PERT = 01,
            ADV_ALL = 02,
            NOUN_TOPS = 03,
            NOUN_ACT = 04,
            NOUN_ANIMAL = 05,
            NOUN_ARTIFACT = 06,
            NOUN_ATTRIBUTE = 07,
            NOUN_BODY = 08,
            NOUN_COGNITION = 09,
            NOUN_COMMUNICATION = 10,
            NOUN_EVENT = 11,
            NOUN_FEELING = 12,
            NOUN_FOOD = 13,
            NOUN_GROUP = 14,
            NOUN_LOCATION = 15,
            NOUN_MOTIVE = 16,
            NOUN_OBJECT = 17,
            NOUN_PERSON = 18,
            NOUN_PHENOMENON = 19,
            NOUN_PLANT = 20,
            NOUN_POSSESSION = 21,
            NOUN_PROCESS = 22,
            NOUN_QUANTITY = 23,
            NOUN_RELATION = 24,
            NOUN_SHAPE = 25,
            NOUN_STATE = 26,
            NOUN_SUBSTANCE = 27,
            NOUN_TIME = 28,
            VERB_BODY = 29,
            VERB_CHANGE = 30,
            VERB_COGNITION = 31,
            VERB_COMMUNICATION = 32,
            VERB_COMPETITION = 33,
            VERB_CONSUMPTION = 34,
            VERB_CONTACT = 35,
            VERB_CREATION = 36,
            VERB_EMOTION = 37,
            VERB_MOTION = 38,
            VERB_PERCEPTION = 39,
            VERB_POSSESSION = 40,
            VERB_SOCIAL = 41,
            VERB_STATIVE = 42,
            VERB_WEATHER = 43,
            ADJ_PPL = 44
        }

        public static LexicographerFiles ParseFile(string value)
        {
            string changedValue = value.Replace('.', '_').ToUpper().Trim();
            return (LexicographerFiles)Enum.Parse(typeof(LexicographerFiles), changedValue);
        }
    }
}
