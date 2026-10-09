using Microsoft.EntityFrameworkCore;
using Hotel_Booking.Core.GenericRepo;
using Hotel_Booking.Core.Model;
using Hotel_Booking.Core.Specifications;
using Hotel_Booking.Repositories.Data;
using Hotel_Booking.Repositories.Specifications;
using System.Linq.Expressions;

namespace Hotel_Booking.Repositories.GenericRepos
{
    public class GenericRepo<T> : IGenericRepository<T> where T : BaseModel
    {
        protected readonly DbContextClass _dbContext;

        public GenericRepo(DbContextClass dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IQueryable<T> GetQuerySpec(ISpecifications<T> spec)
            => ApplySpecifications(spec);

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await BaseQuery().AsNoTracking().ToListAsync(cancellationToken);

        public async Task<T?> GetAsync(int id, CancellationToken cancellationToken = default)
            => await BaseQuery().AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        public async Task<IReadOnlyList<T>> GetAllAsyncSpec(
            ISpecifications<T> spec,
            CancellationToken cancellationToken = default)
            => await ApplySpecifications(spec).ToListAsync(cancellationToken);

        public async Task<T?> GetAsyncSpec(
            ISpecifications<T> spec,
            CancellationToken cancellationToken = default)
            => await ApplySpecifications(spec).FirstOrDefaultAsync(cancellationToken);

        public async Task<int> GetCountAsyncSpec(
            ISpecifications<T> spec,
            CancellationToken cancellationToken = default)
            => await EvaluateSpec<T>.GetQuery(BaseQuery(), spec, applyPaging: false).CountAsync(cancellationToken);

        public async Task<bool> AnyAsyncSpec(
            ISpecifications<T> spec,
            CancellationToken cancellationToken = default)
            => await EvaluateSpec<T>.GetQuery(BaseQuery(), spec, applyPaging: false).AnyAsync(cancellationToken);

        public async Task<decimal> GetSumAsyncSpec(
            ISpecifications<T> spec,
            Expression<Func<T, decimal>> selector,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(selector);
            return await EvaluateSpec<T>.GetQuery(BaseQuery(), spec, applyPaging: false)
                .SumAsync(selector, cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            await _dbContext.Set<T>().AddRangeAsync(entities, cancellationToken);
        }

        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbContext.Set<T>().Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            _dbContext.Set<T>().UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.IsDeleted = true;
            _dbContext.Set<T>().Update(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            foreach (var entity in entities)
                entity.IsDeleted = true;

            _dbContext.Set<T>().UpdateRange(entities);
        }

        private IQueryable<T> BaseQuery()
            => _dbContext.Set<T>().Where(entity => !entity.IsDeleted);

        private IQueryable<T> ApplySpecifications(ISpecifications<T> spec)
        {
            ArgumentNullException.ThrowIfNull(spec);
            return EvaluateSpec<T>.GetQuery(BaseQuery(), spec);
        }
    }
}
