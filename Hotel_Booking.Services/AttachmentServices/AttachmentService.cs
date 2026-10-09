using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Hotel_Booking.Core.Options;
using Hotel_Booking.Core.Services.Contract.AttachmentServices;
using System.Text.RegularExpressions;

namespace Hotel_Booking.Services.AttachmentServices
{
    public class AttachmentService : IAttachmentService
    {
        private static readonly Regex SafeFolderNamePattern = new("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);

        private readonly FileSettingsOptions _fileSettings;

        public AttachmentService(IOptions<FileSettingsOptions> fileSettingsOptions)
        {
            _fileSettings = fileSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(fileSettingsOptions));
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);

            if (file.Length == 0)
                throw new InvalidOperationException("The uploaded file is empty.");

            // Extension
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !_fileSettings.AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file extension.");

            // Content type (defense in depth against renamed files)
            if (_fileSettings.AllowedContentTypes.Count > 0 && !_fileSettings.AllowedContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Invalid file content type.");

            // Size
            if (file.Length > _fileSettings.MaxSize)
                throw new InvalidOperationException("The file exceeds the allowed size.");

            var rootPath = GetRootPath();
            Directory.CreateDirectory(rootPath);

            var fileName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
            var filePath = BuildSafePath(rootPath, _fileSettings.FolderName, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return fileName;
        }

        public async Task<List<string>> UploadsAsync(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                throw new InvalidOperationException("No files uploaded.");

            var fileNames = new List<string>(files.Count);
            foreach (var file in files)
            {
                var fileName = await UploadAsync(file);
                fileNames.Add(fileName);
            }

            return fileNames;
        }

        public Task DeleteAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
                throw new InvalidOperationException("Invalid file name.");

            var filePath = BuildSafePath(GetRootPath(), _fileSettings.FolderName, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        #region Helper Methods
        private static string GetRootPath()
            => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");

        private static string BuildSafePath(string rootPath, string folderName, string fileName)
        {
            var normalizedFolder = NormalizeFolderName(folderName);

            var fullRootPath = Path.GetFullPath(rootPath);
            var fullPath = Path.GetFullPath(Path.Combine(fullRootPath, normalizedFolder, fileName));

            if (!fullPath.StartsWith(fullRootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file path.");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            return fullPath;
        }

        private static string NormalizeFolderName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName) || !SafeFolderNamePattern.IsMatch(folderName))
                throw new InvalidOperationException("Invalid folder name.");
            return folderName;
        }
        #endregion
    }
}
