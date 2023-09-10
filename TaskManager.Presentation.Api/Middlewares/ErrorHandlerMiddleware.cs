using System.Net;
using Newtonsoft.Json;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Wrappers;

namespace TaskManager.Presentation.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                var responseModel = new Response<string>() { Succeeded = false, Message = error?.Message, Data = error.Message };

                switch (error)
                {
                    case Application.Exceptions.ApiException:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case ValidationException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Errors = e.Errors;
                        break;
                    case KeyNotFoundException:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case UnauthorizedAccessException:
                        // unauthorized error
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }
                var result = JsonConvert.SerializeObject(responseModel);

                await response.WriteAsync(result);
            }
        }
    }
}
