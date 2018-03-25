namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    /// <summary>
    /// Contains helper methods.
    /// </summary>
    public interface IUtilityHelper
    {
        /// <summary>
        /// Replaces invalid characters in the text. A small list of them [ &lt;, &gt;, :, ", / , \, |, ?, * ].
        /// </summary>
        /// <param name="text">The text to be replaced.</param>
        /// <param name="replaceCharacter">The replace character.</param>
        /// <returns></returns>
        string ReplaceInvalidCharacters(string text, string replaceCharacter = "_");
    }
}