namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class UtilityHelper : IUtilityHelper
    {
        private const string DefaultReplaceCharacter = "_";
        private HashSet<char> reservedSymbols = null;

        public UtilityHelper()
        {
            IEnumerable<char> specialSymbols = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToList();
            this.reservedSymbols = new HashSet<char>(specialSymbols);
        }

        public string ReplaceInvlaidCharacters(string text, string replaceCharacter = DefaultReplaceCharacter)
        {
            StringBuilder textResult = new StringBuilder();
            foreach (char currentSymbol in text)
            {
                if (this.reservedSymbols.Contains(currentSymbol))
                {
                    textResult.Append(replaceCharacter);
                }
                else
                {
                    textResult.Append(currentSymbol);
                }
            }

            return textResult.ToString();
        }
    }
}
