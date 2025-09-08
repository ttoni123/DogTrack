using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Net;

namespace DogTrack.Helper
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly JsonSerializerSettings _jsonSetting = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
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

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            ProblemDetails? problemDetails = null;


            problemDetails ??= new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Unexpected Error",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            var traceId = Activity.Current?.Id ?? context?.TraceIdentifier;
            if (traceId != null)
            {
                problemDetails.Instance = traceId;
            }

            var result = JsonConvert.SerializeObject(problemDetails, _jsonSetting);

            context!.Response.ContentType = "application/json";

            context.Response.WriteAsync(result).Wait();

            return Task.CompletedTask;
        }
    }
}
