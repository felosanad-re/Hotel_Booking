namespace Hotel_Booking.API.ErrorHandler
{
    public class ResponseValidationError : ErrorResponse
    {
        public IEnumerable<string>? Errors { get; set; }
        public ResponseValidationError(IEnumerable<string>? errors = null)
            : base(400)
        {
            Errors = errors;
            if (errors != null)
                Message = errors.ToList();
        }
    }
}
