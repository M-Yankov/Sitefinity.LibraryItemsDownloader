namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Data;

    public class ConfigManagerHelper : IConfigManagerHelper
    {
        private readonly ConfigManager configManager;

        public ConfigManagerHelper(ConfigManager configManager)
        {
            this.configManager = configManager;
        }

        public IManager Manager
        {
            get
            {
                return this.configManager;
            }
        }

        public TSection GetSection<TSection>() where TSection : ConfigSection, new()
        {
            return this.configManager.GetSection<TSection>();
        }

        public void SaveSection(ConfigSection section)
        {
            this.configManager.SaveSection(section);
        }
    }
}
