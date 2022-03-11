using Microsoft.AspNetCore.Mvc;

namespace Generator.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GeneratorController : ControllerBase
{
    private readonly ILogger<GeneratorController> _logger;

    public GeneratorController(ILogger<GeneratorController> logger)
    {
        _logger = logger;
    }

    [HttpPost, Route("/Generator/RunApplication/{ApplicationName}")]
    public IActionResult RunApplication(string ApplicationName, bool Force)
    {
        return Ok(ApplicationName);
    }

    [HttpPost, Route("/Generator/RunBackend/{ApplicationName}")]
    public IActionResult RunBackend(string ApplicationName, bool Force)
    {
        return Ok(ApplicationName);
    }

    [HttpPost, Route("/Generator/RunFrontend")]
    public IActionResult RunFrontend(string ApplicationName, bool Force)
    {
        return Ok(ApplicationName);
    }

    [HttpPost, Route("/Generator/RunEntity/{ApplicationName}/{EntityName}")]
    public IActionResult RunEntity(string ApplicationName, string EntityName, bool Force)
    {
        return Ok(new { ApplicationName, EntityName });
    }

    [HttpPost, Route("/Generator/RunPages/{ApplicationName}/{FrontendName}")]
    public IActionResult RunPages(string ApplicationName, string FrontendName, bool Force)
    {
        return Ok(new { ApplicationName, FrontendName });
    }

    [HttpPost, Route("/Generator/RunComponent/{ApplicationName}/{ComponentName}")]
    public IActionResult RunComponent(string ApplicationName, string ComponentName, bool Force)
    {
        return Ok(new { ApplicationName, ComponentName });
    }

    [HttpPost, Route("/Generator/RunComponents/{ApplicationName}/{FrontendName}")]
    public IActionResult RunComponents(string ApplicationName, string FrontendName, bool Force)
    {
        return Ok(new { ApplicationName, FrontendName });
    }
}
