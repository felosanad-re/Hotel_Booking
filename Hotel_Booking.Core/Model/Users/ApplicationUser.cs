using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Booking.Core.Model.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }

        // Date of birth used to calculate Age dynamically
        public DateTime Birthday { get; set; }

        // Computed age in years based on Birthday relative to today's date.
        // Not stored in the database — always calculated at runtime for accuracy.
        public int Age
        {
            get
            {
                var today = DateTime.UtcNow;
                var age = today.Year - Birthday.Year;
                // If the birthday hasn't occurred yet this year, subtract 1
                if (today.Month < Birthday.Month
                    || (today.Month == Birthday.Month && today.Day < Birthday.Day))
                {
                    age--;
                }
                return age;
            }
        }

        // Soft delete flag — separate from Identity's LockoutEnd
        public bool IsDeleted { get; set; }

        //public AccountStatus Status { get; set; } = AccountStatus.Active;

        public DateTime? SuspendedAt { get; set; }

        public string? SuspendedBy { get; set; } // Admin Id

        public string? SuspensionReason { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? DeletedBy { get; set; } // Admin Id

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

    }
}
