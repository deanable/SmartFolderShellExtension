using System;
using System.Collections.Generic;
using System.IO;
using SmartFolder.Core;
using Xunit;

namespace SmartFolder.Tests
{
    public class FileOrganizerTests : IDisposable
    {
        private readonly string _testDir;
        private readonly SmartFolderSettings _settings;

        public FileOrganizerTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "SmartFolderTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
            _settings = new SmartFolderSettings();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Fact]
        public void OrganizeFiles_SingleFile_CreatesFolderAndMoves()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "document.txt");
            File.WriteAllText(filePath, "content");
            var organizer = new FileOrganizer(_settings, null, null);

            // Act
            organizer.OrganizeFiles(new[] { filePath });

            // Assert
            string expectedDir = Path.Combine(_testDir, "document");
            string expectedFile = Path.Combine(expectedDir, "document.txt");

            Assert.True(Directory.Exists(expectedDir));
            Assert.True(File.Exists(expectedFile));
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public void OrganizeFiles_MultiFile_FirstItemStrategy()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.jpg");
            File.WriteAllText(file1, "c1");
            File.WriteAllText(file2, "c2");

            _settings.MultiSelectStrategy = MultiSelectStrategy.UseFirstItemName;
            var organizer = new FileOrganizer(_settings, null, null);

            // Act
            organizer.OrganizeFiles(new[] { file1, file2 });

            // Assert
            string expectedDir = Path.Combine(_testDir, "file1");
            Assert.True(Directory.Exists(expectedDir));
            Assert.True(File.Exists(Path.Combine(expectedDir, "file1.txt")));
            Assert.True(File.Exists(Path.Combine(expectedDir, "file2.jpg")));
        }

        [Fact]
        public void OrganizeFiles_MultiFile_PromptStrategy()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.jpg");
            File.WriteAllText(file1, "c1");
            File.WriteAllText(file2, "c2");

            _settings.MultiSelectStrategy = MultiSelectStrategy.AlwaysPrompt;

            var organizer = new FileOrganizer(_settings,
                promptFolderName: (msg) => "MyCustomFolder",
                resolveConflict: null);

            // Act
            organizer.OrganizeFiles(new[] { file1, file2 });

            // Assert
            string expectedDir = Path.Combine(_testDir, "MyCustomFolder");
            Assert.True(Directory.Exists(expectedDir));
            Assert.True(File.Exists(Path.Combine(expectedDir, "file1.txt")));
            Assert.True(File.Exists(Path.Combine(expectedDir, "file2.jpg")));
        }

        [Fact]
        public void OrganizeFiles_Grouping_Date()
        {
             // Arrange
            string file1 = Path.Combine(_testDir, "report.pdf");
            File.WriteAllText(file1, "data");

            // Set file creation time
            var now = DateTime.Now;
            File.SetCreationTime(file1, now);

            _settings.EnableGrouping = true;
            _settings.GroupByYear = true;
            _settings.GroupByMonth = true;

            var organizer = new FileOrganizer(_settings, null, null);

            // Act
            organizer.OrganizeFiles(new[] { file1 });

            // Assert
            // Structure: report/2024/10/report.pdf
            string year = now.Year.ToString();
            string month = now.Month.ToString("00");
            string expectedDir = Path.Combine(_testDir, "report", year, month);
            string expectedFile = Path.Combine(expectedDir, "report.pdf");

            Assert.True(Directory.Exists(expectedDir));
            Assert.True(File.Exists(expectedFile));
        }

        [Fact]
        public void OrganizeFiles_Grouping_Extension()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "image.png");
            File.WriteAllText(file1, "img");

            _settings.EnableGrouping = true;
            _settings.SecondaryGrouping = PropertyGroupingType.Extension;

            var organizer = new FileOrganizer(_settings, null, null);

            // Act
            organizer.OrganizeFiles(new[] { file1 });

            // Assert
            // Structure: image/PNG/image.png
            string expectedDir = Path.Combine(_testDir, "image", "PNG");
            string expectedFile = Path.Combine(expectedDir, "image.png");

            Assert.True(Directory.Exists(expectedDir));
            Assert.True(File.Exists(expectedFile));
        }

        [Fact]
        public void OrganizeFiles_Conflict_Rename()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "doc.txt");
            File.WriteAllText(file1, "new content");

            // Create existing structure
            string targetDir = Path.Combine(_testDir, "doc");
            Directory.CreateDirectory(targetDir);
            string existingFile = Path.Combine(targetDir, "doc.txt");
            File.WriteAllText(existingFile, "old content");

            var organizer = new FileOrganizer(_settings,
                null,
                resolveConflict: (src, dest) => FileOrganizer.ConflictResolution.Rename);

            // Act
            organizer.OrganizeFiles(new[] { file1 });

            // Assert
            Assert.True(File.Exists(existingFile)); // Old file remains
            // Check for renamed file
            string renamedFile = Path.Combine(targetDir, "doc (1).txt");
            Assert.True(File.Exists(renamedFile));
            Assert.Equal("new content", File.ReadAllText(renamedFile));
        }
    }
}
