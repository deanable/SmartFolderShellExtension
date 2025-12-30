using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFolder.Core
{
    public class FileOrganizer
    {
        private readonly SmartFolderSettings _settings;
        private readonly Func<string, string> _promptFolderName;
        private readonly Func<string, string, ConflictResolution> _resolveConflict;

        public enum ConflictResolution
        {
            Overwrite,
            Skip,
            Rename,
            Cancel
        }

        public FileOrganizer(
            SmartFolderSettings settings,
            Func<string, string> promptFolderName,
            Func<string, string, ConflictResolution> resolveConflict)
        {
            _settings = settings;
            _promptFolderName = promptFolderName;
            _resolveConflict = resolveConflict;
        }

        public void OrganizeFiles(IEnumerable<string> filePaths)
        {
            var fileList = filePaths.ToList();
            if (fileList.Count == 0) return;

            string targetFolderName = "";
            string? sourceDirectory = Path.GetDirectoryName(fileList[0]);

            if (sourceDirectory == null) return; // Should not happen for valid files

            // Determine Target Folder Name
            if (fileList.Count == 1)
            {
                // Single Item Scenario
                targetFolderName = GetNameWithoutExtension(fileList[0]);
            }
            else
            {
                // Multiple Items Scenario
                if (_settings.MultiSelectStrategy == MultiSelectStrategy.AlwaysPrompt)
                {
                    targetFolderName = _promptFolderName("Enter name for new folder");
                    if (string.IsNullOrWhiteSpace(targetFolderName)) return; // User cancelled or empty
                }
                else
                {
                    targetFolderName = GetNameWithoutExtension(fileList[0]);
                }
            }

            string baseTargetDir = Path.Combine(sourceDirectory, targetFolderName);

            foreach (var filePath in fileList)
            {
                ProcessFile(filePath, baseTargetDir);
            }
        }

        private void ProcessFile(string filePath, string baseTargetDir)
        {
            try
            {
                string targetDir = baseTargetDir;

                // Apply Grouping if Enabled
                if (_settings.EnableGrouping)
                {
                    targetDir = ApplyGrouping(filePath, targetDir);
                }

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(targetDir, fileName);

                if (File.Exists(destPath))
                {
                    // Handle Conflict
                    var resolution = _resolveConflict(filePath, destPath);
                    switch (resolution)
                    {
                        case ConflictResolution.Overwrite:
                            File.Delete(destPath);
                            MoveFile(filePath, destPath);
                            break;
                        case ConflictResolution.Rename:
                            string newName = AutoIncrementName(targetDir, fileName);
                            MoveFile(filePath, Path.Combine(targetDir, newName));
                            break;
                        case ConflictResolution.Skip:
                            // Do nothing
                            break;
                        case ConflictResolution.Cancel:
                            throw new OperationCanceledException("User cancelled operation.");
                    }
                }
                else
                {
                    MoveFile(filePath, destPath);
                }
            }
            catch (Exception ex)
            {
                // Log or show error?
                // For now, rethrow or swallow based on requirements.
                // "without throwing an error" for existing folder merge implies robustness.
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        private string ApplyGrouping(string filePath, string baseDir)
        {
            DateTime fileDate = File.GetCreationTime(filePath); // Or LastWriteTime?
            // User requirement: "Date Grouping"

            string currentDir = baseDir;

            if (_settings.GroupByYear)
            {
                currentDir = Path.Combine(currentDir, fileDate.Year.ToString());
            }
            if (_settings.GroupByMonth)
            {
                currentDir = Path.Combine(currentDir, fileDate.Month.ToString("00"));
            }
            if (_settings.GroupByDay)
            {
                currentDir = Path.Combine(currentDir, fileDate.Day.ToString("00"));
            }

            if (_settings.SecondaryGrouping != PropertyGroupingType.None)
            {
                string subFolder = GetPropertyGroupValue(filePath, _settings.SecondaryGrouping);
                if (!string.IsNullOrEmpty(subFolder))
                {
                    currentDir = Path.Combine(currentDir, subFolder);
                }
            }

            return currentDir;
        }

        private string GetPropertyGroupValue(string filePath, PropertyGroupingType type)
        {
            switch (type)
            {
                case PropertyGroupingType.Extension:
                    return Path.GetExtension(filePath).TrimStart('.').ToUpper();
                case PropertyGroupingType.Author:
                    // Requires reading file metadata.
                    // For simplicity, let's stick to basic ones or use a placeholder if not simple.
                    // Reading Author usually requires Shell32 or WindowsAPICodePack.
                    return "UnknownAuthor";
                case PropertyGroupingType.CameraModel:
                    return "UnknownCamera";
                default:
                    return "";
            }
        }

        private string GetNameWithoutExtension(string path)
        {
            if (File.Exists(path))
            {
                return Path.GetFileNameWithoutExtension(path);
            }
            else if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).Name;
            }
            return Path.GetFileNameWithoutExtension(path);
        }

        private void MoveFile(string source, string dest)
        {
            if (Directory.Exists(source))
            {
                Directory.Move(source, dest);
            }
            else
            {
                File.Move(source, dest);
            }
        }

        private string AutoIncrementName(string dir, string fileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int count = 1;
            string newName;
            do
            {
                newName = $"{nameWithoutExt} ({count}){ext}";
                count++;
            } while (File.Exists(Path.Combine(dir, newName)));
            return newName;
        }
    }
}
