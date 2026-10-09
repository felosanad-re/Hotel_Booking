using Hotel_Booking.Core.Model.Users;

namespace Hotel_Booking.Core.Services.Contract
{
    public interface ICreateToken
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
    }
}
