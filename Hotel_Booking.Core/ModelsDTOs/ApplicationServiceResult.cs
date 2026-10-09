namespace Hotel_Booking.Core.ModelsDTOs
{
    public sealed class ApplicationServiceResult<T>
    {
        private ApplicationServiceResult(
            bool succeed,
            T? data,
            string? message,
            IReadOnlyList<string> errors)
        {
            Succeed = succeed;
            Data = data;
            Message = message;
            Errors = errors;
        }

        public bool Succeed { get; }
        public string? Message { get; }
        public IReadOnlyList<string> Errors { get; }
        public T? Data { get; }

        public static ApplicationServiceResult<T> Success(T data, string? message = null)
        {
            ArgumentNullException.ThrowIfNull(data);
            return new(true, data, message, []);
        }

        public static ApplicationServiceResult<T> Success(string? message = null)
            => new(true, default, message, []);

        public static ApplicationServiceResult<T> Fail(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Failure message cannot be empty.", nameof(message));

            return new(false, default, message, []);
        }

        public static ApplicationServiceResult<T> Fail(
            IEnumerable<string> errors,
            string? message = null)
        {
            ArgumentNullException.ThrowIfNull(errors);

            var normalizedErrors = errors
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Select(error => error.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (normalizedErrors.Length == 0)
                throw new ArgumentException("At least one failure error is required.", nameof(errors));

            return new(false, default, message, normalizedErrors);
        }
    }
}
