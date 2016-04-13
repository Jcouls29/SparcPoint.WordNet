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

        public static async Task<StorageFile> GetSyntacticCategoryDataFile(Constants.SynSetType synSetType)
        {
            string filename = "";
            switch (synSetType)
            {
                case Constants.SynSetType.NOUN:
                    filename = "data.noun";
                    break;
                case Constants.SynSetType.VERB:
                    filename = "data.verb";
                    break;
                case Constants.SynSetType.ADJECTIVE:
                case Constants.SynSetType.ADJECTIVE_SATELLITE:
                    filename = "data.adj";
                    break;
                case Constants.SynSetType.ADVERB:
                    filename = "data.adv";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(synSetType));
            }

            return await getFileByUri($"ms-appx:///SparcPoint.WordNet.Core/WordNet/dict/{filename}");
        }

        public static async Task<StorageFile> GetSyntacticCategoryIndexFile(Constants.SynSetType synSetType)
        {
            string filename = "";
            switch (synSetType)
            {
                case Constants.SynSetType.NOUN:
                    filename = "index.noun";
                    break;
                case Constants.SynSetType.VERB:
                    filename = "index.verb";
                    break;
                case Constants.SynSetType.ADJECTIVE:
                case Constants.SynSetType.ADJECTIVE_SATELLITE:
                    filename = "index.adj";
                    break;
                case Constants.SynSetType.ADVERB:
                    filename = "index.adv";
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
