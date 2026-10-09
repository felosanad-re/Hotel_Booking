namespace Hotel_Booking.Core.Options
{
    public class SeedAdminOptions
    {
        public const string SectionName = "SeedAdmin"; // the section name In Appsitting

        public bool Enabled { get; set; }
        public string FirstName { get; set; } = "Admin";
        public string LastName { get; set; } = "Admin";
        public string UserName { get; set; } = "Admin";
        public string Email { get; set; } = "Admin@Domin.com";
        public string Address { get; set; } = string.Empty;
        public string Password { get; set; } = "Admin1234$";
    }
}
