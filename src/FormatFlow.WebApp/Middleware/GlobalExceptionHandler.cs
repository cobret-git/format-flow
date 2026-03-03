using Microsoft.AspNetCore.Diagnostics;

namespace FormatFlow.WebApp.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($">>> UNHANDLED: {exception}");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = exception.Message,
                detail = exception.ToString()
            }, cancellationToken);

            return true; // true = exception is handled, stop propagation
        }
    }
}
