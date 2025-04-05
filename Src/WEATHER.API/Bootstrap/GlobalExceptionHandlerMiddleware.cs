using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace WEATHER.API.Bootstrap
{
    public class GlobalExceptionHandlerMiddleware: IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // Handle the exception
                var logger = context.GetLogger("GlobalExceptionHandler");
                logger.LogError(ex, "An unhandled exception occurred.");
                throw;
                //other logic
                //wrapup exception to hide some details from client
                //alo we could add custom exceptions
            }
        }
    }
}
