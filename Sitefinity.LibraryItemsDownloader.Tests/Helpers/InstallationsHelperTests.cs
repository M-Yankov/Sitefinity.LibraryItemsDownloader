namespace Sitefinity.LibraryItemsDownloader.Tests.Helpers
{
    
    using NUnit.Framework;
    using Moq;
    using Sitefinity;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Modules.Libraries.Configuration;
    using Sitefinity.LibraryItemsDownloader.Helpers;
    using Telerik.Sitefinity.Data;
    using System.Linq.Expressions;
    using System;
    using Telerik.Sitefinity.Web.UI.ContentUI.Config;
    using System.Collections.Generic;
    using System.Reflection;

    using System.Configuration;
    using System.ComponentModel;
    using Telerik.Sitefinity.Web.UI.ContentUI.Views.Backend.Master.Config;
    using Telerik.Sitefinity.Web.UI.Backend.Elements.Config;

    [TestFixture]
    public class InstallationsHelperTests
    {
        private const string ContentViewControls = "contentViewControls";
        private const string DefaultParentTagName = "TestTagName";
        private const string DefaultViewName = "ImageBackendListTest";
        private const string DefaultCommandName = "JavaScript";
        private Mock<LibrariesConfig> librariesConfigMock;
        private Mock<ActionMenuWidgetElement> actionMenuWidgetMock;
        private Mock<WidgetBarSectionElement> widgetBarSectionMock;
        private Mock<WidgetBarElement> widgetBarElementMock;
        private Mock<MasterGridViewElement> masterGridViewMock;

        private ConfigSection parentSection;

        [SetUp]
        public void Initialize()
        {
            // Common configuration element used for initializing other configuration elements.
            this.parentSection = new ConfigSection(DefaultParentTagName);

            // Setup all collections and configuration sections.
            this.librariesConfigMock = new Mock<LibrariesConfig>();

            ConfigElementDictionary<string, ContentViewControlElement> contentViewControls = new ConfigElementDictionary<string, ContentViewControlElement>(this.parentSection);

            ContentViewControlElement backendDefinition = new ContentViewControlElement(contentViewControls);
            this.masterGridViewMock = new Mock<MasterGridViewElement>(backendDefinition.ViewsConfig);
            this.widgetBarElementMock = new Mock<WidgetBarElement>(this.masterGridViewMock.Object);
            this.widgetBarSectionMock = new Mock<WidgetBarSectionElement>(this.widgetBarElementMock.Object);
            this.widgetBarSectionMock.CallBase = true;
            this.widgetBarSectionMock.Object.Name = "toolbar";
            this.widgetBarSectionMock.Object.ModuleName = "toolbar";
            this.widgetBarSectionMock.Setup(w => w.GetPath()).Returns("ToolbarWidgetPath");
            this.widgetBarSectionMock.Setup(w => w.TagName).Returns("ToolbarWidgetTag");

            this.actionMenuWidgetMock = new Mock<ActionMenuWidgetElement>(this.widgetBarSectionMock.Object);
            this.actionMenuWidgetMock.CallBase = true;
            this.actionMenuWidgetMock.Object.Name = "MoreActionsWidget";
            this.actionMenuWidgetMock.Setup(w => w.GetPath()).Returns("MoreActionsWidgetPath");
            this.actionMenuWidgetMock.Setup(w => w.TagName).Returns("MoreActionsWidgetTag");

            Mock<CommandWidgetElement> commandWidgetElementMock = new Mock<CommandWidgetElement>(this.actionMenuWidgetMock.Object);
            commandWidgetElementMock.CallBase = true;
            commandWidgetElementMock.Object.Name = DefaultCommandName;
            commandWidgetElementMock.Setup(w => w.GetPath()).Returns("CommandWidgetPath");
            commandWidgetElementMock.Setup(w => w.TagName).Returns("CommandWidgetTag");
            var sectionElements = new ConfigElementListStub<WidgetBarSectionElement>(this.widgetBarElementMock.Object);
            var widgetBarSEction = new ConfigElementItem<WidgetBarSectionElement>(this.widgetBarSectionMock.Object.GetKey(), this.widgetBarSectionMock.Object);
            sectionElements.InsertInDictionary(widgetBarSEction, this.widgetBarSectionMock.Object.GetKey());
            sectionElements.Add(this.widgetBarSectionMock.Object);

            var widgetBarItems = new ConfigElementListStub<WidgetElement>(this.widgetBarSectionMock.Object);
            var actionMenuItem = new ConfigElementItem<WidgetElement>(this.actionMenuWidgetMock.Object.GetKey(), this.actionMenuWidgetMock.Object);
            widgetBarItems.InsertInDictionary(actionMenuItem, this.actionMenuWidgetMock.Object.GetKey());
            widgetBarItems.Add(this.actionMenuWidgetMock.Object);

            var actionMenuItems = new ConfigElementListStub<WidgetElement>(this.actionMenuWidgetMock.Object);
            var commandWidgetItem = new ConfigElementItem<WidgetElement>(commandWidgetElementMock.Object.GetKey(), commandWidgetElementMock.Object);
            actionMenuItems.InsertInDictionary(commandWidgetItem, commandWidgetElementMock.Object.GetKey());
            actionMenuItems.Add(commandWidgetElementMock.Object);

            this.actionMenuWidgetMock.Setup(config => config["menuItems"]).Returns(actionMenuItems);
            this.widgetBarSectionMock.Setup(config => config["items"]).Returns(widgetBarItems);
            this.widgetBarElementMock.Setup(element => element["sections"]).Returns(sectionElements);
            this.masterGridViewMock.Setup(masterView => masterView["toolbar"]).Returns(this.widgetBarElementMock.Object);

            backendDefinition.ViewsConfig[DefaultViewName] = masterGridViewMock.Object;
            contentViewControls.Add(ContentViewControls, backendDefinition);
            this.librariesConfigMock.Setup(config => config[ContentViewControls]).Returns(contentViewControls);
        }

        [Test]
        public void ExprectInitializationMethodToNotThrowAnException()
        {
            Mock<IConfigManagerHelper> configManagerMock = new Mock<IConfigManagerHelper>();
            Mock<IManager> manager = new Mock<IManager>();
            configManagerMock.Setup(config => config.Manager).Returns(manager.Object);

            Mock<InstallationsHelper> installationsHelperMock = new Mock<InstallationsHelper>(configManagerMock.Object);
            installationsHelperMock.CallBase = true;

            Expression<Action<InstallationsHelper>> setupConfigureLibrarySectionMethod =
                (helper) => helper.ConfigureLibrarySection(It.IsAny<IConfigManagerHelper>(), It.IsAny<LibrariesConfig>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());


            installationsHelperMock
                .Setup(setupConfigureLibrarySectionMethod)
                .Verifiable();

            // Act
            installationsHelperMock.Object.Initialize();

            // Assert
            installationsHelperMock
                .Verify(setupConfigureLibrarySectionMethod, Times.Exactly(3));
        }

        #region ConfigureLibrarySection Tests
        [Test]
        public void ExpectConfigureLibrarySectionToThrowAnExceptionWhenDefinitionNameIsMissing()
        {
            Mock<LibrariesConfig> librariesConfig = new Mock<LibrariesConfig>();

            var contentViewControls = new ConfigElementDictionary<string, ContentViewControlElement>(this.parentSection);
            librariesConfig.Setup(config => config[ContentViewControls]).Returns(contentViewControls);

            Assert.Throws<NullReferenceException>(() =>
            {
                InstallationsHelper installationsHelper = new InstallationsHelper(null);
                installationsHelper.ConfigureLibrarySection(null, librariesConfig.Object, null, null, null, null);
            },
            "Cannot find content view control: NULL related with libraries configuration.");
        }

        [Test]
        public void ExpectConfigureLibrarySectionToThrowAnExceptionWhenLibrariesConfigIsNull()
        {
            string testDefinitionName = "DefinitonName";
            Assert.Throws<NullReferenceException>(() =>
            {
                InstallationsHelper installationsHelper = new InstallationsHelper(null);
                installationsHelper.ConfigureLibrarySection(null, null, testDefinitionName, null, null, null);
            },
            $"Cannot find content view control: {testDefinitionName} related with libraries configuration.");
        }

        [Test]
        public void ExpectConfigureLibrarySectionToThrowAnExceptionWhenDefinitionNameDoesNotExist()
        {
            Mock<LibrariesConfig> librariesConfig = new Mock<LibrariesConfig>();

            var contentViewControls = new ConfigElementDictionary<string, ContentViewControlElement>(this.parentSection);
            librariesConfig.Setup(config => config[ContentViewControls]).Returns(contentViewControls);

            string testDefinitionName = "BackEnd";
            Assert.Throws<NullReferenceException>(() =>
            {
                InstallationsHelper installationsHelper = new InstallationsHelper(null);
                installationsHelper.ConfigureLibrarySection(null, librariesConfig.Object, testDefinitionName, null, null, null);
            },
            $"Cannot find content view control: {testDefinitionName} related with libraries configuration.");
        }
        
        [Test]
        public void ExpectConfigureLibrarySectionToThrowAnExceptionWhenBackEndListViewNameIsNull()
        {
            Mock<LibrariesConfig> librariesConfig = new Mock<LibrariesConfig>();
            var contentViewControls = new ConfigElementDictionary<string, ContentViewControlElement>(this.parentSection);
            librariesConfig.Setup(config => config[ContentViewControls]).Returns(contentViewControls);

            Assert.Throws<NullReferenceException>(() =>
            {
                InstallationsHelper installationsHelper = new InstallationsHelper(null);
                installationsHelper.ConfigureLibrarySection(null, librariesConfig.Object, ContentViewControls, null, null, null);
            },
            "Cannot find back-end view: NULL.");
        }

        [Test]
        public void ExpectConfigureLibrarySectionToThrowAnExceptionWhenBackEndListViewNameDoesNotExist()
        {
            Mock<LibrariesConfig> librariesConfig = new Mock<LibrariesConfig>();

            var contentViewControls = new ConfigElementDictionary<string, ContentViewControlElement>(this.parentSection);
            librariesConfig.Setup(config => config[ContentViewControls]).Returns(contentViewControls);

            string notExistingViewName = "BackEndTest";
            Assert.Throws<NullReferenceException>(() =>
            {
                InstallationsHelper installationsHelper = new InstallationsHelper(null);
                installationsHelper.ConfigureLibrarySection(null, librariesConfig.Object, ContentViewControls, notExistingViewName, null, null);
            },
            $"Cannot find back-end view: {notExistingViewName}.");
        }

        [Test]
        public void ExpectConfigureLibrarySectionToDoNotCallSaveSectiosMethodWhenSectionsAreConfiguredExist()
        {
            // Arrange
            Mock<InstallationsHelper> installationsHelperMock = new Mock<InstallationsHelper>(null);
            installationsHelperMock.CallBase = true;
            
            installationsHelperMock
                .Setup(helper => helper.AddOrUpdateScriptReference(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<ConfigElementDictionary<string, ClientScriptElement>>()))
                .Returns(false);

            installationsHelperMock
                .Setup(helper => helper.GetJavaScriptQualifiedNameKey(It.IsAny<Assembly>(), It.IsAny<string>()))
                .Returns("SampleSciptKey");
           
            Mock<IConfigManagerHelper> managerHelperMock = new Mock<IConfigManagerHelper>();

            // Act
            installationsHelperMock.Object.ConfigureLibrarySection(managerHelperMock.Object, this.librariesConfigMock.Object, ContentViewControls, DefaultViewName, DefaultCommandName, null);

            // Assert
            managerHelperMock.Verify(helper => helper.SaveSection(It.IsAny<ConfigSection>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ExpectConfigureLibrarySectionToCallSaveSectionsOnlyOnceWhenUpdatingScriptReference()
        {
            // Arrange
            Mock<InstallationsHelper> installationsHelperMock = new Mock<InstallationsHelper>(null);
            installationsHelperMock.CallBase = true;

            installationsHelperMock
                .Setup(helper => helper.AddOrUpdateScriptReference(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<ConfigElementDictionary<string, ClientScriptElement>>()))
                .Returns(true);

            installationsHelperMock
                .Setup(helper => helper.GetJavaScriptQualifiedNameKey(It.IsAny<Assembly>(), It.IsAny<string>()))
                .Returns("SampleSciptKey");

            Mock<IConfigManagerHelper> managerHelperMock = new Mock<IConfigManagerHelper>();

            // Act
            installationsHelperMock.Object.ConfigureLibrarySection(managerHelperMock.Object, this.librariesConfigMock.Object, ContentViewControls, DefaultViewName, DefaultCommandName, null);

            // Assert
            managerHelperMock.Verify(helper => helper.SaveSection(It.IsAny<ConfigSection>(), It.IsAny<bool>()), Times.Once());
        }


        [Test]
        public void ExpectConfigureLibrarySectionToCallSaveSectionsMethodOnceWhenTheCommandWidgetIsMissing()
        {
            // Arrange
            Mock<InstallationsHelper> installationsHelperMock = new Mock<InstallationsHelper>(null);
            installationsHelperMock.CallBase = true;

            installationsHelperMock
                .Setup(helper => helper.AddOrUpdateScriptReference(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<ConfigElementDictionary<string, ClientScriptElement>>()))
                .Returns(false);

            installationsHelperMock
                .Setup(helper => helper.GetJavaScriptQualifiedNameKey(It.IsAny<Assembly>(), It.IsAny<string>()))
                .Returns("SampleSciptKey");

            Mock<IConfigManagerHelper> managerHelperMock = new Mock<IConfigManagerHelper>();

            this.actionMenuWidgetMock.Object.MenuItems[0].Name = "TestCommandWidget";

            // Act
            installationsHelperMock.Object.ConfigureLibrarySection(managerHelperMock.Object, this.librariesConfigMock.Object, ContentViewControls, DefaultViewName, DefaultCommandName, null);

            // Assert
            managerHelperMock.Verify(helper => helper.SaveSection(It.IsAny<ConfigSection>(), It.IsAny<bool>()), Times.Once());

            const int ExpectedCount = 2;
            Assert.AreEqual(ExpectedCount, this.actionMenuWidgetMock.Object.MenuItems.Count);
        }

        [Test]
        public void ExpectConfigureLibrarySectionToCallSaveSectionsMethodTwice()
        {
            // Arrange
            Mock<InstallationsHelper> installationsHelperMock = new Mock<InstallationsHelper>(null);
            installationsHelperMock.CallBase = true;

            installationsHelperMock
                .Setup(helper => helper.AddOrUpdateScriptReference(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<ConfigElementDictionary<string, ClientScriptElement>>()))
                .Returns(true);

            installationsHelperMock
                .Setup(helper => helper.GetJavaScriptQualifiedNameKey(It.IsAny<Assembly>(), It.IsAny<string>()))
                .Returns("SampleSciptKey");

            Mock<IConfigManagerHelper> managerHelperMock = new Mock<IConfigManagerHelper>();

            this.actionMenuWidgetMock.Object.MenuItems[0].Name = "TestCommandWidget";

            // Act
            installationsHelperMock.Object.ConfigureLibrarySection(managerHelperMock.Object, this.librariesConfigMock.Object, ContentViewControls, DefaultViewName, DefaultCommandName, null);

            const int ExpectedCount = 2;
            // Assert
            managerHelperMock.Verify(helper => helper.SaveSection(It.IsAny<ConfigSection>(), It.IsAny<bool>()), Times.Exactly(ExpectedCount));

            Assert.AreEqual(ExpectedCount, this.actionMenuWidgetMock.Object.MenuItems.Count);
        }
        #endregion

        #region GetJavaScriptQualifiedNameKey Tests
        [Test]
        public void ExpectGetJavaScriptQualifiedNameKeyToThrowArgumentNullExceptionWhenAssemblyIsNull()
        {
            InstallationsHelper installationsHelper = new InstallationsHelper(null);
            Assert.Throws<ArgumentNullException>(() =>
            {
                installationsHelper.GetJavaScriptQualifiedNameKey(null, "TestJavaScript");
            });
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void ExpectGetJavaScriptQualifiedNameKeyToThrowArgumentNullExceptionWhenScriptNameIsInvalid(string scriptName)
        {
            InstallationsHelper installationsHelper = new InstallationsHelper(null);
            Assert.Throws<ArgumentNullException>(() =>
            {
                installationsHelper.GetJavaScriptQualifiedNameKey(installationsHelper.GetType().Assembly, scriptName);
            });
        }

        [Test]
        public void ExpectGetJavaScriptQualifiedNameKeyToThrowNullReferenceExceptionWhenScriptNameIsMissing()
        {
            InstallationsHelper installationsHelper = new InstallationsHelper(null);

            Mock<AssemblyStub> assemblyMock = new Mock<AssemblyStub>();
            assemblyMock.Setup(assembly => assembly.GetManifestResourceNames()).Returns(new string[0]);
            string scriptFileName = "TestJavaScript.js";

            Assert.Throws<NullReferenceException>(() =>
            {
                installationsHelper.GetJavaScriptQualifiedNameKey(assemblyMock.Object, scriptFileName);
            },
            $"Cannot find embedded file {scriptFileName}");
        }
        
        [Test]
        public void ExpectGetJavaScriptQualifiedNameKeyToRetrurnCorrectResults ()
        {
            InstallationsHelper installationsHelper = new InstallationsHelper(null);

            Mock<AssemblyStub> assemblyMock = new Mock<AssemblyStub>();
            string fullName = "TestAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            string fullNameSpacescriptFileName = "Sitefinity.Scripts.TestJavaScript.js";
            string scriptFileName = "TestJavaScript.js";
            assemblyMock.Setup(assembly => assembly.GetManifestResourceNames()).Returns(new string[] { fullNameSpacescriptFileName });
            assemblyMock.Setup(assembly => assembly.FullName).Returns(fullName);

            string result =installationsHelper.GetJavaScriptQualifiedNameKey(assemblyMock.Object, scriptFileName);
            Assert.IsNotNull(result);
            Assert.AreEqual($"{fullNameSpacescriptFileName}, {fullName}", result);
        }
        #endregion
    }

    public class ConfigElementListStub<TElement> : ConfigElementList<TElement> where TElement : ConfigElement
    {
        public ConfigElementListStub(ConfigElement parent) : base(parent)
        {
        }

        public override void AddLinkedElement(object key, string path, string moduleName = null)
        {
            // Get element From Dictionary 
            TElement element = (TElement)this.GetElementByKey(key.ToString());

            ConfigElementItem<TElement> configElement  = new ConfigElementItem<TElement>(key.ToString(), element);
            (this.Items as List<ConfigElementItem<TElement>>).Add(configElement);
        }

        public void InsertInDictionary(IConfigElementItem item, string key)
        {
            this.OnItemInserted(item, key);
        }
    }

    public class AssemblyStub : Assembly
    {
    }
}
