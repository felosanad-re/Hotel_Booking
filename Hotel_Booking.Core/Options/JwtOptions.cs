namespace Hotel_Booking.Core.Options
{
    public class JwtOptions
    {
        public const string SectionName = "JWT";

        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public double Expires { get; set; } = 1;
        public int ClockSkewMinutes { get; set; } = 5;
        public string Key { get; set; } = string.Empty;
    }
}
