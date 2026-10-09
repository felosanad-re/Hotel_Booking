using Microsoft.AspNetCore.Http;

namespace Hotel_Booking.Core.Services.Contract.AttachmentServices
{
    public interface IAttachmentService
    {
        /// <summary>
        /// Upload a single file. Validation rules (folder, size, extensions, content types)
        /// come from the "FileSettings" section in appsettings.
        /// </summary>
        Task<string> UploadAsync(IFormFile file);

        /// <summary>
        /// Upload many files. Validation rules come from the "FileSettings" section in appsettings.
        /// </summary>
        Task<List<string>> UploadsAsync(List<IFormFile> files);

        /// <summary>
        /// Delete a previously uploaded file from the configured folder.
        /// </summary>
        Task DeleteAsync(string fileName);
    }
}
