using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking.API.ErrorHandler
{
    public static class ErrorMessageHandle
    {
        public static IServiceCollection AddErrorMessage(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                    var response = new ResponseValidationError(errors);
                    return new BadRequestObjectResult(response);
                };
            });

            return services;
        }
    }
}
