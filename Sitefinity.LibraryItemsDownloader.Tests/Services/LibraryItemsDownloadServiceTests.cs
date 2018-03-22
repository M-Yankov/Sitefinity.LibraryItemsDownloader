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

namespace Sitefinity.LibraryItemsDownloader.Tests.Services
{
    public class LibraryItemsDownloadServiceTests
    {
        private const string ReturnTestResult = "TestingContent";
        private Mock<LibraryItemsDownloadService> libraryItemsDownloadService;
        private Mock<ILibraryManagerHelper> managerHelper;

        public readonly Action DoNothing = () => { };

        [SetUp]
        public void Initialize()
        {
            Mock<IUtilityHelper> utilityHelperMock = new Mock<IUtilityHelper>();
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

        }

        [Test]
        public void ExpectGetDownloadableContentTo_WellIDoNotKnowWhatToExpect_WhenOnlyFoldersAreSelectedButTheyDoNoHaveAnything()
        {

        }

        [Ignore("For Later")]
        public void WhenHelperCannotFindTheFolder()
        {

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
