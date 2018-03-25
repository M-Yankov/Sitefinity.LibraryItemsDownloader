namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Contains helper methods.
    /// </summary>
    /// <seealso cref="Sitefinity.LibraryItemsDownloader.Helpers.IUtilityHelper" />
    public class UtilityHelper : IUtilityHelper
    {
        private const string DefaultReplaceCharacter = "_";
        private HashSet<char> reservedSymbols = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityHelper"/> class.
        /// Initializes the symbols to work with.
        /// </summary>
        public UtilityHelper()
        {
            IEnumerable<char> specialSymbols = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToList();
            this.reservedSymbols = new HashSet<char>(specialSymbols);
        }

        /// <summary>
        /// Replaces invalid characters in the text. A small list of them [ &lt;, &gt;, :, ", / , \, |, ?, * ].
        /// </summary>
        /// <param name="text">The text to be replaced.</param>
        /// <param name="replaceCharacter">The replace character.</param>
        /// <returns>Text without invalid symbols.</returns>
        public string ReplaceInvalidCharacters(string text, string replaceCharacter = DefaultReplaceCharacter)
        {
            if (text == null)
            {
                text = string.Empty;
            }

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
