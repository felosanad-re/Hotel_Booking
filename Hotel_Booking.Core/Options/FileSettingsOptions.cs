namespace Hotel_Booking.Core.Options
{
    public class FileSettingsOptions
    {
        public const string SectionName = "FileSettings"; // the section name in appsettings

        public string FolderName { get; set; } = "Files";
        public int MaxSize { get; set; } = 2_097_152; // 2 MB default
        public HashSet<string> AllowedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> AllowedContentTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
