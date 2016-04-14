using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SparcPoint.WordNet
{
    public static class FileRetriever
    {
        public static async Task<StorageFile> GetSenseIndexFile()
        {
            return await getFileByUri("ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/index.sense");
        }

        public static async Task<StorageFile> GetExampleSentencesFile()
        {
            return await getFileByUri("ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/sents.vrb");
        }

        public static async Task<StorageFile> GetExampleSentencesIndexFile()
        {
            return await getFileByUri("ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/sentidx.vrb");
        }

        public static async Task<StorageFile> GetSyntacticCategoryDataFile(Constants.PartOfSpeech synSetType)
        {
            string filename = "";
            switch (synSetType)
            {
                case Constants.PartOfSpeech.NOUN:
                    filename = "data.noun";
                    break;
                case Constants.PartOfSpeech.VERB:
                    filename = "data.verb";
                    break;
                case Constants.PartOfSpeech.ADJECTIVE:
                case Constants.PartOfSpeech.ADJECTIVE_SATELLITE:
                    filename = "data.adj";
                    break;
                case Constants.PartOfSpeech.ADVERB:
                    filename = "data.adv";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(synSetType));
            }

            return await getFileByUri($"ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/{filename}");
        }

        public static async Task<StorageFile> GetSyntacticCategoryIndexFile(Constants.PartOfSpeech synSetType)
        {
            string filename = "";
            switch (synSetType)
            {
                case Constants.PartOfSpeech.NOUN:
                    filename = "index.noun";
                    break;
                case Constants.PartOfSpeech.VERB:
                    filename = "index.verb";
                    break;
                case Constants.PartOfSpeech.ADJECTIVE:
                case Constants.PartOfSpeech.ADJECTIVE_SATELLITE:
                    filename = "index.adj";
                    break;
                case Constants.PartOfSpeech.ADVERB:
                    filename = "index.adv";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(synSetType));
            }

            return await getFileByUri($"ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/{filename}");
        }

        public static async Task<StorageFile> GetSyntacticCategoryExceptionFile(Constants.PartOfSpeech synSetType)
        {
            string filename = "";
            switch (synSetType)
            {
                case Constants.PartOfSpeech.NOUN:
                    filename = "noun.exc";
                    break;
                case Constants.PartOfSpeech.VERB:
                    filename = "verb.exc";
                    break;
                case Constants.PartOfSpeech.ADJECTIVE:
                case Constants.PartOfSpeech.ADJECTIVE_SATELLITE:
                    filename = "adj.exc";
                    break;
                case Constants.PartOfSpeech.ADVERB:
                    filename = "adv.exc";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(synSetType));
            }

            return await getFileByUri($"ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/{filename}");
        }

        public static async Task<StorageFile> GetLexicographerFile(Constants.LexicographerFiles fileType)
        {
            // Generate Filename from Enum
            string filename = fileType.ToString().ToLower().Replace("_", ".");
            return await getFileByUri($"ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/dbfiles/{filename}");
        }

        private static async Task<StorageFile> getFileByUri(string fileUri)
        {
            return await StorageFile.GetFileFromApplicationUriAsync(new Uri(fileUri));
        }
    }
}
