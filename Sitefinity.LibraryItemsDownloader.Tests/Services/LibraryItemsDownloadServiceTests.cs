using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Moq;
using Sitefinity.LibraryItemsDownloader.Services;
using Sitefinity.LibraryItemsDownloader.Services.Models;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Libraries.Model;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Globalization;
using Telerik.Sitefinity.Lifecycle;
using Telerik.OpenAccess.Metadata;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Utilities.Zip;
using Sitefinity.LibraryItemsDownloader.Helpers;
using System.IO;
using Telerik.Sitefinity;
using Telerik.Sitefinity.GenericContent.Model;

namespace Sitefinity.LibraryItemsDownloader.Tests.Services
{
    public class LibraryItemsDownloadServiceTests
    {
        private const string ReturnTestResult = "TestingContent";
        private Mock<LibraryItemsDownloadService> libraryItemsDownloadService;
        private Mock<ILibraryManagerHelper> managerHelper;

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
            #endregion

            typeof(DataExtensions).GetField("appSettings", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .SetValue(null, appSettingsMock.Object);

            Mock<IUtilityHelper> utilityHelperMock = new Mock<IUtilityHelper>();
            utilityHelperMock
                .Setup(helper => helper.ReplaceInvlaidCharacters(It.IsAny<string>(), It.IsAny<string>()))
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

        [Test]
        public void ExpectGetDownloadableContentToReturnEmptyStringWhenIdsAreInWrongFormat()
        {
            // Arrange
            this.libraryItemsDownloadService
                .Setup(this.GetSaveLibraryItemsToStreamMethodExpression<Video>())
                //.Callback(this.DoNothing);
                .Verifiable();

            ICollection<DownloadLibaryItemRequestModel> requestModels = new List<DownloadLibaryItemRequestModel>();
            requestModels.Add(new DownloadLibaryItemRequestModel() { Id = "", IsFolder = false });
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

            const int callsCount = 3;

            this.managerHelper
                .Verify(helper => helper.GetChildFolders(It.IsAny<IFolder>()), Times.Exactly(callsCount));

            this.managerHelper
                .Verify(helper => helper.GetChildItems(It.IsAny<IFolder>()), Times.Exactly(callsCount));
        }

        [Test]
        public void ExpectSaveLibraryItemsToStreamRecursivelyToSaveItemsInTheCurrentFolder()
        {
            var list = new List<Document>();

            list.Add(this.GetMockContent<Document>("ReadMe.MD", "/Root").Object);
            list.Add(this.GetMockContent<Document>("ReleaseNotes9.1.5600.txt", "/Root").Object);
            list.Add(this.GetMockContent<Document>("Configuration.doc", "/Root").Object);
            list.Add(this.GetMockContent<Document>("Sitefinity.lic", "/Root").Object);

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

        public class FileZipModel
        {
            public FileZipModel(string content, string filename, string directory)
            {
                this.Content = content;
                this.FileNameWithExtension = filename;
                this.DirectoryInZip = directory;
            }

            public string Content { get; set; }

            public string FileNameWithExtension { get; set; }

            public string DirectoryInZip { get; set; }

            // Hack field - synchronize the last modified date between result data and expected data
            public DateTime LastModified { get; set; }
        }

        private Mock<TContent> GetMockContent<TContent>(string title, string path) where TContent : MediaContent
        {
            Lstring videoTitle = title;

            Mock<TContent> contentMock = new Mock<TContent>();
            contentMock.Setup(v => v.Title).Returns(videoTitle);
            contentMock.Object.Status = ContentLifecycleStatus.Live;
            contentMock.Object.MediaFileLinks.Add(new MediaFileLink() { Culture = CultureInfo.CurrentCulture.LCID });
            contentMock.Object.FilePath = path + videoTitle;

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
