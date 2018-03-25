namespace Sitefinity.LibraryItemsDownloader.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;

    using Moq;
    using NUnit.Framework;
    using Sitefinity.LibraryItemsDownloader.Helpers;
    using Sitefinity.LibraryItemsDownloader.Services;
    using Sitefinity.LibraryItemsDownloader.Services.Models;
    using Sitefinity.LibraryItemsDownloader.Tests.Services.Stubs;
    using Telerik.Microsoft.Practices.EnterpriseLibrary.Logging;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.Abstractions;
    using Telerik.Sitefinity.GenericContent.Model;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Model;
    using Telerik.Sitefinity.Utilities.Zip;

    public partial class LibraryItemsDownloadServiceTests
    {
        private const string ReturnTestResult = "TestingContent";
        private const string NotFoundExceptionMessage = "The file cannot be found. The server respond with status 404.";

        private Mock<LibraryItemsDownloadService> libraryItemsDownloadService;
        private Mock<ILibraryManagerHelper> managerHelper;
        private Mock<TestLogWriter> testWriterMock;

        [SetUp]
        public void Initialize()
        {
            #region Hack MediaContent.FilePath = value
            Mock<IAppSettings> appSettingsMock = new Mock<IAppSettings>();
            appSettingsMock
                .Setup(settings => settings.Current)
                .Returns(appSettingsMock.Object);

            appSettingsMock
                .Setup(settings => settings.CurrentCulture)
                .Returns(CultureInfo.CurrentCulture);

            typeof(DataExtensions)
                .GetField("appSettings", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .SetValue(null, appSettingsMock.Object);
            #endregion

            #region Hack Log.Writer
            this.testWriterMock = new Mock<TestLogWriter>();
            typeof(Log)
                .GetField("writer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static)
                .SetValue(null, this.testWriterMock.Object);
            #endregion

            Mock<IUtilityHelper> utilityHelperMock = new Mock<IUtilityHelper>();
            utilityHelperMock
                .Setup(helper => helper.ReplaceInvalidCharacters(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((text, replaceChar) => text);

            this.managerHelper = new Mock<ILibraryManagerHelper>();

            this.libraryItemsDownloadService = new Mock<LibraryItemsDownloadService>(utilityHelperMock.Object, this.managerHelper.Object);
            this.libraryItemsDownloadService.CallBase = true;

            this.libraryItemsDownloadService
                .Setup((service) => service.VerifyUserHasPermissionsToAceessService())
                .Verifiable();
        }

        [Test]
        public void ExpectDownloadImagesToReturnCorrectResults()
        {
            // Arrange
            this.libraryItemsDownloadService
                .Setup(this.GetDownloadableContentMethodExpession<Image>())
                .Returns(ReturnTestResult);

            // Act 
            string actualResult = this.libraryItemsDownloadService.Object.DownloadImages(Enumerable.Empty<DownloadLibaryItemRequestModel>());

            // Assert
            Assert.AreEqual(ReturnTestResult, actualResult);
            this.libraryItemsDownloadService
                .Verify(this.GetDownloadableContentMethodExpession<Image>(), Times.Once());
        }

        [Test]
        public void ExpectDownloadVideosToReturnCorrectResults()
        {
            // Arrange
            this.libraryItemsDownloadService
                .Setup(this.GetDownloadableContentMethodExpession<Video>())
                .Returns(ReturnTestResult);

            // Act 
            string actualResult = this.libraryItemsDownloadService.Object.DownloadVideos(Enumerable.Empty<DownloadLibaryItemRequestModel>());

            // Assert
            Assert.AreEqual(ReturnTestResult, actualResult);
            this.libraryItemsDownloadService
                .Verify(this.GetDownloadableContentMethodExpession<Video>(), Times.Once());
        }

        [Test]
        public void ExpectDownloadDocumentsToReturnCorrectResults()
        {
            // Arrange
            this.libraryItemsDownloadService
                .Setup(this.GetDownloadableContentMethodExpession<Document>())
                .Returns(ReturnTestResult);

            // Act 
            string actualResult = this.libraryItemsDownloadService.Object.DownloadDocuments(Enumerable.Empty<DownloadLibaryItemRequestModel>());

            // Assert
            Assert.AreEqual(ReturnTestResult, actualResult);
            this.libraryItemsDownloadService
                .Verify(this.GetDownloadableContentMethodExpession<Document>(), Times.Once());
        }

        #region GetDownloadableContent Tests
        [Test]
        public void ExpectGetDownloadableContentToReturnEmptyStringWhenIdsAreInWrongFormat()
        {
            // Arrange
            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamMethodExpression<Video>())
                .Verifiable();

            ICollection<DownloadLibaryItemRequestModel> requestModels = new List<DownloadLibaryItemRequestModel>();
            requestModels.Add(new DownloadLibaryItemRequestModel() { Id = string.Empty, IsFolder = false });
            requestModels.Add(new DownloadLibaryItemRequestModel() { Id = "1001012312031203", IsFolder = false });
            requestModels.Add(new DownloadLibaryItemRequestModel() { Id = "12312310-213-21233-", IsFolder = false });
            requestModels.Add(new DownloadLibaryItemRequestModel() { Id = "-   -  12313 ", IsFolder = false });
            requestModels.Add(new DownloadLibaryItemRequestModel() { Id = null, IsFolder = false });

            // Act
            string zipFileBinaryData = this.libraryItemsDownloadService.Object.GetDownloadableContent<Video>(requestModels);

            // Assert
            string expectedData = this.GetTestZipStreamBytesAsText();

            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamMethodExpression<Video>(), Times.Once());

            Assert.AreEqual(expectedData, zipFileBinaryData);
        }

        [Test]
        public void ExpectGetDownloadableContentToSaveOneSelectedFile()
        {
            // Arrange
            List<DownloadLibaryItemRequestModel> guids = new List<DownloadLibaryItemRequestModel>();
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97A", IsFolder = false });

            FileZipModel testFileZip = new FileZipModel("TestDataFile", "FileName.txt", string.Empty);

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Video>(guids.Count))
                .Callback<IEnumerable<Guid>, ZipFile, string>((guidIds, zipStream, directory) =>
               {
                   ZipEntry zipEntry = zipStream.AddFileFromString(testFileZip.FileNameWithExtension, testFileZip.DirectoryInZip, testFileZip.Content);
                   testFileZip.LastModified = zipEntry.LastModified;
               });

            // Act
            string result = this.libraryItemsDownloadService.Object.GetDownloadableContent<Video>(guids);

            // Assert
            this.libraryItemsDownloadService
              .Verify(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Video>(guids.Count), Times.Once());

            string expectedData = this.GetTestZipStreamBytesAsText(testFileZip);

            Assert.AreEqual(expectedData, result);
        }

        [Test]
        public void ExpectGetDownloadableContentToSaveOneSelectedFolderAndOneSelectedFile()
        {
            // Arrange
            List<DownloadLibaryItemRequestModel> guids = new List<DownloadLibaryItemRequestModel>();
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97A", IsFolder = false });
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97B", IsFolder = true });

            FileZipModel testFileZip = new FileZipModel("TestDataFile", "FileName.txt", string.Empty);
            FileZipModel testFileInFolderZip = new FileZipModel("var a = 5;", "Documentation.pdf", "TestFolder");

            Mock<IFolder> folderMock = new Mock<IFolder>();
            this.managerHelper
                .Setup(helper => helper.GetFolder(It.IsAny<Guid>()))
                .Returns(folderMock.Object);

            int notFolderItemsCount = guids.Count(model => !model.IsFolder);

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Image>(notFolderItemsCount))
                .Callback<IEnumerable<Guid>, ZipFile, string>((guidIds, zipStream, directory) =>
                 {
                     ZipEntry zipEntry = zipStream.AddFileFromString(testFileZip.FileNameWithExtension, testFileZip.DirectoryInZip, testFileZip.Content);
                     testFileZip.LastModified = zipEntry.LastModified;
                 });

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamRecursively<Image>())
                .Callback<IFolder, ZipFile, string>((folder, zipStream, directory) =>
                {
                    ZipEntry zipEntryFolder = zipStream.AddFileFromString(testFileInFolderZip.FileNameWithExtension, testFileInFolderZip.DirectoryInZip, testFileInFolderZip.Content);
                    testFileInFolderZip.LastModified = zipEntryFolder.LastModified;
                });

            // Act
            string result = this.libraryItemsDownloadService.Object.GetDownloadableContent<Image>(guids);

            // Assert
            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Image>(notFolderItemsCount), Times.Once());

            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamRecursively<Image>(), Times.Once());

            string expectedData = this.GetTestZipStreamBytesAsText(testFileInFolderZip, testFileZip);

            Assert.AreEqual(expectedData, result);
        }

        [Test]
        public void ExpectGetDownloadableContentToSaveSelectedFoldersOnly()
        {
            // Arrange
            List<DownloadLibaryItemRequestModel> guids = new List<DownloadLibaryItemRequestModel>();
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97A", IsFolder = true });
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97B", IsFolder = true });

            FileZipModel testFileFolderZip = new FileZipModel("TestDataFile", "FileName.txt", "RootFolder");
            FileZipModel testFileInFolderZip = new FileZipModel("var a = 5;", "Documentation.pdf", "Categories");

            Mock<IFolder> folderMock = new Mock<IFolder>();
            this.managerHelper
                .Setup(helper => helper.GetFolder(It.IsAny<Guid>()))
                .Returns(folderMock.Object);

            int notFolderItemsCount = guids.Count(model => !model.IsFolder);

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Document>(notFolderItemsCount))
                .Verifiable();

            int calls = 0;

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamRecursively<Document>())
                .Callback<IFolder, ZipFile, string>((folder, zipStream, directory) =>
                {
                    calls++;
                    switch (calls)
                    {
                        case 1:
                            ZipEntry zipEntry = zipStream.AddFileFromString(testFileFolderZip.FileNameWithExtension, testFileFolderZip.DirectoryInZip, testFileFolderZip.Content);
                            testFileFolderZip.LastModified = zipEntry.LastModified;
                            break;
                        case 2:
                            ZipEntry zipEntryFolder = zipStream.AddFileFromString(testFileInFolderZip.FileNameWithExtension, testFileInFolderZip.DirectoryInZip, testFileInFolderZip.Content);
                            testFileInFolderZip.LastModified = zipEntryFolder.LastModified;
                            break;
                        default:
                            break;
                    }
                });

            // Act
            string result = this.libraryItemsDownloadService.Object.GetDownloadableContent<Document>(guids);

            // Assert
            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Document>(notFolderItemsCount), Times.Once());

            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamRecursively<Document>(), Times.Exactly(2));

            string expectedData = this.GetTestZipStreamBytesAsText(testFileFolderZip, testFileInFolderZip);

            Assert.AreEqual(expectedData, result);
        } 
        #endregion

        #region SaveLibraryItemsToStreamRecursively Tests
        [Test]
        public void ExpectGetDownloadableContentToReturnemptyZipArchiveWhenHelperCannotFindTheFolder()
        {
            // Arrange
            List<DownloadLibaryItemRequestModel> guids = new List<DownloadLibaryItemRequestModel>();
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97A", IsFolder = true });
            guids.Add(new DownloadLibaryItemRequestModel() { Id = "55DDA8C6-4270-4092-8196-BF62631FB97B", IsFolder = true });

            this.managerHelper
                .Setup(helper => helper.GetFolder(It.IsAny<Guid>()))
                .Returns<IFolder>(null);

            int notFolderItemsCount = guids.Count(model => !model.IsFolder);

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Document>(notFolderItemsCount))
                .Verifiable();

            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamRecursively<Document>())
                .Verifiable();

            // Act
            string result = this.libraryItemsDownloadService.Object.GetDownloadableContent<Document>(guids);

            // Assert
            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<Document>(notFolderItemsCount), Times.Once());

            this.libraryItemsDownloadService
                .Verify(this.GetSaveLibraryItemsToStreamRecursively<Document>(), Times.Never());

            string expectedData = this.GetTestZipStreamBytesAsText();

            Assert.AreEqual(expectedData, result);
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamRecursivelyToLeftZipArchiveEmptyWhenTheTheNestedFoldersDoNotHaveFiles()
        {
            Lstring mockTitle = "TestTitle";
            Mock<IFolder> folderMock = new Mock<IFolder>();
            folderMock
                .Setup(f => f.Title)
                .Returns(mockTitle);

            // Arrange
            this.managerHelper
                .Setup(helper => helper.GetChildFolders(It.Is<IFolder>(f => f.Title != mockTitle)))
                .Returns(new List<IFolder>() { folderMock.Object }.AsQueryable());

            this.managerHelper
                .Setup(helper => helper.GetChildFolders(It.Is<IFolder>(f => f.Title == mockTitle)))
                .Returns(Enumerable.Empty<IFolder>().AsQueryable());

            this.managerHelper
                .Setup(helper => helper.GetChildItems(It.IsAny<IFolder>()))
                .Returns(Enumerable.Empty<Image>().AsQueryable());

            Mock<IFolder> rootFolderMock = new Mock<IFolder>();
            rootFolderMock
                .Setup(f => f.Title)
                .Returns("Root");

            ZipFile zipStream = new ZipFile();

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStreamRecursively<Image>(rootFolderMock.Object, zipStream, string.Empty);

            // Assert
            const int ExpectedCallsCount = 2;

            Assert.AreEqual(0, zipStream.Entries.Count);
            this.managerHelper
               .Verify(helper => helper.GetChildFolders(It.IsAny<IFolder>()), Times.Exactly(ExpectedCallsCount));

            this.managerHelper
               .Verify(helper => helper.GetChildItems(It.IsAny<IFolder>()), Times.Exactly(ExpectedCallsCount));

            this.managerHelper
                .Verify(helper => helper.Download(It.IsAny<Image>()), Times.Never());
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamRecursivelyToAddOneEntryWithTripleNestedFolders()
        {
            // Arrange
            Lstring rootFolderTitle = "Root";
            Lstring secondLevelTitle = "Second";
            Lstring thirdLevelTitle = "Third";

            Mock<IFolder> rootFolderMock = new Mock<IFolder>();
            rootFolderMock.Setup(f => f.Title).Returns(rootFolderTitle);

            Mock<IFolder> secondLevelMock = new Mock<IFolder>();
            secondLevelMock.Setup(f => f.Title).Returns(secondLevelTitle);

            Mock<IFolder> thirdLevelMock = new Mock<IFolder>();
            thirdLevelMock.Setup(f => f.Title).Returns(thirdLevelTitle);

            this.managerHelper
               .Setup(helper => helper.GetChildFolders(It.Is<IFolder>(f => f.Title == rootFolderTitle)))
               .Returns(new List<IFolder>() { secondLevelMock.Object }.AsQueryable());

            this.managerHelper
               .Setup(helper => helper.GetChildFolders(It.Is<IFolder>(f => f.Title == secondLevelTitle)))
               .Returns(new List<IFolder>() { thirdLevelMock.Object }.AsQueryable());

            this.managerHelper
                .Setup(helper => helper.GetChildFolders(It.Is<IFolder>(f => f.Title == thirdLevelTitle)))
                .Returns(Enumerable.Empty<IFolder>().AsQueryable());

            Lstring videoTitle = "Demo.avi";

            Mock<Video> videoMock = new Mock<Video>();
            videoMock.Setup(v => v.Title).Returns(videoTitle);
            videoMock.Object.Status = ContentLifecycleStatus.Live;
            videoMock.Object.MediaFileLinks.Add(new MediaFileLink() { Culture = CultureInfo.CurrentCulture.LCID });
            videoMock.Object.FilePath = "/One/Two/Three/" + videoTitle;

            this.managerHelper
                .Setup(helper => helper.GetChildItems(It.Is<IFolder>(f => f.Title == thirdLevelTitle)))
                .Returns(new List<Video>() { videoMock.Object }.AsQueryable());

            ZipFile zipStream = new ZipFile();

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStreamRecursively<Video>(rootFolderMock.Object, zipStream, string.Empty);

            // Assert
            string expectedPath = string.Format("{0}/{1}/{2}/{3}", rootFolderTitle.ToString(), secondLevelTitle.ToString(), thirdLevelTitle.ToString(), videoTitle.ToString());
            Assert.AreEqual(1, zipStream.Entries.Count);
            Assert.AreEqual(expectedPath, zipStream.Entries[0].FileName);
            Assert.AreEqual(videoTitle.ToString(), zipStream.Entries[0].LocalFileName);

            const int CallsCount = 3;

            this.managerHelper
                .Verify(helper => helper.GetChildFolders(It.IsAny<IFolder>()), Times.Exactly(CallsCount));

            this.managerHelper
                .Verify(helper => helper.GetChildItems(It.IsAny<IFolder>()), Times.Exactly(CallsCount));
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamRecursivelyToSaveItemsInTheCurrentFolder()
        {
            var list = new List<Document>();

            list.Add(this.GetMockContent<Document>("ReadMe.MD", "/Root", Guid.Empty).Object);
            list.Add(this.GetMockContent<Document>("ReleaseNotes9.1.5600.txt", "/Root", Guid.Empty).Object);
            list.Add(this.GetMockContent<Document>("Configuration.doc", "/Root", Guid.Empty).Object);
            list.Add(this.GetMockContent<Document>("Sitefinity.lic", "/Root", Guid.Empty).Object);

            // Arrange
            Lstring rootFolderTitle = "Root";

            Mock<IFolder> rootFolderMock = new Mock<IFolder>();
            rootFolderMock.Setup(f => f.Title).Returns(rootFolderTitle);

            this.managerHelper
                .Setup(helper => helper.GetChildItems(It.Is<IFolder>(f => f.Title == rootFolderTitle)))
                .Returns(list.AsQueryable());

            ZipFile zipStream = new ZipFile();

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStreamRecursively<Document>(rootFolderMock.Object, zipStream, string.Empty);

            // Assert
            Assert.AreEqual(list.Count, zipStream.Entries.Count);

            this.managerHelper
               .Verify(helper => helper.GetChildItems(It.Is<IFolder>(f => f.Title == rootFolderTitle)), Times.Once());

            this.managerHelper
               .Verify(helper => helper.GetChildFolders(It.Is<IFolder>(f => f.Title == rootFolderTitle)), Times.Once());
        }

        #endregion

        #region SaveLibraryItemsToStream Tests
        [Test]
        public void ExpectSaveLibraryItemsToStreamToNotThrowErrorIfGuidListIsNull()
        {
            ZipFile zipArchive = new ZipFile();

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStream<Document>(null, zipArchive, string.Empty);

            // Assert
            Assert.AreEqual(0, zipArchive.Entries.Count);

            this.managerHelper
                .Verify(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Never());
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamToNotSaveAnyEntries()
        {
            ZipFile zipArchive = new ZipFile();
            List<Guid> guidList = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            // Arrange
            this.managerHelper
                .Setup(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()))
                .Returns(null);

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStream<Document>(guidList, zipArchive, string.Empty);

            // Assert
            Assert.AreEqual(0, zipArchive.Entries.Count);

            this.managerHelper
                .Verify(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Exactly(guidList.Count));
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamToNotAddEntriesInZipAndCallLoggerWhenGetLiveBaseReturnsNull()
        {
            ZipFile zipArchive = new ZipFile();
            List<Guid> contentItemIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            Lstring videoTitle = "TestVideo";
            Guid videoId = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8e");
            Mock<Video> masterVersion = new Mock<Video>();
            masterVersion.Setup(v => v.Title).Returns(videoTitle);
            masterVersion.Setup(v => v.Id).Returns(videoId);

            // Arrange
            this.managerHelper
                .Setup(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()))
                .Returns(masterVersion.Object);

            this.managerHelper
                .Setup(helper => helper.GetLiveBase<Video>(It.IsAny<Video>()))
                .Returns<Video>(null);

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStream<Video>(contentItemIds, zipArchive, string.Empty);

            // Assert
            Assert.AreEqual(0, zipArchive.Entries.Count);

            this.managerHelper
                .Verify(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Exactly(contentItemIds.Count));

            this.managerHelper
                .Verify(helper => helper.GetLiveBase<Video>(It.IsAny<Video>()), Times.Exactly(contentItemIds.Count));

            string traceCategory = Enum.GetName(typeof(ConfigurationPolicy), ConfigurationPolicy.Trace);
            string expectedMessage = $"LibraryItemsDownloader Info: Cannot find the live version of item. Title: { videoTitle.ToString() }, id: { videoId.ToString() }";

            this.testWriterMock
                .Verify(writer => writer.Write(It.Is<LogEntry>(entry => entry.Message == expectedMessage && entry.Categories.Contains(traceCategory))), Times.Exactly(contentItemIds.Count));
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamToNotAddEntriesInZipAndCallLoggerWhenTheContentCannotBeDownloaded()
        {
            ZipFile zipArchive = new ZipFile();
            List<Guid> contentItemIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            Lstring videoTitle = "TestVideo";
            Guid videoId = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8e");
            Mock<Video> masterVersion = new Mock<Video>();
            masterVersion.Setup(v => v.Title).Returns(videoTitle);
            masterVersion.Setup(v => v.Id).Returns(videoId);

            // Arrange
            this.managerHelper
                .Setup(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()))
                .Returns(masterVersion.Object);

            this.managerHelper
                .Setup(helper => helper.GetLiveBase<Video>(It.IsAny<Video>()))
                .Returns(masterVersion.Object);

            this.managerHelper
                .Setup(helper => helper.Download(It.IsAny<Video>()))
                .Throws(new FileNotFoundException(NotFoundExceptionMessage));

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStream<Video>(contentItemIds, zipArchive, string.Empty);

            // Assert
            Assert.AreEqual(0, zipArchive.Entries.Count);

            this.managerHelper
                .Verify(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Exactly(contentItemIds.Count));

            this.managerHelper
                .Verify(helper => helper.GetLiveBase<Video>(It.IsAny<Video>()), Times.Exactly(contentItemIds.Count));

            string traceCategory = Enum.GetName(typeof(ConfigurationPolicy), ConfigurationPolicy.ErrorLog);
            string expectedMessage = $"LibraryItemsDownloader Error: { NotFoundExceptionMessage }{ Environment.NewLine }Item title: \"{ videoTitle.ToString() }\", id: { videoId.ToString() }";

            this.testWriterMock
                .Verify(writer => writer.Write(It.Is<LogEntry>(entry => entry.Message == expectedMessage && entry.Categories.Contains(traceCategory))), Times.Exactly(contentItemIds.Count));
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamToAddAllEntriesCorrectly()
        {
            ZipFile zipArchive = new ZipFile();
            Guid imageId1 = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8e");
            Guid imageId2 = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8a");
            Guid imageId3 = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8b");

            List<Guid> contentItemIds = new List<Guid>() { imageId1, imageId2, imageId3 };

            Lstring imageTitle1 = "TestImage.png";
            Lstring imageTitle2 = "sample.gif";
            Lstring imageTitle3 = "Picture.jpeg";

            Mock<Image> imageMock1 = this.GetMockContent<Image>(imageTitle1, imageTitle1, imageId1);
            Mock<Image> imageMock2 = this.GetMockContent<Image>(imageTitle2, imageTitle2, imageId2);
            Mock<Image> imageMock3 = this.GetMockContent<Image>(imageTitle3, imageTitle3, imageId3);

            int callsCount = 0;

            // Arrange
            this.managerHelper
                .Setup(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    callsCount++;
                    switch (callsCount)
                    {
                        case 1:
                            return imageMock1.Object;
                        case 2:
                            return imageMock2.Object;
                        case 3:
                            return imageMock3.Object;
                        default:
                            return null;
                    }
                });

            // It doesn't matter what this method returns in this case.
            this.managerHelper
                .Setup(helper => helper.GetLiveBase<Image>(It.IsAny<Image>()))
                .Returns(imageMock1.Object);

            Stream testStream = new MemoryStream();
            this.managerHelper
                .Setup(helper => helper.Download(It.IsAny<Image>()))
                .Returns(testStream);

            string directory = "/Rood";

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStream<Image>(contentItemIds, zipArchive, directory);

            // Assert
            Assert.AreEqual(contentItemIds.Count, zipArchive.Entries.Count);

            this.managerHelper
                .Verify(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Exactly(contentItemIds.Count));

            this.managerHelper
                .Verify(helper => helper.GetLiveBase<Image>(It.IsAny<Image>()), Times.Exactly(contentItemIds.Count));
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamToAddEntriesCorrectlyWhenOneOfThemIsMissing()
        {
            ZipFile zipArchive = new ZipFile();
            Guid imageId1 = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8e");
            Guid imageId2 = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8a");
            Guid imageId3 = Guid.Parse("28b01ced-ec89-4ad1-939c-45310b124a8b");

            List<Guid> contentItemIds = new List<Guid>() { imageId1, imageId2, imageId3 };

            Lstring imageTitle1 = "TestImage.png";
            Lstring imageTitle2 = "sample.gif";
            Lstring imageTitle3 = "Picture.jpeg";

            Mock<Image> imageMock1 = this.GetMockContent<Image>(imageTitle1, imageTitle1, imageId1);
            Mock<Image> imageMock2 = this.GetMockContent<Image>(imageTitle2, imageTitle2, imageId2);
            Mock<Image> imageMock3 = this.GetMockContent<Image>(imageTitle3, imageTitle3, imageId3);

            int callsCount = 0;

            // Arrange
            this.managerHelper
                .Setup(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    callsCount++;
                    switch (callsCount)
                    {
                        case 1:
                            return imageMock1.Object;
                        case 2:
                            return imageMock2.Object;
                        case 3:
                            return imageMock3.Object;
                        default:
                            return null;
                    }
                });

            // Always return the not found image.
            this.managerHelper
                .Setup(helper => helper.GetLiveBase<Image>(It.IsAny<Image>()))
                .Returns(imageMock3.Object);

            Stream testStream = new MemoryStream();
            int callCountDownload = 0;
            this.managerHelper
                .Setup(helper => helper.Download(It.IsAny<Image>()))
                .Returns<Image>((image) => 
                {
                    callCountDownload++;
                    if (callCountDownload > 2)
                    {
                        throw new FileNotFoundException(NotFoundExceptionMessage);
                    }

                    return testStream;
                });

            string directory = "/Rood";

            // Act
            this.libraryItemsDownloadService.Object.SaveLibraryItemsToStream<Image>(contentItemIds, zipArchive, directory);

            // Assert
            Assert.AreEqual(contentItemIds.Count - 1, zipArchive.Entries.Count);

            this.managerHelper
                .Verify(helper => helper.GetItem(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Exactly(contentItemIds.Count));

            this.managerHelper
                .Verify(helper => helper.GetLiveBase<Image>(It.IsAny<Image>()), Times.Exactly(contentItemIds.Count));

            string traceCategory = Enum.GetName(typeof(ConfigurationPolicy), ConfigurationPolicy.ErrorLog);
            string expectedMessage = $"LibraryItemsDownloader Error: { NotFoundExceptionMessage }{ Environment.NewLine }Item title: \"{ imageTitle3.ToString() }\", id: { imageId3.ToString() }";

            this.testWriterMock
                .Verify(writer => writer.Write(It.Is<LogEntry>(entry => entry.Message == expectedMessage && entry.Categories.Contains(traceCategory))), Times.Once());
        }
        #endregion

        private Mock<TContent> GetMockContent<TContent>(string title, string path, Guid guid) where TContent : MediaContent
        {
            Lstring contentTile = title;

            Mock<TContent> contentMock = new Mock<TContent>();
            contentMock.Setup(v => v.Title).Returns(contentTile);
            contentMock.Setup(v => v.Id).Returns(guid);
            contentMock.Object.Status = ContentLifecycleStatus.Live;
            contentMock.Object.MediaFileLinks.Add(new MediaFileLink() { Culture = CultureInfo.CurrentCulture.LCID });
            contentMock.Object.FilePath = path + contentTile;

            return contentMock;
        }

        private string GetTestZipStreamBytesAsText(params FileZipModel[] zipModels)
        {
            string result;
            using (MemoryStream ms = new MemoryStream())
            using (ZipFile zipFile = new ZipFile())
            {
                foreach (FileZipModel item in zipModels ?? Enumerable.Empty<FileZipModel>())
                {
                    zipFile.AddFileFromString(item.FileNameWithExtension, item.DirectoryInZip, item.Content);
                    ZipEntry file = zipFile.Entries.FirstOrDefault(z => z.FileName.EndsWith(item.FileNameWithExtension, StringComparison.CurrentCultureIgnoreCase));
                    file.LastModified = item.LastModified;
                }

                zipFile.Save(ms);
                byte[] byteResult = ms.ToArray();
                result = Convert.ToBase64String(byteResult);
            }

            return result;
        }

        private Expression<Func<LibraryItemsDownloadService, string>> GetDownloadableContentMethodExpession<TContent>() where TContent : MediaContent
        {
            return (service) => service.GetDownloadableContent<TContent>(It.IsAny<IEnumerable<DownloadLibaryItemRequestModel>>());
        }

        private Expression<Action<LibraryItemsDownloadService>> GetSaveLibraryItemsToStreamMethodExpression<TContent>() where TContent : MediaContent
        {
            return (service) => service.SaveLibraryItemsToStream<TContent>(It.IsAny<HashSet<Guid>>(), It.IsAny<ZipFile>(), It.IsAny<string>());
        }

        private Expression<Action<LibraryItemsDownloadService>> GetSaveLibraryItemsToStreamMethodExpressionWithListFilter<TContent>(int countOfItems) where TContent : MediaContent
        {
            return (service) => service.SaveLibraryItemsToStream<TContent>(It.Is<HashSet<Guid>>(list => list.Count == countOfItems), It.IsAny<ZipFile>(), It.IsAny<string>());
        }

        private Expression<Action<LibraryItemsDownloadService>> GetSaveLibraryItemsToStreamRecursively<TContent>() where TContent : MediaContent
        {
            return (service) => service.SaveLibraryItemsToStreamRecursively<TContent>(It.IsAny<IFolder>(), It.IsAny<ZipFile>(), It.IsAny<string>());
        }
    }
}