namespace Hotel_Booking.API.ErrorHandler
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public List<string>? Message { get; set; }
        public ErrorResponse(int statusCode, List<string>? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? new List<string> { GetDefaultMessageError(statusCode) ?? "Unexpected Error" };
        }

        private string GetDefaultMessageError(int statusCode)
        {
            return StatusCode switch
            {
                400 => "Invalid request data",
                401 => "Unauthorized access",
                403 => "Access forbidden",
                404 => "Resource not found",
                500 => "Internal server error",
                _ => "Unexpected error occurred"
            };
        }
    }
}
