namespace Sitefinity.LibraryItemsDownloader.Tests.Services.Stubs
{
    using System;

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
}
