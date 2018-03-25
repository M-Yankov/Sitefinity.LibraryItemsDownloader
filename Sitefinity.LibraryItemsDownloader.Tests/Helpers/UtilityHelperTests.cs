namespace Sitefinity.LibraryItemsDownloader.Tests.Helpers
{
    using NUnit.Framework;
    using Sitefinity.LibraryItemsDownloader.Helpers;

    public class UtilityHelperTests
    {
        private UtilityHelper helper;

        [SetUp]
        public void Initialize()
        {
            this.helper = new UtilityHelper();
        }

        [Test]
        public void ExpectReplaceInvlaidCharactersToReturnEmptyStringWhenTextIsNull()
        {
            string result = this.helper.ReplaceInvalidCharacters(null);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ExpectReplaceInvlaidCharactersToReturnTheSameValueWhenItDoesNotContainSpecialSymbols()
        {
            string text = ".NETCORE2.0.1";
            string result = this.helper.ReplaceInvalidCharacters(text);
            Assert.AreEqual(text, result);
        }

        [Test]
        public void ExpectReplaceInvlaidCharactersToReplaceSpecialSymbolsWithDefaultReplacer()
        {
            string text = "?<>.NETCORE2.0.1 |:*";
            string result = this.helper.ReplaceInvalidCharacters(text);
            Assert.AreEqual("___.NETCORE2.0.1 ___", result);
        }

        [Test]
        public void ExpectReplaceInvlaidCharactersToReplaceSpecialSymbolsWithProvidedReplacer()
        {
            string text = "?<>.NETCORE2.0.1 |:*";
            string result = this.helper.ReplaceInvalidCharacters(text, " ");
            Assert.AreEqual("   .NETCORE2.0.1    ", result);
        }

        [Test]
        public void ExpectReplaceInvlaidCharactersToRemoveSpecialSymbolsWithProvidedReplacer()
        {
            string text = "?<>.NETCORE2.0.1 |:*";
            string result = this.helper.ReplaceInvalidCharacters(text, string.Empty);
            Assert.AreEqual(".NETCORE2.0.1 ", result);
        }
    }
}
