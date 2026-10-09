using Hotel_Booking.Core.Model;
using System.Linq.Expressions;

namespace Hotel_Booking.Core.Specifications
{
    public abstract class BaseSpecifications<T> : ISpecifications<T> where T : BaseModel
    {
        private readonly List<Expression<Func<T, object>>> _includes = [];
        private readonly List<string> _includesString = [];
        private readonly List<Expression<Func<T, object>>> _thenByExpressions = [];
        private readonly List<Expression<Func<T, object>>> _thenByDescendingExpressions = [];

        protected BaseSpecifications()
        {
        }

        protected BaseSpecifications(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        }

        public Expression<Func<T, bool>>? Criteria { get; private set; }
        public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;
        public IReadOnlyList<string> IncludesString => _includesString;
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDesc { get; private set; }
        public IReadOnlyList<Expression<Func<T, object>>> ThenByExpressions => _thenByExpressions;
        public IReadOnlyList<Expression<Func<T, object>>> ThenByDescendingExpressions => _thenByDescendingExpressions;
        public bool IsTracking { get; private set; }
        public bool IsSplitQuery { get; private set; }
        public int Skip { get; private set; }
        public int Take { get; private set; }
        public bool IsPagination { get; private set; }

        protected void AddCriteria(Expression<Func<T, bool>> criteria)
            => Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
            => _includes.Add(includeExpression ?? throw new ArgumentNullException(nameof(includeExpression)));

        protected void AddInclude(string includePath)
        {
            if (string.IsNullOrWhiteSpace(includePath))
                throw new ArgumentException("Include path cannot be empty.", nameof(includePath));

            _includesString.Add(includePath);
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderBy)
            => OrderBy = orderBy ?? throw new ArgumentNullException(nameof(orderBy));

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescending)
            => OrderByDesc = orderByDescending ?? throw new ArgumentNullException(nameof(orderByDescending));

        protected void ApplyThenBy(Expression<Func<T, object>> thenBy)
            => _thenByExpressions.Add(thenBy ?? throw new ArgumentNullException(nameof(thenBy)));

        protected void ApplyThenByDescending(Expression<Func<T, object>> thenByDescending)
            => _thenByDescendingExpressions.Add(thenByDescending ?? throw new ArgumentNullException(nameof(thenByDescending)));

        protected void ApplyPaging(int skip, int take)
        {
            if (skip < 0)
                throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
            if (take <= 0)
                throw new ArgumentOutOfRangeException(nameof(take), "Take must be greater than zero.");

            Skip = skip;
            Take = take;
            IsPagination = true;
        }

        protected void ApplyPagingByPage(int pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            var skip = checked((pageIndex - 1) * pageSize);
            ApplyPaging(skip, pageSize);
        }

        protected void ApplyTracking() => IsTracking = true;
        protected void ApplyNoTracking() => IsTracking = false;
        protected void ApplySplitQuery() => IsSplitQuery = true;
    }
}
