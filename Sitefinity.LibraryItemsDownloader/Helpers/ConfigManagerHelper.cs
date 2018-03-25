namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Data;

    public class ConfigManagerHelper : IConfigManagerHelper
    {
        private readonly ConfigManager configManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sitefinity.LibraryItemsDownloader.Helpers.ConfigManagerHelper"/> class.
        /// </summary>
        /// <param name="configManager">Instance of the <see cref="Telerik.Sitefinity.Configuration.ConfigManager"/> from Sitefinity.</param>
        public ConfigManagerHelper(ConfigManager configManager)
        {
            this.configManager = configManager;
        }

        /// <summary>
        /// Returns the <see cref="Telerik.Sitefinity.Configuration.ConfigManager"/>.
        /// </summary>
        public IManager Manager
        {
            get
            {
                return this.configManager;
            }
        }

        /// <summary>
        /// Returns the desired section.
        /// </summary>
        /// <typeparam name="TSection">Section witch inherits <seealso cref="Telerik.Sitefinity.Configuration.ConfigSection" /></typeparam>
        /// <returns>TSection</returns>
        public TSection GetSection<TSection>() where TSection : ConfigSection, new()
        {
            return this.configManager.GetSection<TSection>();
        }

        /// <summary>
        /// Saves the changes related to the provided section. If <paramref name="useFileSystemMode"/> is true it will save changes in config files.
        /// </summary>
        /// <param name="section">Modified section to save.</param>
        /// <param name="useFileSystemMode">Whether to save section in file system or in database.</param>
        public void SaveSection(ConfigSection section)
        {
            this.configManager.SaveSection(section);
        }
    }
}
