using Hotel_Booking.Core.GenericRepo;
using Hotel_Booking.Core.Model;

namespace Hotel_Booking.Core.UnitOfWork
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<T> CreateRepository<T>() where T : BaseModel;
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
