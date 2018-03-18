namespace Sitefinity.LibraryItemsDownloader.Tests.Stubs
{
    using System.Collections.Generic;
    using Telerik.Sitefinity.Configuration;

    public class ConfigElementListStub<TElement> : ConfigElementList<TElement> where TElement : ConfigElement
    {
        public ConfigElementListStub(ConfigElement parent) : base(parent)
        {
        }

        public override void AddLinkedElement(object key, string path, string moduleName = null)
        {
            // Get element From Dictionary 
            TElement element = (TElement)this.GetElementByKey(key.ToString());

            ConfigElementItem<TElement> configElement = new ConfigElementItem<TElement>(key.ToString(), element);
            (this.Items as List<ConfigElementItem<TElement>>).Add(configElement);
        }

        public void InsertInDictionary(IConfigElementItem item, string key)
        {
            this.OnItemInserted(item, key);
        }
    }

}
