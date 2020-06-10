using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Serilog;

namespace TestApi.Middleware
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private ILogger _log;

        public GlobalExceptionFilter(ILogger log)
        {
            _log = log;
        }

        public override void OnException(ExceptionContext context)
        {

            _log.Error(context.Exception, $"Message: {context.Exception.Message}");

            var statusCode = HttpStatusCode.OK;
            string errorId = Guid.NewGuid().ToString();

            switch (context.Exception)
            {
                case NotImplementedException niEx:
                    // case NotFoundException nfEx:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case ArgumentOutOfRangeException aorEx:
                case ArgumentNullException anEx:
                case ArgumentException aex:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case UnauthorizedAccessException uaEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                case TimeoutException toEx:
                    statusCode = HttpStatusCode.ServiceUnavailable;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            context.HttpContext.Response.Headers["cache-control"] = "no-cache, no-store, must-revalidate";

            context.Result = new ContentResult()
            {
                StatusCode = (int)statusCode,
                Content = JsonConvert.SerializeObject(new
                {
                    error = true,
                    id = errorId,
                    message = "Someting went wromg while processing your request"
                }),
                ContentType = "application/json"
            };

            base.OnException(context);
        }

    }
}