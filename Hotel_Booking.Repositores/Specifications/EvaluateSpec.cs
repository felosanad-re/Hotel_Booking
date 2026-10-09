using Microsoft.EntityFrameworkCore;
using Hotel_Booking.Core.Model;
using Hotel_Booking.Core.Specifications;

namespace Hotel_Booking.Repositories.Specifications
{
    public static class EvaluateSpec<T> where T : BaseModel
    {
        public static IQueryable<T> GetQuery(IQueryable<T> initialQuery, ISpecifications<T> spec, bool applyPaging = true)
        {
            ArgumentNullException.ThrowIfNull(initialQuery);
            ArgumentNullException.ThrowIfNull(spec);

            var query = initialQuery;

            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
            query = spec.IncludesString.Aggregate(query, (current, include) => current.Include(include));

            if (spec.IsSplitQuery)
                query = query.AsSplitQuery();
            if (!spec.IsTracking)
                query = query.AsNoTracking();

            if (spec.OrderBy is not null && spec.OrderByDesc is not null)
                throw new InvalidOperationException("A specification cannot define both ascending and descending primary ordering.");

            IOrderedQueryable<T>? orderedQuery = null;
            if (spec.OrderBy is not null)
                orderedQuery = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDesc is not null)
                orderedQuery = query.OrderByDescending(spec.OrderByDesc);

            if (orderedQuery is not null)
            {
                foreach (var expression in spec.ThenByExpressions)
                    orderedQuery = orderedQuery.ThenBy(expression);
                foreach (var expression in spec.ThenByDescendingExpressions)
                    orderedQuery = orderedQuery.ThenByDescending(expression);

                query = orderedQuery;
            }

            if (applyPaging && spec.IsPagination)
            {
                if (spec.Skip < 0)
                    throw new ArgumentOutOfRangeException(nameof(spec.Skip), "Skip cannot be negative.");
                if (spec.Take <= 0)
                    throw new ArgumentOutOfRangeException(nameof(spec.Take), "Take must be greater than zero.");

                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }
    }
}
