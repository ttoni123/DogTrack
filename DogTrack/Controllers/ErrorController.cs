using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DogTrack.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("/error")]
    public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

        if (exceptionHandlerFeature == null)
        {
            return default!;
        }

        var ex = exceptionHandlerFeature.Error;
        var endpoint = exceptionHandlerFeature.Endpoint?.DisplayName ?? "Unknown";

        _logger.LogError(ex, "Operation error in {name} {endpoint}.", hostEnvironment.ApplicationName, endpoint);


        return Problem
        (
            title: ex.Message,
            detail: ex.StackTrace
        );
    }
}
