namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Data;

    /// <summary>
    /// This interface is a wrapper for ConfigManager
    /// </summary>
    public interface IConfigManagerHelper
    {
        /// <summary>
        /// Returns the CofnigManger
        /// </summary>
        IManager Manager { get; }

        /// <summary>
        /// Returns the desired section. 
        /// </summary>
        /// <typeparam name="TSection">Section witch inherits <seealso cref="Telerik.Sitefinity.Configuration.ConfigSection"/></typeparam>
        /// <returns>TSection</returns>
        TSection GetSection<TSection>() where TSection : ConfigSection, new();

        /// <summary>
        /// Saves the changes related to the provided section. If <paramref name= "useFileSystemMode" /> is true it will save changes in config files.
        /// </summary>
        /// <param name="section">Modified section to save.</param>
        void SaveSection(ConfigSection section);
    }
}