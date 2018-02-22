namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    public interface IUtilityHelper
    {
        string ReplaceInvlaidCharacters(string text, string replaceCharacter = "_");
    }
}