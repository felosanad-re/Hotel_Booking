using Hotel_Booking.Core.Model;
using Hotel_Booking.Core.Specifications;
using System.Linq.Expressions;

namespace Hotel_Booking.Core.GenericRepo
{
    public interface IGenericRepository<T> where T : BaseModel
    {
        IQueryable<T> GetQuerySpec(ISpecifications<T> spec);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllAsyncSpec(ISpecifications<T> spec, CancellationToken cancellationToken = default);
        Task<T?> GetAsyncSpec(ISpecifications<T> spec, CancellationToken cancellationToken = default);
        Task<int> GetCountAsyncSpec(ISpecifications<T> spec, CancellationToken cancellationToken = default);
        Task<bool> AnyAsyncSpec(ISpecifications<T> spec, CancellationToken cancellationToken = default);
        Task<decimal> GetSumAsyncSpec(ISpecifications<T> spec, Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
