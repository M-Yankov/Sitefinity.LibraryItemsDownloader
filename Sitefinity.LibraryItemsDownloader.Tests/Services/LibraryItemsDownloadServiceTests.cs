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

namespace Sitefinity.LibraryItemsDownloader.Tests.Services
{
    public class LibraryItemsDownloadServiceTests
    {
        private const string ReturnTestResult = "TestingContent";
        private Mock<LibraryItemsDownloadService> libraryItemsDownloadService;
        
        public readonly Action DoNothing = () => { };

        [SetUp]
        public void Initialize()
        {
            Mock<IUtilityHelper> utilityHelperMock = new Mock<IUtilityHelper>();
            Mock<ILibraryManagerHelper> managerHelper = new Mock<ILibraryManagerHelper>();

            this.libraryItemsDownloadService = new Mock<LibraryItemsDownloadService>(utilityHelperMock.Object, managerHelper.Object);
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

        private string GetTestZipStreamBytesAsText(string testData = null)
        {
            string result;
            using (MemoryStream ms = new MemoryStream())
            using (ZipFile zipFile = new ZipFile())
            {
                zipFile.Save(ms);

                byte[] byteResult = ms.ToArray();
                result = Convert.ToBase64String(byteResult);
            }

            return result;
        }

        private Expression<Func<LibraryItemsDownloadService, string>> GetDownloadableContentMethodExpession<TContent>() where TContent: MediaContent
        {
            return (service) => service.GetDownloadableContent<TContent>(It.IsAny<IEnumerable<DownloadLibaryItemRequestModel>>());
        }

        private Expression<Action<LibraryItemsDownloadService>> GetSaveLibraryItemsToStreamMethodExpression<TContent>() where TContent : MediaContent
        {
            return (service) => service.SaveLibraryItemsToStream<TContent>(It.IsAny<HashSet<Guid>>(), It.IsAny<ZipFile>(), It.IsAny<string>()); ;
        }
    }
}
