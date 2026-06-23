using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VAM.Exceptions;

namespace VAM.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            int statusCode = StatusCodes.Status500InternalServerError;
            object responsePayload;

            if (exception is AppException appException)
            {
                statusCode = appException.StatusCode;
                responsePayload = new
                {
                    message = appException.Message,
                    errorCode = appException.ErrorCode
                };
            }
            else
            {
                responsePayload = new
                {
                    message = exception.Message
                };
            }

            context.Response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(responsePayload);
            return context.Response.WriteAsync(json);
        }
    }
}
